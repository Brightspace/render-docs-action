using System.IO;
using System.Threading.Tasks;

namespace D2L.Dev.Docs.Render {
	internal sealed class RelativeFile {

		public string Path { get; }
		public string SourceRoot { get; }
		public string DestinationRoot { get; }
		public string Name { get { return System.IO.Path.GetFileNameWithoutExtension( Path ); } }
		public string Extension { get { return System.IO.Path.GetExtension( Path ).TrimStart( '.' ); } }

		public RelativeFile( string srcRoot, string dstRoot, string path ) {
			SourceRoot = srcRoot;
			DestinationRoot = dstRoot;
			Path = path;
		}

		public async Task<string> Read() {
			return await File.ReadAllTextAsync( Path );
		}

		/// <summary>
		/// Copies source file to destination file
		/// </summary>
		/// <param name="newName">new filename to use for destination, defaults to same name as source</param>
		public void Copy( string newName = null ) {
			var path = GetDestinationPath( newName );
			GuaranteeParent( path );
			File.Copy( Path, path );
		}

		/// <summary>
		/// Writes data into the destination file
		/// </summary>
		/// <param name="data">data to write to the file</param>
		/// <param name="newName">new filename to use for destination, defaults to same name as source</param>
		/// <returns></returns>
		public async Task Write( string data, string newName = null ) {
			var path = GetDestinationPath( newName );
			GuaranteeParent( path );
			await File.WriteAllTextAsync( path, data );
		}

		private string GetDestinationPath( string newName = null ) {
			string path;
			string relpath = System.IO.Path.GetRelativePath( SourceRoot, Path );
			if ( newName != null ) {
				string relpathWithoutFilename = Directory.GetParent( relpath ).FullName;
				path = System.IO.Path.Join( DestinationRoot, relpathWithoutFilename, newName );
			} else {
				path = System.IO.Path.Join( DestinationRoot, relpath );
			}
			return path;
		}

		private void GuaranteeParent( string path ) {
			Directory.GetParent( path ).Create();
		}

	}
}
