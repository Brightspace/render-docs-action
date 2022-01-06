using D2L.Dev.Docs.Render.VFS;

namespace D2L.Dev.Docs.Render.Markdown {
	// TODO: Add more properties for context
	// Some possibilities could be github repo url, list of docs repos
	internal sealed class DocumentContext {

		public string InputDirectory { get; }
		public string OutputDirectory { get; }
		public string DocRootRepoName { get; }
		public string Branch { get; }
		public string DocsPath { get; }
		public string RepoRoot { get; }
		public string? TemplatePath { get; }

		public DocumentContext( string inputDir, string outputDir, string docRootRepoName, string defaultBranch, string docsPath, string repoRoot, string? templatePath ) {
			InputDirectory = inputDir;
			OutputDirectory = outputDir;
			DocRootRepoName = docRootRepoName;
			Branch = defaultBranch;
			DocsPath = docsPath;
			RepoRoot = repoRoot;
			TemplatePath = templatePath;
		}
	}
}
