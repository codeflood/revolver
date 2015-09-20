using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;


namespace Revolver.Test {
	[TestFixture]
	[Category("XPathMatch")]
	public class XPathMatch : BaseCommandTest {
		
		Cmd.XPathMatch cmd = null;
		
		[SetUp]
		public void SetUp() {
			cmd = new Cmd.XPathMatch();
			InitCommand(cmd);
		}

		[Test]
		public void NoParameters() {

			cmd.Input = "a";
			cmd.XPath = string.Empty;
			var result = cmd.Run();
			Assert.AreEqual(CommandStatus.Failure, result.Status);

			cmd.Input = string.Empty;
			cmd.XPath = "a";
			result = cmd.Run();
			Assert.AreEqual(CommandStatus.Failure, result.Status);
		}

		[Test]
		public void HAPTest(){
			
			//input, xpath
			cmd.Input = "(<div><h1>title</h1><br><div class='top'>the top</div></div>)";
			cmd.XPath = "//div[@class='top']";
			
			cmd.HAPRequired = true;
			cmd.ValueOutput = true;
			var result = cmd.Run();
			//test1
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.IsTrue(!result.Message.Contains("<div class=\"top\" />"));
			Assert.IsTrue(result.Message.Contains("Matched 1 node"));

			cmd.HAPRequired = true;
			cmd.ValueOutput = false;
			result = cmd.Run();
			//test2
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.IsTrue(result.Message.Contains("<div class=\"top\" />"));
			Assert.IsTrue(result.Message.Contains("Matched 1 node"));

			cmd.XPath = "//div[@class='none']";
			result = cmd.Run();
			Assert.IsTrue(result.Message.Contains("Matched 0 node"));
		}

		[Test]
		public void XMLTest() {
			
			//input, xpath
			cmd.Input = "<a id='myid'></a>";
			cmd.XPath = "//@id";

			cmd.HAPRequired = false;
			cmd.ValueOutput = true;
			var result = cmd.Run();
			//test3
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.IsTrue(!result.Message.Contains("id=\"myid\""));
			Assert.IsTrue(result.Message.Contains("Matched 1 node"));

			cmd.HAPRequired = false;
			cmd.ValueOutput = false;
			result = cmd.Run();
			//test4
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.IsTrue(result.Message.Contains("id=\"myid\""));
			Assert.IsTrue(result.Message.Contains("Matched 1 node"));

			cmd.XPath = "(//@rel)";
			result = cmd.Run();
			Assert.IsTrue(result.Message.Contains("Matched 0 node"));
		}

		[Test]
		public void TestStatistics(){
			cmd.Input = "<a id='myid'></a>";
			cmd.XPath = "//@id";
			cmd.NoStats = true;
			var result = cmd.Run();
			Assert.IsTrue(!result.Message.Contains("Matched"));

			cmd.NoStats = false;
			result = cmd.Run();
			Assert.IsTrue(result.Message.Contains("Matched"));
		}

		[Test]
		public void BadXMLTest() {
			cmd.Input = "a";
			cmd.XPath = "a";
			cmd.HAPRequired = false;
			var result = cmd.Run();
			if (!result.Message.Contains("Failed to parse input")) {
				Assert.Fail("'a' is not xml and should have failed to load");
			}
		}
	}
}