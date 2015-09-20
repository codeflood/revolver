using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test {
	[TestFixture]
	[Category("MathModulus")]
	public class MathModulus : BaseCommandTest {
		Cmd.MathModulus cmd = null;

		[TestFixtureSetUp]
		public void TestFixtureSetUp() {
			cmd = new Cmd.MathModulus();
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
			cmd.Numbers.Add("10");
			cmd.Numbers.Add("4");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("2", result.Message);
		}

		[Test]
		public void ManyNumbers() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("39");
			cmd.Numbers.Add("10");
			cmd.Numbers.Add("2");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("1", result.Message);
		}

		[Test]
		public void Negatives() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("-30");
			cmd.Numbers.Add("9");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("-3", result.Message);
		}

		[Test]
		public void MultipleNegatives() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("-30");
			cmd.Numbers.Add("9");
			cmd.Numbers.Add("-2");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("-1", result.Message);
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
			Assert.AreEqual("0", result.Message);
		}
	}
}