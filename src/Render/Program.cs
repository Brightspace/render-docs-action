using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using D2L.Dev.Docs.Render.Markdown;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using D2L.Dev.Docs.Render.VFS;

namespace D2L.Dev.Docs.Render {
	internal static class Program {
		/// <param name="input">The path to a directory of markdown files to render</param>
		/// <param name="output">The directory to put the rendered html files</param>
		public static async Task<int> Main( string input, string output ) {
			if ( input == null || output == null ) {
				Console.Error.WriteLine( "--input and --output arguments are required" );
				return -1;
			}

			if( !Directory.Exists( input ) ) {
				Console.Error.WriteLine( "input must be a  directory containing markdown (.md) files" );
				return -1;
			}

			if( !Directory.Exists( output ) ) {
				Directory.CreateDirectory( output );
			}

			// See https://help.github.com/en/actions/configuring-and-managing-workflows/using-environment-variables#default-environment-variables
			var repoName = Environment.GetEnvironmentVariable( "GITHUB_REPOSITORY" )?.Split( '/' )[1] ?? "";
			var context = new DocumentContext( input, output, repoName );
			var directories = Directory.EnumerateFiles( input, "*", SearchOption.AllDirectories );
			foreach ( var filename in directories ) {
				var file = GetOutput(context, filename);
				await DoFile( context, file );
			}

			return 0;
		}

		private static async Task DoFile( DocumentContext context, RelativeFile file ) {
			if ( file.Source.Extension != "md" ) {
				return;
			}
			Console.Error.WriteLine( $"Working on {file.Source.FullPath}" );

			var text = await file.Read();

			var doc = MarkdownFactory.Parse( text );
			doc.ApplyD2LTweaks();
			var html = MarkdownFactory.RenderToString( doc, context );

			var renderer = TemplateRenderer.CreateFromResource( "Templates.page.html" );
			var formatted = await renderer.RenderAsync(
				title: GetTitle( doc ),
				content: html,
				editLink: file.EditSourceLink
			);

			await file.Write( formatted );
			CopyAssociatedFiles( doc, context, file.Source.Path );
		}

		private static void CopyAssociatedFiles( MarkdownDocument doc, DocumentContext context, string directory ) {
			var links = doc.Descendants().OfType<LinkInline>();
                        
			foreach( var link in links ) {
				string url = link.GetDynamicUrl?.Invoke() ?? link.Url;

				if ( url.EndsWith( ".md" ) || url.StartsWith("#") ) {
					continue;
				}
				// Skip any URL which has a scheme
				if ( Uri.IsWellFormedUriString( url, UriKind.Absolute ) ) {
					continue;
				}

				url = Path.Join( directory, url );
				if ( !File.Exists( url ) ) {
					Console.WriteLine( $"Could not find file '{url}', skipping" );
					continue;
				}

				// TODO: Handle "absolute" urls, e.g. start with "/"

				GetOutput( context, url ).Copy();
				Console.WriteLine( $"Copied file '{url}'" );
			}
		}

		private static string GetTitle( MarkdownDocument doc ) {
			var inline = doc.Descendants<HeadingBlock>().SingleOrDefault(
				h => h.Level == 1
			);
			if( inline == default ) {
				throw new ArgumentException( "Document should have exactly one level-1 heading" );
			}
			var titleLiteral = inline.Inline.Descendants<LiteralInline>().SingleOrDefault();
			if ( titleLiteral == default ) {
				throw new ArgumentException( "The level-1 heading should be unformatted." );
			}
			return titleLiteral.Content.ToString();
		}

		private static RelativeFile GetOutput( DocumentContext context, string path ) {
			var infile = new FileInfo( path );
			string name = (infile.Name == "README") ? "index" : infile.Name;
			string ext = (infile.Extension == "md") ? "html" : infile.Extension;

			string relparent = Path.GetRelativePath( context.InputDirectory, infile.Path );
			string dstpath = Path.Join( context.OutputDirectory, relparent, $"{name}.{ext}" );
			var outfile = new FileInfo( dstpath );

			string relativePath = Path.GetRelativePath( context.InputDirectory, path );
			Uri editSourceUri = new Uri(
				$"https://github.com/Brightspace/{context.DocRootRepoName}/edit/master/{relativePath}",
				UriKind.Absolute
			);

			return new RelativeFile( context, infile, outfile, editSourceUri );
		}
	}
}
