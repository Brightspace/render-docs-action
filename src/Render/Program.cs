using System;
using System.IO;
using System.Threading.Tasks;
using D2L.Dev.Docs.Render.Markdown;
using Markdig.Parsers;

namespace D2L.Dev.Docs.Render {
	internal static class Program {
		/// <param name="input">The path to a markdown (.md) file to render</param>
		/// <param name="pullRequestCommentsPath">The path (on https://github.com) to send diagnostics as comments</param>
		public static async Task<int> Main(
			string input,
			string pullRequestCommentsPath
		) {
			if ( input == null ) {
				Console.Error.WriteLine( "Missing --input argument" );
				return -1;
			}

			var (name, ext) = GetNameAndExtension( input );

			if ( ext != "md" ) {
				Console.Error.WriteLine( "Input should be a file ending in .md" );
				return -1;
			}

			if ( name == "README" ) {
				name = "index";
			}

			var outputDir = Path.GetDirectoryName( input );

			if ( outputDir == null ) {
				Console.Error.WriteLine( "Couldn't get output dir");
				return -1;
			}

			using var outputHtml = GetOutput( outputDir, name, ".html");

			var text = await File.ReadAllTextAsync( input );


			var doc = MarkdownFactory.Parse( text );

			doc.ApplyD2LTweaks();

			MarkdownFactory.Render( outputHtml, doc );
			
			return 0;
		}

		private static (string, string) GetNameAndExtension( string input ) {
			var idx = input.LastIndexOf( '.' );
			return (input.Substring( 0, idx ), input.Substring( idx + 1 ) );
		} 

		private static TextWriter GetOutput( string outputDir, string name, string ext ) {
			return new StreamWriter(
				Path.Combine( outputDir, name + ext )
			);
		}
	}
}
