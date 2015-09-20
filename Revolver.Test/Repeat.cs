using NUnit.Framework;
using Revolver.Core;
using Sitecore.SecurityModel;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
	[TestFixture]
	[Category("Repeat")]
	public class Repeat : BaseCommandTest
	{
		[TestFixtureSetUp]
		public void Init()
		{
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      InitContent();
		}

		[TearDown]
		public void TearDown()
		{
			using (new SecurityDisabler())
			{
				_testRoot.DeleteChildren();
			}
		}

		[Test]
		public void RepeatSimple()
		{
			var cmd = new Cmd.Repeat();
			base.InitCommand(cmd);

			cmd.Number = "3";
			cmd.Command = "echo $num$";

			var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
			Assert.That(result.Message, Is.EqualTo("1\r\n2\r\n3\r\n"));
		}

		[Test]
		public void MissingCommand()
		{
      var cmd = new Cmd.Repeat();
			base.InitCommand(cmd);

			cmd.Number = "7";

			var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
		}

		[Test]
		public void WithRelativePath()
		{
			var cmd = new Cmd.Repeat();
			base.InitCommand(cmd);

			cmd.Number = "4";
			cmd.Command = "touch -t (" + Constants.Paths.DocTemplate + ") (item $num$)";
			cmd.Path = _testRoot.Paths.FullPath;

			var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
			Assert.That(_testRoot.Children.Count, Is.EqualTo(4));
			Assert.That(_testRoot.Children[0].Name, Is.EqualTo("item 1"));
      Assert.That(_testRoot.Children[1].Name, Is.EqualTo("item 2"));
      Assert.That(_testRoot.Children[2].Name, Is.EqualTo("item 3"));
      Assert.That(_testRoot.Children[3].Name, Is.EqualTo("item 4"));
		}

		[Test]
		public void WithNegativeNumber()
		{
			var cmd = new Cmd.Repeat();
			base.InitCommand(cmd);

			cmd.Number = "-3";
			cmd.Command = "echo $num$";

			var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
		}

		[Test]
		public void NonNumbericNumber()
		{
			var cmd = new Cmd.Repeat();
			base.InitCommand(cmd);

			cmd.Number = "a";
			cmd.Command = "echo $num$";

			var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
		}
	}
}
