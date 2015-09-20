using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data.Items;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("MoveItem")]
  public class MoveItem : BaseCommandTest
  {
    TemplateItem _template = null;
    private Item _item1 = null;
    private Item _item2 = null;
    private Item _subItem1 = null;
    private Item _subItem2 = null;

    [TestFixtureSetUp]
    public void Init()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;
      InitContent();

      _template = _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate];
    }

    [SetUp]
    public void SetUp()
    {
      _item1 = _testRoot.Add("item1", _template);
      _item2 = _testRoot.Add("item2", _template);
      _subItem1 = _item1.Add("subitem1", _template);
      _subItem2 = _item1.Add("subitem2", _template);
    }

    [TearDown]
    public void TearDown()
    {
      _testRoot.DeleteChildren();
    }

    [Test]
    public void NoTarget()
    {
      var cmd = new Cmd.MoveItem();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void FromContextToRelative()
    {
      var cmd = new Cmd.MoveItem();
      InitCommand(cmd);

      _context.CurrentItem = _subItem1;
      cmd.TargetPath = "../..";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_subItem1.Parent.ID, Is.EqualTo(_testRoot.ID));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_subItem1.ID));
    }

    [Test]
    public void FromRelativeToAbsolute()
    {
      var cmd = new Cmd.MoveItem();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Path = _item1.Name + "/" + _subItem2.Name;
      cmd.TargetPath = _testRoot.Paths.FullPath;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_subItem2.Parent.ID, Is.EqualTo(_testRoot.ID));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testRoot.ID));
    }

    [Test]
    public void FromAbsoluteToAbsolute()
    {
      var cmd = new Cmd.MoveItem();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Path = _subItem2.ID.ToString();
      cmd.TargetPath = _testRoot.ID.ToString();

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_subItem2.Parent.ID, Is.EqualTo(_testRoot.ID));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testRoot.ID));
    }

    [Test]
    public void ToDifferentDB()
    {
      var cmd = new Cmd.MoveItem();
      InitCommand(cmd);

      _context.CurrentItem = _subItem1;
      cmd.TargetPath = "/web/sitecore/content/home";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      var webDb = Sitecore.Configuration.Factory.GetDatabase("web");
      var webItem = webDb.GetItem(_subItem1.ID);
      Assert.That(webItem.ID, Is.EqualTo(_subItem1.ID));
      Assert.That(webItem.Paths.FullPath, Is.EqualTo(_subItem1.Paths.FullPath));
      webItem.Delete();
    }

    [Test]
    public void InvalidTarget()
    {
      var cmd = new Cmd.MoveItem();
      InitCommand(cmd);

      _context.CurrentItem = _subItem1;
      cmd.TargetPath = "/sitecore/blah/blah";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void InvalidSource()
    {
      var cmd = new Cmd.MoveItem();
      InitCommand(cmd);

      _context.CurrentItem = _subItem1;
      cmd.Path = "/sitecore/blah/blah";
      cmd.TargetPath = _testRoot.ID.ToString();

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void MoveTree()
    {
      var cmd = new Cmd.MoveItem();
      InitCommand(cmd);

      _context.CurrentItem = _item1;
      cmd.TargetPath = "../item2";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_item1.Parent.ID, Is.EqualTo(_item2.ID));
      Assert.That(result.Message, Contains.Substring("Moved 3 items"));
    }
  }
}
