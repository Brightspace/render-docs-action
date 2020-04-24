using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System;
using System.Collections.Immutable;

namespace D2L.Dev.Docs.Render.Markdown {
	internal sealed class HeadingLinkRenderer : HtmlObjectRenderer<HeadingBlock> {

        private static readonly ImmutableArray<string> D2lHeadings = ImmutableArray.Create(
            "d2l-heading-1",
            "d2l-heading-2",
            "d2l-heading-3",
            "d2l-heading-4",
            "d2l-heading-5",
            "d2l-heading-6"
        );

        private const string linkTemplate = "<a class=\"doc-fragment\" href=\"#{id}\" aria-hidden=\"true\">" + 
            "<d2l-icon icon=\"tier2:link\"></d2l-icon></a>";

        protected override void Write( HtmlRenderer renderer, HeadingBlock obj ) {
            int index = obj.Level - 1;
            int d2lHeadingId = Math.Min( index, D2lHeadings.Length );
            string headingText = "h" + obj.Level.ToString();
            var attributes = obj.GetAttributes();
            
            attributes.AddClass( D2lHeadings[d2lHeadingId] );
            attributes.AddClass( "doc-fragment-heading" );

            if ( renderer.EnableHtmlForBlock ) {
                renderer.Write( "<" ).Write( headingText ).WriteAttributes( obj ).Write( ">" );
                renderer.Write( linkTemplate.Replace("{id}", attributes.Id ) );
            }

            renderer.WriteLeafInline( obj );

            if ( renderer.EnableHtmlForBlock ) {
                renderer.Write( "</" ).Write( headingText ).WriteLine( ">" );
            }

            renderer.EnsureLine();
        }
	}
}
