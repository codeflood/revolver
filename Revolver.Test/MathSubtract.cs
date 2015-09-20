using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test {
	[TestFixture]
	[Category("MathSubtract")]
	public class MathSubtract : BaseCommandTest {
		Cmd.MathSubtract cmd = null;

		[TestFixtureSetUp]
		public void TestFixtureSetUp() {
			cmd = new Cmd.MathSubtract();
			base.InitCommand(cmd);
		}

		[Test]
		public void NoNumbers() {
			cmd.Numbers.Clear();
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("0", result.Message);
		}

		[Test]
		public void OneNumber() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("4");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("4", result.Message);
		}

		[Test]
		public void TwoNumbers() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("4");
			cmd.Numbers.Add("10");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("-6", result.Message);
		}

		[Test]
		public void SixNumbers() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("100");
			cmd.Numbers.Add("1");
			cmd.Numbers.Add("1");
			cmd.Numbers.Add("10");
			cmd.Numbers.Add("40");
			cmd.Numbers.Add("7");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("41", result.Message);
		}

		[Test]
		public void Negatives() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("-3");
			cmd.Numbers.Add("1");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("-4", result.Message);
		}

		[Test]
		public void MultipleNegatives() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("4");
			cmd.Numbers.Add("-1");
			cmd.Numbers.Add("1");
			cmd.Numbers.Add("-2");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("6", result.Message);
		}

		[Test]
		public void NonNumber() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("b");
			cmd.Numbers.Add("1");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Failure, result.Status);
		}

		[Test]
		public void WithFractions() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("10");
			cmd.Numbers.Add("0.5");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("9.5", result.Message);
		}
	}
}