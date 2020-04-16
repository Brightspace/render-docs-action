using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using D2L.Dev.Docs.Render.Markdown;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

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

			var directories = Directory.EnumerateFiles( input, "*", SearchOption.AllDirectories );
			foreach ( var filename in directories ) {
				await DoFile( new RelativeFile( input, output, filename ) );
			}

			return 0;
		}

		private static async Task DoFile( RelativeFile file ) {
			if ( file.Extension != "md" ) {
				return;
			}
			Console.Error.WriteLine( $"Working on {file.Name}.{file.Extension}" );

			string name = file.Name;
			if ( name.ToUpperInvariant() == "README" ) {
				name = "index";
			}

			var text = await file.Read();

			// See https://help.github.com/en/actions/configuring-and-managing-workflows/using-environment-variables#default-environment-variables
			var repoName = Environment.GetEnvironmentVariable( "GITHUB_REPOSITORY" ).Split('/')[1];

			var context = new DocumentContext( repoName );
			var doc = MarkdownFactory.Parse( text );
			doc.ApplyD2LTweaks();
			var html = MarkdownFactory.RenderToString( doc, context );

			var renderer = TemplateRenderer.CreateFromResource( "Templates.page.html" );
			var formatted = await renderer.RenderAsync(
				title: GetTitle( doc ),
				content: html
			);

			await file.Write( formatted, name + ".html" );
			CopyAssociatedFiles( doc, file.SourceRoot, file.DestinationRoot );
		}

		private static void CopyAssociatedFiles( MarkdownDocument doc, string input, string output ) {
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

				url = Path.Join( input, url );
				if ( !File.Exists( url ) ) {
					Console.WriteLine( $"Could not find file '{url}', skipping" );
					continue;
				}

				// TODO: Handle "absolute" urls, e.g. start with "/"

				new RelativeFile( input, output, url ).Copy();
				Console.WriteLine( $"Copied file '{url}'" );
			}
		}

		private static string GetTitle( MarkdownDocument doc ) {
			var inline = doc.Descendants<HeadingBlock>().Single(
				h => h.Level == 1
			).Inline;
			var titleLiteral = inline.Descendants<LiteralInline>().Single();
			return titleLiteral.Content.ToString();
		}

	}
}
