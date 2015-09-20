using NUnit.Framework;
using Revolver.Core;
using System.Text.RegularExpressions;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
	[TestFixture]
	[Category("ListDatabases")]
	public class ListDatabases : BaseCommandTest
	{
		Cmd.ListDatabases _command = null;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_command = new Cmd.ListDatabases();
			base.InitCommand(_command);
		}

		[Test]
		public void List()
		{
			var result = _command.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			
			var regex = new Regex("\\r\\n");
			Assert.AreEqual(3, regex.Matches(result.Message.Trim()).Count);

			Assert.IsTrue(result.Message.Contains("master"));
			Assert.IsTrue(result.Message.Contains("web"));
			Assert.IsTrue(result.Message.Contains("core"));
			Assert.IsTrue(result.Message.Contains("filesystem"));
		}
	}
}
