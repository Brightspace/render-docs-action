using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using D2L.Dev.Docs.Render.Markdown;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using D2L.Dev.Docs.Render.VFS;

namespace D2L.Dev.Docs.Render {
	internal static class Program {
		/// <param name="repoRoot">The path to repo root directory.</param>
		/// <param name="output">The directory to put the rendered html files.</param>
		/// <param name="docsPath">The path within the repo containing the docs files to be rendered.</param>
		public static async Task<int> Main( string repoRoot, string output, string docsPath ) {
			if( repoRoot == null || output == null || docsPath == null ) {
				Console.Error.WriteLine( "--repo-root, --output, and --docs-path arguments are required" );
				return -1;
			}

			string input = Path.Combine( repoRoot, docsPath );

			if( !Directory.Exists( input ) ) {
				Console.Error.WriteLine( "input must be a  directory containing markdown (.md) files" );
				return -1;
			}

			if( !Directory.Exists( output ) ) {
				Directory.CreateDirectory( output );
			}

			string docsPathSanitized = GetSanitizedDocsPath( docsPath );

			// See https://help.github.com/en/actions/configuring-and-managing-workflows/using-environment-variables#default-environment-variables
			var repoName = Environment.GetEnvironmentVariable( "GITHUB_REPOSITORY" )?.Split( '/' )[1] ?? "";
			var context = new DocumentContext( input, output, repoName, GetBranch(), docsPathSanitized );
			var directories = Directory.EnumerateFiles( input, "*", SearchOption.AllDirectories );
			foreach ( var filename in directories ) {
				var file = GetOutput(context, filename);
				await DoFile( context, file );
			}

			return 0;
		}

		private static string GetSanitizedDocsPath( string docsPath ) {
			string docsPathSanitized;
			if( docsPath == "." ) {
				docsPathSanitized = "";
			} else {
				// Remove leading slashes and ensure there's only one trailing slash
				docsPathSanitized = docsPath.TrimStart( '/' ).TrimEnd( '/' ) + '/';
			}

			return docsPathSanitized;
		}

		private static string GetBranch() {
			return "master";

			//// See https://help.github.com/en/actions/configuring-and-managing-workflows/using-environment-variables#default-environment-variables

			//var gitRef = Environment.GetEnvironmentVariable( "GITHUB_REF" );

			//if( gitRef == null ) {
			//	return "master";
			//}

			//const string refsHeads = "refs/heads/";

			//// I'm not clear on all the possible values of this envvar, so for
			//// now I'll handle the expected value (refs/heads/<branch>) and
			//// fail fast for other values
			//if( !gitRef.StartsWith( refsHeads ) ) {
			//	throw new ArgumentException( $"unexpected REF, {gitRef}", nameof( gitRef ) );
			//}

			//return gitRef.Substring( refsHeads.Length );
		}

		private static async Task DoFile( DocumentContext context, RelativeFile file ) {
			if ( file.Source.Extension != "md" ) {
				return;
			}
			Console.Error.WriteLine( $"Working on {file.Source.FullPath}" );

			var text = await file.Read();

			var doc = MarkdownFactory.Parse( text );
			doc.ApplyD2LTweaks();
			var html = MarkdownFactory.RenderToString( doc, context );

			var renderer = TemplateRenderer.CreateFromResource( "Templates.page.html" );
			var formatted = await renderer.RenderAsync(
				title: GetTitle( doc ),
				content: html,
				editLink: file.EditSourceLink
			);

			await file.Write( formatted );
			CopyAssociatedFiles( doc, context, file.Source.Path );
		}

		private static void CopyAssociatedFiles( MarkdownDocument doc, DocumentContext context, string directory ) {
			var links = doc.Descendants().OfType<LinkInline>();
                        
			foreach( var link in links ) {
				string url = link.GetDynamicUrl?.Invoke() ?? link.Url;

				if ( url.EndsWith( ".md" ) || url.StartsWith("#") ) {
					continue;
				}
				// Skip any URL which has a scheme
				if ( Uri.IsWellFormedUriString( url, UriKind.Absolute ) ) {
					continue;
				}

				url = Path.Join( directory, url );
				if ( !File.Exists( url ) ) {
					Console.WriteLine( $"Could not find file '{url}', skipping" );
					continue;
				}

				// TODO: Handle "absolute" urls, e.g. start with "/"

				GetOutput( context, url ).Copy();
				Console.WriteLine( $"Copied file '{url}'" );
			}
		}

		private static string GetTitle( MarkdownDocument doc ) {

			HeadingBlock headingBlock = GetSingleTitleOrThrow( doc );

			LiteralInline titleLiteral = GetUnformattedContentOrThrow( headingBlock );

			return titleLiteral.Content.ToString();
		}

		private static LiteralInline GetUnformattedContentOrThrow(HeadingBlock headingBlock) {
			var literalInlines =
				headingBlock
					.Inline
					.Descendants<LiteralInline>()
					.ToArray();

			if( literalInlines.Length > 1 ) {
				throw new ArgumentException( "The level-1 heading must be unformatted." );
			}

			var titleLiteral = literalInlines.Single();
			return titleLiteral;
		}

		private static HeadingBlock GetSingleTitleOrThrow( MarkdownDocument doc ) {
			var titles =
				doc
					.Descendants<HeadingBlock>()
					.Where( h => h.Level == 1 )
					.ToArray();

			if( titles.Length == 0 ) {
				throw new ArgumentException("Document is missing a level-1 heading");
			}

			if( titles.Length > 1 ) {
				throw new ArgumentException("Document has multiple level-1 headings");
			}

			var inline = titles.Single();
			return inline;
		}

		private static RelativeFile GetOutput( DocumentContext context, string path ) {
			var infile = new FileInfo( path );
			string name = (infile.Name == "README") ? "index" : infile.Name;
			string ext = (infile.Extension == "md") ? "html" : infile.Extension;

			string relparent = Path.GetRelativePath( context.InputDirectory, infile.Path );
			string dstpath = Path.Join( context.OutputDirectory, relparent, $"{name}.{ext}" );
			var outfile = new FileInfo( dstpath );

			string relativePath = Path.GetRelativePath( context.InputDirectory, path );

			Uri editSourceUri = new Uri(
				$"https://github.com/Brightspace/{context.DocRootRepoName}/edit/master/{context.DocsPath}{relativePath}",
				UriKind.Absolute
			);

			return new RelativeFile( context, infile, outfile, editSourceUri );
		}
	}
}
