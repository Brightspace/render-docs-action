using Scriban;
using System.Threading.Tasks;

namespace D2L.Dev.Docs.Render.Markdown {
	internal class TemplateRenderer {
		private readonly Template m_template;
		
		public TemplateRenderer( string template ) {
			m_template = Template.Parse( template );
		}

		public async Task<string> RenderAsync( string title, string content ) {
			return await m_template.RenderAsync( new {
				title,
				content
			} );
		}

	}
}
