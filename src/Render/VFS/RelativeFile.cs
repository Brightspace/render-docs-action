using D2L.Dev.Docs.Render.Markdown;
using System.IO;
using System.Threading.Tasks;

namespace D2L.Dev.Docs.Render.VFS {

	internal sealed class RelativeFile {

		public FileInfo Source { get; }
		public FileInfo Destination { get; }
		public DocumentContext Context { get; }

		public RelativeFile( DocumentContext context, FileInfo source, FileInfo destination ) {
			Context = context;
			Source = source;
			Destination = destination;
		}

		public async Task<string> Read() {
			return await File.ReadAllTextAsync( Source.FullPath );
		}

		public void Copy() {
			if( File.Exists( Destination.FullPath ) ) {
				return;
			}
			new DirectoryInfo( Destination.Path ).Create();
			File.Copy( Source.FullPath, Destination.FullPath );
		}

		public async Task Write( string data ) {
			new DirectoryInfo( Destination.Path ).Create();
			await File.WriteAllTextAsync( Destination.FullPath, data );
		}

	}
}
