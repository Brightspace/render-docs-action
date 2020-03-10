using System;
using System.Collections.Immutable;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace D2L.Dev.Docs.Render.Markdown {
	internal static class Extensions {
		private static readonly ImmutableArray<string> HEADINGS = ImmutableArray.Create(
			"d2l-heading-1",
			"d2l-heading-2",
			"d2l-heading-3",
			"d2l-heading-4",
			"d2l-heading-5",
			"d2l-heading-6"
		);

		public static void StyleHeadingsForD2L( this MarkdownDocument @this ) {
			var headings = @this.Descendants<HeadingBlock>();

			foreach( var heading in headings ) {
				var level = Math.Min( HEADINGS.Length, heading.Level );
				heading.GetAttributes().AddClass( HEADINGS[level - 1] );
			}
		}

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
			@this.StyleHeadingsForD2L();
			@this.StyleParagraphsForD2L();
			@this.StyleCodeForD2L();			
		}
	}
}
