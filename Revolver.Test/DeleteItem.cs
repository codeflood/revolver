using NUnit.Framework;
using Revolver.Core;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Archiving;
using Sitecore.Data.Items;
using System.Linq;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("DeleteItem")]
  public class DeleteItem : BaseCommandTest
  {
    Item _target = null;

    [TestFixtureSetUp]
    public void Init()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;
      InitContent();
    }

    [SetUp]
    public void SetUp()
    {
      _testRoot.DeleteChildren();
      _target = _testRoot.Add("item to delete " + DateUtil.IsoNow, _context.CurrentDatabase.Templates[Constants.IDs.DocTemplateId]);
    }

    [Test]
    public void SingleItem()
    {
      var cmd = new Cmd.DeleteItem();
      InitCommand(cmd);

      _context.CurrentItem = _target;

      var parent = _target.Parent;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Recycled 1 item"));
      Assert.That(parent.GetChildren().Count, Is.EqualTo(0));
      Assert.That(FindItemInRecycleBin(_target.ID), Is.Not.Null);
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(parent.ID));
    }

    [Test]
    public void SingleItemAbsolutePath()
    {
      var cmd = new Cmd.DeleteItem();
      InitCommand(cmd);

      _context.CurrentItem = _context.CurrentDatabase.GetRootItem();
      cmd.Path = _target.ID.ToString();

      var parent = _target.Parent;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Recycled 1 item"));
      Assert.That(parent.GetChildren().Count, Is.EqualTo(0));
      Assert.That(FindItemInRecycleBin(_target.ID), Is.Not.Null);
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_context.CurrentDatabase.GetRootItem().ID));
    }

    [Test]
    public void SingleItemDelete()
    {
      var cmd = new Cmd.DeleteItem();
      InitCommand(cmd);

      _context.CurrentItem = _target;
      cmd.NoRecycle = true;

      var parent = _target.Parent;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Deleted 1 item"));
      Assert.That(parent.GetChildren().Count, Is.EqualTo(0));
      Assert.That(FindItemInRecycleBin(_target.ID), Is.Null);
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(parent.ID));
    }

    [Test]
    public void Tree()
    {
      var child1 = _target.Add("child1", _context.CurrentDatabase.Templates[Constants.IDs.DocTemplateId]);
      var child2 = _target.Add("child2", _context.CurrentDatabase.Templates[Constants.IDs.DocTemplateId]);
      var child3 = _target.Add("child3", _context.CurrentDatabase.Templates[Constants.IDs.DocTemplateId]);

      var cmd = new Cmd.DeleteItem();
      InitCommand(cmd);

      var parent = _target.Parent;

      _context.CurrentItem = _target;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Recycled 4 item"));
      Assert.That(parent.GetChildren().Count, Is.EqualTo(0));
      Assert.That(FindItemInRecycleBin(_target.ID), Is.Not.Null);
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(parent.ID));
    }

    [Test]
    public void InvalidItem()
    {
      var cmd = new Cmd.DeleteItem();
      InitCommand(cmd);

      _context.CurrentItem = _target;
      cmd.Path = "blah/blah/blah";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_target.ID));
    }

    #region Helper methods

    private ArchiveEntry FindItemInRecycleBin(ID id)
    {
      var archive = _context.CurrentDatabase.Archives["recyclebin"];
      return (from x in archive.GetEntries(0, 50)
              where x.ItemId == id
              select x).FirstOrDefault();
    }

    #endregion
  }
}
