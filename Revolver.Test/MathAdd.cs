using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test {
	[TestFixture]
	[Category("MathAdd")]
	public class MathAdd : BaseCommandTest {
		Cmd.MathAdd cmd = null;

		[TestFixtureSetUp]
		public void TestFixtureSetUp() {
			cmd = new Cmd.MathAdd();
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
			Assert.AreEqual("14", result.Message);
		}

		[Test]
		public void SixNumbers() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("4");
			cmd.Numbers.Add("1");
			cmd.Numbers.Add("1");
			cmd.Numbers.Add("10");
			cmd.Numbers.Add("100");
			cmd.Numbers.Add("7");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("123", result.Message);
		}

		[Test]
		public void Negatives() {
			cmd.Numbers.Clear();
			cmd.Numbers.Add("-3");
			cmd.Numbers.Add("1");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("-2", result.Message);
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
			cmd.Numbers.Add("0.25");
			cmd.Numbers.Add("0.5");
			cmd.Numbers.Add("0.33");
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("1.08", result.Message);
		}
	}
}