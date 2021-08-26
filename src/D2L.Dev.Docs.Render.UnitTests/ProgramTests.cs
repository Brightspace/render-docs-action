using NUnit.Framework;
using D2L.Dev.Docs.Render;

namespace D2L.Dev.Docs.Render.UnitTests {
	public class ProgramTests {

		[TestCase( "refs/heads/main",                         ExpectedResult = "main" )]
		[TestCase( "refs/heads/master",                       ExpectedResult = "master" )]
		[TestCase( "refs/heads",                              ExpectedResult = "master" )]
		[TestCase( "refs",                                    ExpectedResult = "master" )]
		[TestCase( "refs/heads/askfjdhksaljdfhkjashfkjafhsd", ExpectedResult = "master" )]
		[TestCase( "refs/pull/638/merge",                     ExpectedResult = "master" )]
		[TestCase( "refs/pull",                               ExpectedResult = "master" )]
		[TestCase( "randomnonsense",                          ExpectedResult = "master" )]
		public string GetBranchFromRef( string gitRef ) {
			return Program.GetBranchFromRef( gitRef );
		}
	}
}
