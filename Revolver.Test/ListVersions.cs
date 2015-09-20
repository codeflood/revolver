using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test {
	[TestFixture]
	[Category("ListVersions")]
	public class ListVersions : BaseCommandTest
	{
		private Cmd.ListVersions cmd = null;

		[Test]
		public void AllLangs() {
			cmd = new Cmd.ListVersions();
			base.InitCommand(cmd);
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.IsTrue(result.Message.Contains("English [en]"));
		}

		[Test]
		public void SpecifyLang() {
			cmd = new Cmd.ListVersions();
			base.InitCommand(cmd);
			cmd.Lang = "en";
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.IsTrue(result.Message.Contains("1"));
		}

		[Test]
		public void SpecifyBadLang() {
			cmd = new Cmd.ListVersions();
			base.InitCommand(cmd);
			cmd.Lang = "3";
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Failure, result.Status);
		}
	}
}