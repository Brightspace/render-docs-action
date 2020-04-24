using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;
using System;
using System.Text;

namespace D2L.Dev.Docs.Render.Markdown {
	/// <summary>An HtmlObjectRenderer for D2L links</summary>
	/// <remarks>This is complicated; but it's mostly a copy-and paste of the
	/// default one. The only signficant difference is that we use d2l-link
	/// instead of a.</remarks>
	internal sealed class D2LLinkInlineRenderer : HtmlObjectRenderer<LinkInline> {

		private readonly DocumentContext m_docContext;

		public D2LLinkInlineRenderer( DocumentContext context ) {
			m_docContext = context;
		}

		protected override void Write(
			HtmlRenderer renderer,
			LinkInline link
		) {
			if ( renderer.EnableHtmlForBlock ) {
				if ( link.IsImage ) {
					renderer.Write( "<img src=\"" );
				} else {
					renderer.Write( "<d2l-link href=\"" );
				}

				string url = link.GetDynamicUrl?.Invoke() ?? link.Url;
				renderer.WriteEscapeUrl( RewriteLink( url, m_docContext ) );

				renderer.Write( "\"" );
				renderer.WriteAttributes( link );
			}

			if ( link.IsImage ) {
				if ( renderer.EnableHtmlForInline ) {
					renderer.Write( " alt=\"" );
				}

				var wasEnableHtmlForInline = renderer.EnableHtmlForInline;
				renderer.EnableHtmlForInline = false;
				renderer.WriteChildren( link );
				renderer.EnableHtmlForInline = wasEnableHtmlForInline;

				if ( renderer.EnableHtmlForInline ) {
					renderer.Write( "\"" );
				}
			}


			if ( renderer.EnableHtmlForInline && !string.IsNullOrEmpty( link.Title ) ) {
				renderer.Write( " title=\"" );
				renderer.WriteEscape( link.Title );
				renderer.Write( "\"" );
			}

			if ( link.IsImage && renderer.EnableHtmlForInline ) {
				renderer.Write( " />" );
			} else {
				if ( renderer.EnableHtmlForInline ) {
					renderer.Write( ">" );
				}

				renderer.WriteChildren( link );

				if ( renderer.EnableHtmlForInline ) {
					renderer.Write( "</d2l-link>" );
				}
			}
		}

		private string RewriteLink( string url, DocumentContext context ) {
			string rewritten = url;

			if( url.StartsWith( "/" ) && context.SubModule != null ) {
				rewritten = $"/{context.SubModule.RepoName}{rewritten}";
			}

			if ( rewritten.EndsWith( ".md" ) ) {
				rewritten = rewritten.Substring( 0, rewritten.Length - 3 ) + ".html";
			}

			return rewritten;
		}

	}
}
