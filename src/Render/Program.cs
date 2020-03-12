using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using D2L.Dev.Docs.Render.Markdown;

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

			var doc = MarkdownFactory.Parse( text );
			doc.ApplyD2LTweaks();
			var html = MarkdownFactory.RenderToString( doc );

			var renderer = TemplateRenderer.CreateFromResource( "Templates.page.html" );
			var formatted = await renderer.RenderAsync(
				title: "TODO: Get Title",
				content: html
			);

			outputHtml.Write( formatted );
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
