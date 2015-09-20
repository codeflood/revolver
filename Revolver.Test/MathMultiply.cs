using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test {
	[TestFixture]
	[Category("MathMultiply")]
	public class MathMultiply : BaseCommandTest {
		Cmd.MathMultiply cmd = null;

		[TestFixtureSetUp]
		public void TestFixtureSetUp() {
			cmd = new Cmd.MathMultiply();
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
			Assert.AreEqual("40", result.Message);
		}

		[Test]
		public void FourNumbers() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("4");
			cmd.Numbers.Add("10");
			cmd.Numbers.Add("1");
			cmd.Numbers.Add("3");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("120", result.Message);
		}

		[Test]
		public void Negatives() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("-3");
			cmd.Numbers.Add("2");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("-6", result.Message);
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
			Assert.AreEqual("8", result.Message);
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
			Assert.AreEqual("5", result.Message);
		}
	}
}