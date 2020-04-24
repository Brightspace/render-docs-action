using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace D2L.Dev.Docs.Render.Markdown {
	internal static class Extensions {

		public static void StyleParagraphsForD2L( this MarkdownDocument @this ) {
			var ps = @this.Descendants<ParagraphBlock>();

			foreach( var p in ps ) { 
				p.GetAttributes().AddClass( "d2l-body-standard" );
			}
		}

		public static void StyleCodeForD2L( this MarkdownDocument @this ) {
			var pres = @this.Descendants<CodeBlock>();
			
			foreach( var pre in pres ) {
				pre.GetAttributes().AddClass( "d2l-body-small" );
			}	
		}

		public static void ApplyD2LTweaks( this MarkdownDocument @this ) {
			@this.StyleParagraphsForD2L();
			@this.StyleCodeForD2L();			
		}
	}
}
