using Scriban;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace D2L.Dev.Docs.Render.Markdown {
	internal static class TemplateRenderer {
	
		public static async Task<string> RenderAsync( 
			string templateFileName, 
			IDictionary<string, string> args 
		) {
			var html = await File.ReadAllTextAsync( templateFileName );
			var template = Template.Parse( html );

			return await template.RenderAsync( args );
		}

	}
}
