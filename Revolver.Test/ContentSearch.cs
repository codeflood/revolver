using NUnit.Framework;
using Revolver.Core;
using Sitecore.ContentSearch;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ContentSearch")]
  public class ContentSearch : BaseCommandTest
  {
    private const string Name1 = "Zoolander";
    private const string Name2 = "Quark";
    private const string IndexName = "sitecore_web_index";
    private readonly string Content = ID.NewID.ToShortID().ToString();

    private Item _item1 = null;
    private Item _item2 = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      _context.CurrentDatabase = Sitecore.Configuration.Factory.GetDatabase("web");

      var template = _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate];

      using (new SecurityDisabler())
      {
        InitContent();

        _item1 = _testRoot.Add(Name1, template);
        _item1.Editing.BeginEdit();
        _item1["text"] = Content;
        _item1.Editing.EndEdit();

        _item2 = _testRoot.Add(Name2, template);
        _item2.Editing.BeginEdit();
        _item2["text"] = Content;
        _item2.Editing.EndEdit();

        var index = ContentSearchManager.GetIndex(IndexName);

        // Rebuild rather than update to ensure previous test invocations haven't muddied the index
        index.Rebuild();
      }
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      if (_testRoot != null)
      {
        using (new SecurityDisabler())
        {
          _testRoot.Delete();
        }
      }
    }

    [Test]
    public void Item1ByName()
    {
      var cmd = new Cmd.ContentSearch();
      base.InitCommand(cmd);

      cmd.Query = "_name:" + Name1;
      cmd.Command = "ga -a id";
      cmd.IndexName = IndexName;

      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.IsTrue(result.Message.Contains(_item1.ID.ToString()));
      Assert.IsTrue(result.Message.Contains("Found 1 item"));
    }

    [Test]
    public void Item2ByName()
    {
      var cmd = new Cmd.ContentSearch();
      base.InitCommand(cmd);

      cmd.Query = "_name:" + Name2;
      cmd.Command = "ga -a id";
      cmd.IndexName = IndexName;
      
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.IsTrue(result.Message.Contains(_item2.ID.ToString()));
      Assert.IsTrue(result.Message.Contains("Found 1 item"));
    }

    [Test]
    public void BothItemsByContent()
    {
      var cmd = new Cmd.ContentSearch();
      base.InitCommand(cmd);

      cmd.Query = "_content:" + Content;
      cmd.Command = "pwd";
      cmd.IndexName = IndexName;

      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.IsTrue(result.Message.Contains(_item1.Paths.FullPath));
      Assert.IsTrue(result.Message.Contains(_item2.Paths.FullPath));
      Assert.IsTrue(result.Message.Contains("Found 2 items"));
    }

    [Test]
    public void ByPathWithoutItem2Name()
    {
      var cmd = new Cmd.ContentSearch();
      base.InitCommand(cmd);

      cmd.Query = "_path:" + _testRoot.ID.ToShortID().ToString().ToLower() + ";-_name:" + _item2.Name;
      cmd.Command = "ga -a name";
      cmd.IndexName = IndexName;

      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.IsTrue(result.Message.Contains(_item1.Name));
      Assert.IsTrue(result.Message.Contains(_testRoot.Name));
      Assert.IsTrue(result.Message.Contains("Found 2 item"));
    }

    [Test]
    public void NoStats()
    {
      var cmd = new Cmd.ContentSearch();
      base.InitCommand(cmd);

      cmd.Query = "_path:" + _testRoot.ID.ToShortID().ToString().ToLower() + ";\\-_name:" + _item2.Name;
      cmd.Command = "ga -a name";
      cmd.NoStats = true;
      cmd.IndexName = IndexName;

      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.IsTrue(result.Message.Contains(_item1.Name));
      Assert.IsFalse(result.Message.Contains("Found"));
    }

    [Test]
    public void NoMatches()
    {
      var cmd = new Cmd.ContentSearch();
      base.InitCommand(cmd);

      cmd.Query = "blah:foo";
      cmd.Command = "ga -a id";
      cmd.IndexName = IndexName;

      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.IsTrue(result.Message.Contains("Found 0 items"));
    }

    [Test]
    public void StatsOnly()
    {
      var cmd = new Cmd.ContentSearch();
      base.InitCommand(cmd);

      cmd.Query = "_content:" + Content;
      cmd.StatsOnly = true;
      cmd.IndexName = IndexName;

      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual("2", result.Message);
    }

    [Test]
    public void DifferentIndex()
    {
      var cmd = new Cmd.ContentSearch();
      base.InitCommand(cmd);

      cmd.Query = "_group:" + Sitecore.ItemIDs.ContentRoot.ToShortID().ToString().ToLower();
      cmd.StatsOnly = true;
      cmd.IndexName = "sitecore_core_index";

      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual("1", result.Message);
    }

    [Test]
    public void MissingCommand()
    {
      var cmd = new Cmd.ContentSearch();
      base.InitCommand(cmd);

      cmd.Query = "_content:" + Content;
      cmd.IndexName = IndexName;

      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }

    [Test]
    public void MissingQuery()
    {
      var cmd = new Cmd.ContentSearch();
      base.InitCommand(cmd);

      cmd.IndexName = IndexName;

      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }

    [Test]
    public void InvalidIndex()
    {
      var cmd = new Cmd.ContentSearch();
      base.InitCommand(cmd);

      cmd.IndexName = "someinvalidindex";
      cmd.Query = "_name:" + Name2;
      cmd.Command = "ga -a id";

      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }
  }
}