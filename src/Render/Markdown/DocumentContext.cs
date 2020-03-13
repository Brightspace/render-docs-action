namespace D2L.Dev.Docs.Render.Markdown {
	// TODO: Add more properties for context
	// Some possibilities could be github repo url, list of docs repos
	internal sealed class DocumentContext {

		public string DocRootRepoName { get; }

		public DocumentContext( string docRootRepoName ) {
			DocRootRepoName = docRootRepoName;
		}
	}
}
