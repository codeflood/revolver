using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test {
	[TestFixture]
	[Category("Timer")]
	public class Timer : BaseCommandTest {
		private Cmd.Timer cmd = null;

		[TestFixtureSetUp]
		public void Init() {
			cmd = new Revolver.Core.Commands.Timer();
			base.InitCommand(cmd);
		}

		[Test]
		public void CommandTest() {
			cmd.Command = "ls";
			CommandResult output = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, output.Status);
		}
	}
}