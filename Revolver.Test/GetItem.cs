using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data.Items;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
	[TestFixture]
	[Category("GetItem")]
	public class GetItem : BaseCommandTest
	{
    Item _contentRoot = null;

	  [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      InitContent();

      _contentRoot = TestUtil.CreateContentFromFile("TestResources\\find content.xml", _testRoot);
    }

    [Test]
    public void SingelItemFromContext()
    {
      var cmd = new Cmd.GetItem();
      base.InitCommand(cmd);

      _context.CurrentItem = _contentRoot;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("<item"));
      Assert.That(result.Message, Contains.Substring("</item>"));
      Assert.That(result.Message, Contains.Substring("key=\"items\""));
      Assert.That(result.Message, Is.Not.ContainsSubstring("key=\"neso\""));
    }

    [Test]
    public void SingelItemFromPath()
    {
      var cmd = new Cmd.GetItem();
      base.InitCommand(cmd);

      _context.CurrentItem = _contentRoot.Database.GetRootItem();
      cmd.Path = _contentRoot.Paths.FullPath;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("<item"));
      Assert.That(result.Message, Contains.Substring("</item>"));
      Assert.That(result.Message, Contains.Substring("key=\"items\""));
      Assert.That(result.Message, Is.Not.ContainsSubstring("key=\"neso\""));
    }

    [Test]
    public void RecursiveItemFromContext()
    {
      var cmd = new Cmd.GetItem();
      base.InitCommand(cmd);

      _context.CurrentItem = _contentRoot.Axes.GetChild("proteus");
      cmd.Recursive = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("<item"));
      Assert.That(result.Message, Contains.Substring("</item>"));
      Assert.That(result.Message, Contains.Substring("key=\"proteus\""));
      Assert.That(result.Message, Contains.Substring("key=\"child1\""));
      Assert.That(result.Message, Contains.Substring("key=\"child2\""));
    }
	}
}
