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

			foreach ( var filename in Directory.GetFiles( input ) ) {
				await DoFile( filename, output );
			}

			return 0;
		}

		private static async Task DoFile( string filename, string outputDirectory ) {
			var (name, ext) = GetNameAndExtension( filename );

			if ( ext != "md" ) {
				Console.Error.WriteLine( "{0} : Ignoring file not ending in .md", filename );
				return;
			}

			if ( name == "README" ) {
				name = "index";
			}

			using var outputHtml = GetOutput( outputDirectory, name, ".html" );

			var text = await File.ReadAllTextAsync( filename );

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

			outputHtml.Write( formatted );

			CopyAssociatedFiles( doc, outputDirectory );
		}

		private static void CopyAssociatedFiles( MarkdownDocument doc, string outputDirectory ) {
			var links = doc.Descendants().OfType<LinkInline>();
                        
			foreach( var link in links ) {
				if ( link.Url.EndsWith( ".md" ) ) {
					continue;
				}
				// Skip any URL which has a scheme
				if ( Uri.IsWellFormedUriString( link.Url, UriKind.Absolute ) ) {
					continue;
				}

				// TODO: Handle "absolute" urls, e.g. start with "/"
				
				CopyFileKeepingRelativePath(
					filepath: link.Url,
					outputDirectoryRoot: outputDirectory
				);
			}
		}

		private static void CopyFileKeepingRelativePath( string filepath, string outputDirectoryRoot ) {
			string outputPath = Path.Combine( outputDirectoryRoot, filepath );
			string parent = Directory.GetParent( outputPath ).FullName;
			
			if ( File.Exists( outputPath ) ) {
				return;
			}
			if ( !Directory.Exists( parent ) ) {
				Directory.CreateDirectory( parent );
			}
			File.Copy( filepath, outputPath );
		}

		private static string GetTitle( MarkdownDocument doc ) {
			var inline = doc.Descendants<HeadingBlock>().Single(
				h => h.Level == 1
			).Inline;
			var titleLiteral = inline.Descendants<LiteralInline>().Single();
			return titleLiteral.Content.ToString();
		}

		private static (string, string) GetNameAndExtension( string input ) {
			var idx = input.LastIndexOf( '.' );
			return (input.Substring( 0, idx ), input.Substring( idx + 1 ) );
		}

		private static TextWriter GetOutput( string outputDir, string name, string ext ) {
			var path = Path.Combine( outputDir, name + ext );
			var parent = Directory.GetParent( path );
			if( !parent.Exists ) {
				parent.Create();
			}

			return new StreamWriter( path );
		}
	}
}
