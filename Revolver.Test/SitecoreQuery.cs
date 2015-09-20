using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data.Items;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("SitecoreQuery")]
  public class SitecoreQuery : BaseCommandTest
  {
    private Item _testContent = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      InitContent();

      _testContent = TestUtil.CreateContentFromFile("TestResources\\find content.xml", _testRoot);
    }

    [Test]
    public void MissingQuery()
    {
      var cmd = new Cmd.SitecoreQuery();
      InitCommand(cmd);
      _context.CurrentItem = _testContent;

      cmd.StatisticsOnly = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void MissingCommand()
    {
      var cmd = new Cmd.SitecoreQuery();
      InitCommand(cmd);
      _context.CurrentItem = _testContent;

      cmd.Query = "*";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void CommandAndStats()
    {
      var cmd = new Cmd.SitecoreQuery();
      InitCommand(cmd);
      _context.CurrentItem = _testContent;

      cmd.StatisticsOnly = true;
      cmd.Command = "pwd";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void FastQueryWithPath()
    {
      var cmd = new Cmd.SitecoreQuery();
      InitCommand(cmd);
      _context.CurrentItem = _testContent;

      cmd.Query = "fast://sitecore/content/*";
      cmd.Path = "..";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void NormalQuery()
    {
      var cmd = new Cmd.SitecoreQuery();
      InitCommand(cmd);
      _context.CurrentItem = _testContent;

      cmd.Query = "*";
      cmd.Command = "pwd";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Laomedeia"));
      Assert.That(result.Message, Contains.Substring("Neso"));
      Assert.That(result.Message, Contains.Substring("Proteus"));
      Assert.That(result.Message, Contains.Substring("Thalassa"));
      Assert.That(result.Message, Contains.Substring("Triton"));
      Assert.That(result.Message, Contains.Substring("Found 5 items"));
    }

    [Test]
    public void NormalQueryWithPath()
    {
      var cmd = new Cmd.SitecoreQuery();
      InitCommand(cmd);
      _context.CurrentItem = _testContent.Parent;

      cmd.Query = "*";
      cmd.Command = "pwd";
      cmd.Path = _testContent.Name;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Laomedeia"));
      Assert.That(result.Message, Contains.Substring("Neso"));
      Assert.That(result.Message, Contains.Substring("Proteus"));
      Assert.That(result.Message, Contains.Substring("Thalassa"));
      Assert.That(result.Message, Contains.Substring("Triton"));
      Assert.That(result.Message, Contains.Substring("Found 5 items"));
    }

    [Test]
    public void StatsOnly()
    {
      var cmd = new Cmd.SitecoreQuery();
      InitCommand(cmd);
      _context.CurrentItem = _testContent;

      cmd.Query = "*";
      cmd.StatisticsOnly = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo("5"));
    }

    [Test]
    public void NoStats()
    {
      var cmd = new Cmd.SitecoreQuery();
      InitCommand(cmd);
      _context.CurrentItem = _context.CurrentDatabase.GetRootItem();

      cmd.Query = _testContent.Paths.FullPath + "/*";
      cmd.Command = "pwd";
      cmd.NoStatistics = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Laomedeia"));
      Assert.That(result.Message, Contains.Substring("Neso"));
      Assert.That(result.Message, Contains.Substring("Proteus"));
      Assert.That(result.Message, Contains.Substring("Thalassa"));
      Assert.That(result.Message, Contains.Substring("Triton"));
      Assert.That(result.Message, Is.Not.ContainsSubstring("5 items"));
    }

    [Test]
    public void QueryMaximumResults()
    {
      // note: this test may fail if the host instance has changed the MaxQueryResults setting
      var cmd = new Cmd.SitecoreQuery();
      InitCommand(cmd);
      _context.CurrentItem = _context.CurrentDatabase.GetRootItem();

      cmd.Query = "//*";
      cmd.Command = "pwd";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Number of results equals the maximum query items length"));
    }
  }
}
