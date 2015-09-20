using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test {
	[TestFixture]
	[Category("MathDivide")]
	public class MathDivide : BaseCommandTest {
		Cmd.MathDivide cmd = null;

		[TestFixtureSetUp]
		public void TestFixtureSetUp() {
			cmd = new Cmd.MathDivide();
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
			Assert.AreEqual("0.4", result.Message);
		}

		[Test]
		public void ManyNumbers() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("40");
			cmd.Numbers.Add("10");
			cmd.Numbers.Add("2");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("2", result.Message);
		}

		[Test]
		public void Negatives() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("-3");
			cmd.Numbers.Add("2");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("-1.5", result.Message);
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
			Assert.AreEqual("2", result.Message);
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
			Assert.AreEqual("20", result.Message);
		}
	}
}