using D2L.Dev.Docs.Render.Markdown;
using System.IO;
using System.Threading.Tasks;

namespace D2L.Dev.Docs.Render {

	internal sealed class RelativeFile {

		public FileInfo Source { get; }
		public FileInfo Destination { get; }
		public DocumentContext Context { get; }

		public RelativeFile( DocumentContext context, string path, string dstName = null ) {
			Context = context;
			Source = new FileInfo( path );

			string relparent = Path.GetRelativePath( Context.InputDirectory, Source.Path );
			string dstpath = Path.Join( Context.OutputDirectory, relparent, dstName );
			Destination = new FileInfo( dstpath );
		}

		public async Task<string> Read() {
			return await File.ReadAllTextAsync( Source.FullPath );
		}

		/// <summary>
		/// Copies source file to destination file
		/// </summary>
		/// <param name="newName">new filename to use for destination, defaults to same name as source</param>
		public void Copy() {
			new DirectoryInfo( Destination.Path ).Create();
			File.Copy( Source.FullPath, Destination.FullPath );
		}

		/// <summary>
		/// Writes data into the destination file
		/// </summary>
		/// <param name="data">data to write to the file</param>
		/// <param name="newName">new filename to use for destination, defaults to same name as source</param>
		/// <returns></returns>
		public async Task Write( string data ) {
			new DirectoryInfo( Destination.Path ).Create();
			await File.WriteAllTextAsync( Destination.FullPath, data );
		}

	}
}
