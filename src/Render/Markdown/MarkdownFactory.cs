using System.IO;
using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax;

namespace D2L.Dev.Docs.Render.Markdown {
	internal static class MarkdownFactory {
		public static MarkdownDocument Parse( string text ){
			var pipeline = CreatePipeline();

			return MarkdownParser.Parse(
				text,
				pipeline,
				context: null
			);
		}

		public static void Render( TextWriter outputHtml, MarkdownDocument doc, DocumentContext context ) {
			var renderer = CreateRenderer( outputHtml, context );
			renderer.Render( doc );
		}

		public static string RenderToString( MarkdownDocument doc, DocumentContext context ) {
			using var writer = new StringWriter();
			var renderer = CreateRenderer( writer, context );
			renderer.Render( doc );
			return writer.ToString();
		}

		public static MarkdownPipeline CreatePipeline() =>
			new MarkdownPipelineBuilder()
				// We could explore disabling this and re-parsing only if we have errors.
				.UsePreciseSourceLocation()

				// Third-party extensions
				.UseDefinitionLists()
				.UseFigures()
				.UseFootnotes()
				.UsePipeTables()
				.UseAutoIdentifiers()
				.UseYamlFrontMatter()
				
				.Build();

		public static IMarkdownRenderer CreateRenderer( TextWriter writer, DocumentContext context ) {
			var renderer = new HtmlRenderer( writer );
	
			renderer.ObjectRenderers.Replace<LinkInlineRenderer>(
				new D2LLinkInlineRenderer( context )
			); 

			renderer.ObjectRenderers
				.Find<CodeBlockRenderer>()
				.OutputAttributesOnPre = true;
	
			return renderer;
		}
	}
}
