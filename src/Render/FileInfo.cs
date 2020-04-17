namespace D2L.Dev.Docs.Render {
	internal readonly struct FileInfo {
		public string Path { get; }
		public string Name { get; }
		public string Extension { get; }
		public string FullPath {
			get {
				return System.IO.Path.Join( Path, $"{Name}.{Extension}" );
			}
		}

		public FileInfo( string path ) {
			Path = path.Substring( 0, path.LastIndexOf( '/' ) );
			Name = System.IO.Path.GetFileNameWithoutExtension( path );
			Extension = System.IO.Path.GetExtension( path ).TrimStart( '.' );
		}
		public FileInfo( string path, string name, string extension ) {
			Path = path;
			Name = name;
			Extension = extension;
		}
	}
}
