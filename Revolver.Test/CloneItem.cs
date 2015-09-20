using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("CloneItem")]
  public class CloneItem : BaseCommandTest
  {
    Cmd.CloneItem _command = null;
    TemplateItem _template = null;


    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      // Set DB to master so notification provider exists
      _context.CurrentDatabase = Sitecore.Configuration.Factory.GetDatabase("master");

      using (new SecurityDisabler())
      {
        InitContent(_context.CurrentDatabase);
      }

      _template = _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate];
    }

    [SetUp]
    public void SetUp()
    {
      _command = new Cmd.CloneItem();
      base.InitCommand(_command);

      // DB is set back to web on base.Init(), so change it back
      _context.CurrentDatabase = Sitecore.Configuration.Factory.GetDatabase("master");
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
    public void NoParameters()
    {
      Item source = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        source = _testRoot.Add("item a", _template);

        _context.CurrentItem = source;
        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Success, res.Status);

      var cloneItem = source.Axes.GetChild(source.Name);
      Assert.IsNotNull(cloneItem);
      Assert.IsTrue(cloneItem.IsClone);
      Assert.AreEqual(source.Uri, new ItemUri(cloneItem[Sitecore.FieldIDs.Source]));
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void NoParametersInadequateSecurity()
    {
      Item source = null;      

      using (new SecurityDisabler())
      {
        source = _testRoot.Add("item a", _template);
      }

      _context.CurrentItem = source;
      _command.Run();
    }

    [Test]
    public void CloneSingleFromTree()
    {
      Item source = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        source = _testRoot.Add("item a", _template);
        source.Add("subitem a", _template);

        _context.CurrentItem = source;
        _command.SingleClone = true;
        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Success, res.Status);

      var cloneItem = source.Axes.GetChild(source.Name);
      Assert.IsNotNull(cloneItem);
      Assert.IsTrue(cloneItem.IsClone);
      Assert.AreEqual(source.Uri, new ItemUri(cloneItem[Sitecore.FieldIDs.Source]));
      Assert.AreEqual(0, cloneItem.GetChildren().Count);
    }

    [Test]
    public void CloneTree()
    {
      Item source = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        source = _testRoot.Add("item a", _template);
        source.Add("subitem a", _template);

        _context.CurrentItem = source;
        _command.SingleClone = false;

        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Success, res.Status);

      var cloneItem = source.Axes.GetChild(source.Name);
      Assert.IsNotNull(cloneItem);
      Assert.IsTrue(cloneItem.IsClone);
      Assert.AreEqual(source.Uri, new ItemUri(cloneItem[Sitecore.FieldIDs.Source]));
      Assert.AreEqual(1, cloneItem.GetChildren().Count);
    }

    [Test]
    public void CloneDifferentPath()
    {
      Item source = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        var bufferItem = _testRoot.Add("buffer", _template);
        source = bufferItem.Add("item a", _template);

        _context.CurrentItem = source;
        _command.TargetPath = _testRoot.ID.ToString();

        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual(2, _testRoot.GetChildren().Count);

      var cloneItem = _testRoot.Axes.GetChild(source.Name);
      Assert.IsNotNull(cloneItem);
      Assert.IsTrue(cloneItem.IsClone);
      Assert.AreEqual(source.Uri, new ItemUri(cloneItem[Sitecore.FieldIDs.Source]));
    }

    [Test]
    public void CloneDifferentPathFromDifferentPath()
    {
      Item source = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        var bufferItem = _testRoot.Add("buffer", _template);
        source = bufferItem.Add("item a", _template);

        _context.CurrentItem = _testRoot.Database.GetRootItem();
        _command.TargetPath = _testRoot.ID.ToString();
        _command.Path = source.Paths.FullPath;
        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual(2, _testRoot.GetChildren().Count);

      var cloneItem = _testRoot.Axes.GetChild(source.Name);
      Assert.IsNotNull(cloneItem);
      Assert.IsTrue(cloneItem.IsClone);
      Assert.AreEqual(source.Uri, new ItemUri(cloneItem[Sitecore.FieldIDs.Source]));
    }

    [Test]
    public void Unclone()
    {
      Item sourceItem = null;
      Item cloneItem = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        sourceItem = _testRoot.Add("item a", _template);
        cloneItem = sourceItem.CloneTo(sourceItem);

        _context.CurrentItem = cloneItem;
        _command.Unclone = true;
        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual(1, sourceItem.GetChildren().Count);

      cloneItem.Reload();
      Assert.IsFalse(cloneItem.IsClone);
    }

    [Test]
    public void UncloneDifferentPath()
    {
      Item sourceItem = null;
      Item cloneItem = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        sourceItem = _testRoot.Add("item a", _template);
        cloneItem = sourceItem.CloneTo(sourceItem);

        _context.CurrentItem = _testRoot;
        _command.TargetPath = cloneItem.ID.ToString();
        _command.Unclone = true;
        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual(1, sourceItem.GetChildren().Count);

      cloneItem.Reload();
      Assert.IsFalse(cloneItem.IsClone);
    }

    [Test]
    public void UncloneRealItem()
    {
      Item sourceItem = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        sourceItem = _testRoot.Add("item a", _template);

        _context.CurrentItem = sourceItem;
        _command.Unclone = true;
        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Failure, res.Status);
    }

    [Test]
    public void AcceptChange()
    {
      Item sourceItem = null;
      Item cloneItem = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        sourceItem = _testRoot.Add("item a", _template);
        cloneItem = sourceItem.CloneTo(sourceItem);

        using (new EditContext(cloneItem))
        {
          cloneItem["title"] = "different title";
        }

        using (new EditContext(sourceItem))
        {
          sourceItem["title"] = "updated title";
        }

        _context.CurrentItem = cloneItem;
        _command.AcceptChanges = true;
        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Success, res.Status);

      cloneItem.Reload();
      Assert.AreEqual(sourceItem["title"], cloneItem["title"]);
    }

    [Test]
    public void AcceptChangeNoChange()
    {
      const string title = "lorem ipsum dolor sit";

      Item sourceItem = null;
      Item cloneItem = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        sourceItem = _testRoot.Add("item a", _template);
        cloneItem = sourceItem.CloneTo(sourceItem);

        using (new EditContext(cloneItem))
        {
          cloneItem["title"] = title;
        }

        _context.CurrentItem = cloneItem;
        _command.AcceptChanges = true;
        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Success, res.Status);

      cloneItem.Reload();
      Assert.AreEqual(title, cloneItem["title"]);
    }

    [Test]
    public void AcceptChangeNotOnClone()
    {
      Item sourceItem = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        sourceItem = _testRoot.Add("item a", _template);

        _context.CurrentItem = sourceItem;
        _command.AcceptChanges = true;
        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Success, res.Status);
    }

    [Test]
    public void RejectChange()
    {
      const string title = "this is my title";
      Item sourceItem = null;
      Item cloneItem = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        sourceItem = _testRoot.Add("item a", _template);
        cloneItem = sourceItem.CloneTo(sourceItem);

        using (new EditContext(cloneItem))
        {
          cloneItem["title"] = title;
        }

        using (new EditContext(sourceItem))
        {
          sourceItem["title"] = "updated title";
        }

        _context.CurrentItem = cloneItem;
        _command.RejectChanges = true;
        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Success, res.Status);

      cloneItem.Reload();
      Assert.AreEqual(title, cloneItem["title"]);
    }

    [Test]
    public void RejectChangeRelativePath()
    {
      const string title = "this is my title";
      Item sourceItem = null;
      Item cloneItem = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        sourceItem = _testRoot.Add("item a", _template);
        cloneItem = sourceItem.CloneTo(sourceItem);

        using (new EditContext(cloneItem))
        {
          cloneItem["title"] = title;
        }

        using (new EditContext(sourceItem))
        {
          sourceItem["title"] = "updated title";
        }

        _context.CurrentItem = sourceItem.Parent;
        _command.TargetPath = sourceItem.Name + "/" + cloneItem.Name;
        _command.RejectChanges = true;
        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Success, res.Status);

      cloneItem.Reload();
      Assert.AreEqual(title, cloneItem["title"]);
    }

    [Test]
    public void RejectChangeNoChange()
    {
      const string title = "this is my title";
      Item sourceItem = null;
      Item cloneItem = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        sourceItem = _testRoot.Add("item a", _template);
        cloneItem = sourceItem.CloneTo(sourceItem);

        using (new EditContext(cloneItem))
        {
          cloneItem["title"] = title;
        }

        _context.CurrentItem = cloneItem;
        _command.RejectChanges = true;
        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Success, res.Status);

      cloneItem.Reload();
      Assert.AreEqual(title, cloneItem["title"]);
    }

    [Test]
    public void RejectChangeNotOnClone()
    {
      Item sourceItem = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        sourceItem = _testRoot.Add("item a", _template);

        _context.CurrentItem = sourceItem;
        _command.RejectChanges = true;
        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Success, res.Status);
    }

    [Test]
    public void AcceptAndReject()
    {
      const string title = "this is my title";
      Item sourceItem = null;
      Item cloneItem = null;
      CommandResult res = null;

      using (new SecurityDisabler())
      {
        sourceItem = _testRoot.Add("item a", _template);
        cloneItem = sourceItem.CloneTo(sourceItem);

        using (new EditContext(cloneItem))
        {
          cloneItem["title"] = title;
        }

        using (new EditContext(sourceItem))
        {
          sourceItem["title"] = "updated title";
        }

        _context.CurrentItem = cloneItem;
        _command.AcceptChanges = true;
        _command.RejectChanges = true;
        res = _command.Run();
      }

      Assert.AreEqual(CommandStatus.Failure, res.Status);
    }
  }
}
