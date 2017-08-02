using System.Threading;
using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Search;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("IndexSearch")]
  public class IndexSearch : BaseCommandTest
  {
    private const string FIELD1_NAME = "title";
    private const string FIELD2_NAME = "text";
    private Item _item1 = null;
    private Item _item2 = null;
    private readonly string _item1Field = ID.NewID.ToShortID().ToString();
    private readonly string _item2Field = ID.NewID.ToShortID().ToString();

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      // Use master DB so the system index works
      var db = Sitecore.Configuration.Factory.GetDatabase("master");
      InitContent(db);

      var template = _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate];

      _item1 = _testRoot.Add("item1", template);
      _item1.Editing.BeginEdit();
      _item1[FIELD1_NAME] = _item1Field;
      _item1.Editing.EndEdit();

      _item2 = _testRoot.Add("item2", template);
      _item2.Editing.BeginEdit();
      _item2[FIELD2_NAME] = _item2Field;
      _item2.Editing.EndEdit();

      SearchManager.SystemIndex.Rebuild();
    }

    [Test]
    public void MissingQuery()
    {
      var cmd = new Cmd.IndexSearch();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Command = "ga -a id";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void MissingCommand()
    {
      var cmd = new Cmd.IndexSearch();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Query = FIELD1_NAME + ":" + _item1Field;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void StatsOnlyAndCommand()
    {
      var cmd = new Cmd.IndexSearch();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Query = FIELD1_NAME + ":" + _item1Field;
      cmd.Command = "ga -a id";
      cmd.StatsOnly = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void Item1ByTitle()
    {
      var cmd = new Cmd.IndexSearch();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Query = FIELD1_NAME + ":" + _item1Field;
      cmd.Command = "ga -a id";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring(_item1.ID.ToString()));
      Assert.That(result.Message, Contains.Substring("Found 1 item"));
    }

    [Test]
    public void Item2ByText()
    {
      var cmd = new Cmd.IndexSearch();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Query = FIELD2_NAME + ":" + _item2Field;
      cmd.Command = "pwd";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring(_item2.Paths.FullPath));
      Assert.That(result.Message, Contains.Substring("Found 1 item"));
    }

    [Test]
    public void BothItemsDifferentFields()
    {
      var cmd = new Cmd.IndexSearch();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Query = FIELD1_NAME + ":" + _item1Field + " or " + FIELD2_NAME + ":" + _item2Field;
      cmd.Command = "ga -a id";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring(_item1.ID.ToString()));
      Assert.That(result.Message, Contains.Substring(_item2.ID.ToString()));
      Assert.That(result.Message, Contains.Substring("Found 2 items"));
    }

    [Test]
    public void NoStats()
    {
      var cmd = new Cmd.IndexSearch();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Query = FIELD1_NAME + ":" + _item1Field + " or " + FIELD2_NAME + ":" + _item2Field;
      cmd.Command = "ga -a id";
      cmd.NoStats = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring(_item1.ID.ToString()));
      Assert.That(result.Message, Contains.Substring(_item2.ID.ToString()));
      Assert.That(result.Message, Is.Not.ContainsSubstring("Found"));
      Assert.That(result.Message, Is.Not.ContainsSubstring("item"));
    }

    [Test]
    public void StatsOnly()
    {
      var cmd = new Cmd.IndexSearch();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Query = FIELD1_NAME + ":" + _item1Field + " or " + FIELD2_NAME + ":" + _item2Field;
      cmd.StatsOnly = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("2"));
      Assert.That(result.Message, Is.Not.ContainsSubstring("Found"));
    }

    [Test]
    public void NoMatches()
    {
      var cmd = new Cmd.IndexSearch();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Query = "foo:bar";
      cmd.Command = "ga -a id";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Found 0 items"));
    }

    [Test]
    public void InvalidIndex()
    {
      var cmd = new Cmd.IndexSearch();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Query = "foo:bar";
      cmd.Command = "ga -a id";
      cmd.IndexName = "invalidIndexName";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }
  }
}