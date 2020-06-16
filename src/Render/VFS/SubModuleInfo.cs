using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace D2L.Dev.Docs.Render.VFS {
	internal sealed class SubModuleInfo {
		public string RepoName { get; }
		public string DocRoot { get; }
		public string MountPath { get; }

		public SubModuleInfo( string repoName, string docRoot, string mountPath ) {
			RepoName = repoName;
			DocRoot = docRoot;
			MountPath = mountPath;
		}

		public static ImmutableDictionary<string, SubModuleInfo> LoadFromYaml( string yamlFile ) {
			using ( var file = new StreamReader( yamlFile ) ) {
				var yaml = new YamlStream();
				yaml.Load( file );
				var cfg = (YamlMappingNode)yaml.Documents[0].RootNode;
				var nodes = (YamlMappingNode)cfg["external"];

				var submodules = nodes.Select( node => {
					var key = (YamlScalarNode)node.Key;
					var value = (YamlMappingNode)node.Value;

					return new KeyValuePair<string, SubModuleInfo>( 
						key.Value,
						new SubModuleInfo(
							key.Value,
							value.GetStr( "repo-docroot", "." ),
							value.GetStr( "mount-to" )
						)
					);
				});

				return submodules.ToImmutableDictionary();
			}
		}

	}
}
