using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using D2L.Dev.Docs.Render.Markdown;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using D2L.Dev.Docs.Render.VFS;
using LibGit2Sharp;

namespace D2L.Dev.Docs.Render {
	internal static class Program {
		/// <param name="input">The path to a directory of markdown files to render</param>
		/// <param name="output">The directory to put the rendered html files</param>
		public static async Task<int> Main( string input, string output ) {
			if ( input == null || output == null ) {
				Console.Error.WriteLine( "--input and --output arguments are required" );
				return -1;
			}

			if( !Directory.Exists( input ) ) {
				Console.Error.WriteLine( "input must be a  directory containing markdown (.md) files" );
				return -1;
			}

			if( !Directory.Exists( output ) ) {
				Directory.CreateDirectory( output );
			}

			await DoFiles( new DocumentContext( input, output, null ) );

			var submoduleInfos = SubModuleInfo.LoadFromYaml( Path.Join( input, "docs.yml" ) );
			var repoPath = Repository.Discover( "." );
			using var repo = new Repository( repoPath );

			foreach( var sub in repo.Submodules ) {
				Console.WriteLine( $"Cloning {sub.Name} into {sub.Path}" );
				// checkout the subrepo to access the files
				var subrepoPath = Path.Combine( repo.Info.WorkingDirectory, sub.Path );
				using ( var subRepo = new Repository( subrepoPath ) ) {
					Branch remoteBranch = subRepo.Branches["origin/master"];
					subRepo.Reset( ResetMode.Hard, remoteBranch.Tip );
				}

				var currSubModuleInfo = submoduleInfos[sub.Name];
				var ctx = new DocumentContext(
					Path.Join( subrepoPath, currSubModuleInfo.DocRoot ),
					Path.Join( output, currSubModuleInfo.MountPath ),
					currSubModuleInfo
				);
				await DoFiles( ctx );
			}
			return 0;
		}

		private static async Task DoFiles( DocumentContext ctx ) {
			var directories = Directory.EnumerateFiles( ctx.InputDirectory, "*", SearchOption.AllDirectories );
			foreach ( var filename in directories ) {
				var file = GetOutput( ctx, filename );
				await DoFile( file );
			}
		}

		private static async Task DoFile( RelativeFile file ) {
			if ( file.Source.Extension != "md" ) {
				return;
			}
			Console.Error.WriteLine( $"Working on {file.Source.FullPath}" );

			var text = await file.Read();

			var doc = MarkdownFactory.Parse( text );
			doc.ApplyD2LTweaks();
			var html = MarkdownFactory.RenderToString( doc, file.Context );

			var renderer = TemplateRenderer.CreateFromResource( "Templates.page.html" );
			var formatted = await renderer.RenderAsync(
				title: GetTitle( doc ),
				content: html
			);

			await file.Write( formatted );
			CopyAssociatedFiles( doc, file.Context, file.Source.Path );
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
			var inline = doc.Descendants<HeadingBlock>().Single(
				h => h.Level == 1
			).Inline;
			var titleLiteral = inline.Descendants<LiteralInline>().Single();
			return titleLiteral.Content.ToString();
		}

		private static RelativeFile GetOutput( DocumentContext context, string path ) {
			var infile = new FileInfo( path );
			string name = (infile.Name == "README") ? "index" : infile.Name;
			string ext = (infile.Extension == "md") ? "html" : infile.Extension;

			string outputDir = context.OutputDirectory;
			if ( context.SubModule != null ) {
				outputDir = Path.Join( outputDir, context.SubModule.DocRoot );
			}

			string relparent = Path.GetRelativePath( context.InputDirectory, infile.Path );
			string dstpath = Path.Join( outputDir, relparent, $"{name}.{ext}" );
			var outfile = new FileInfo( dstpath );

			return new RelativeFile( context, infile, outfile );
		}

	}
}
