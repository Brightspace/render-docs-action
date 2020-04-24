using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace D2L.Dev.Docs.Render.VFS {
	internal static class Extensions {
		public static R Get<R>(
			this YamlMappingNode map,
			string key
		) where R : YamlNode =>
			(R)map.Children[new YamlScalarNode( key )];

		public static bool TryGet<R>(
			this YamlMappingNode map,
			string key,
			out R result
		) where R : YamlNode {
			if ( map.Children.ContainsKey( key ) ) {
				result = (R)map.Children[key];
				return true;
			}

			result = null;
			return false;
		}

		public static string GetStr(
			this YamlMappingNode map,
			string key
		) => map.Get<YamlScalarNode>( key ).Value;

		public static bool TryGetStr(
			this YamlMappingNode map,
			string key,
			out string result
		) {
			YamlScalarNode x;
			if ( map.TryGet<YamlScalarNode>( key, out x ) ) {
				result = x.Value;
				return true;
			}

			result = null;
			return false;
		}

		public static string GetStr(
			this YamlMappingNode map,
			string key,
			string defaultValue
		) {
			if ( map.TryGetStr( key, out var r ) ) {
				return r;
			}

			return defaultValue;
		}

		public static R Get<R>(
			this YamlMappingNode map,
			string key,
			R defaultValue
		) where R : YamlNode {
			if ( map.TryGet<R>( key, out var r ) ) {
				return r;
			}

			return defaultValue;
		}
	}
}
