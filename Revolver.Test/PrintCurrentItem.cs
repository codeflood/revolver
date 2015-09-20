using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
	[TestFixture]
	[Category("PrintCurrentItem")]
	public class PrintCurrentItem : BaseCommandTest
	{
    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;
      InitContent();
    }

		[Test]
		public void FromRootItem()
		{
			var cmd = new Cmd.PrintCurrentItem();
			base.InitCommand(cmd);

			_context.CurrentItem = _context.CurrentDatabase.GetRootItem();

			var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(@"/sitecore"));
		}

		[Test]
		public void ProvidedPath()
		{
			var cmd = new Cmd.PrintCurrentItem();
			base.InitCommand(cmd);

			_context.CurrentItem = _context.CurrentDatabase.GetRootItem();

			cmd.Path = _testRoot.Paths.FullPath;
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(_testRoot.Paths.FullPath));
		}

		[Test]
		public void FromHomeItem()
		{
      var cmd = new Cmd.PrintCurrentItem();
      base.InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(_testRoot.Paths.FullPath));
		}

		[Test]
		public void WithInvalidParameter()
		{
      var cmd = new Cmd.PrintCurrentItem();
      base.InitCommand(cmd);

			_context.CurrentItem = _context.CurrentDatabase.GetRootItem();
			cmd.Path = "jfj";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
		}
	}
}
