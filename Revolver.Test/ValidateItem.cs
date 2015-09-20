using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data;
using Sitecore.Data.Items;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ValidateItem")]
  public class ValidateItem : BaseCommandTest
  {
    private readonly Database _database = Sitecore.Configuration.Factory.GetDatabase("master");
    private TemplateItem _template = null;
    private Item _itemPassing = null;
    private Item _itemFailing = null;
    private Item _itemDup1 = null;
    private Item _itemDup2 = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      _context.CurrentDatabase = _database;
      InitContent(_database);

      var templateFolder = _context.CurrentDatabase.GetItem("/sitecore/templates/user defined");
      _template = TestUtil.CreateContentFromFile("TestResources\\validation template.xml", templateFolder);

      _itemPassing = _testRoot.Add("passing", _template);
      using (new EditContext(_itemPassing))
      {
        _itemPassing["Required Button"] = "lorem";
        _itemPassing["Required Gutter"] = "lorem";
        _itemPassing["Required Bar"] = "lorem";
        _itemPassing["Integer Workflow"] = "5";
      }

      _itemFailing = _testRoot.Add("fail", _template);
      using (new EditContext(_itemFailing))
      {
        _itemFailing["Integer Workflow"] = "lorem";
      }

      _itemDup1 = _testRoot.Add("dup", _template);
      using (new EditContext(_itemDup1))
      {
        _itemDup1["Required Button"] = "lorem";
        _itemDup1["Required Gutter"] = "lorem";
        _itemDup1["Required Bar"] = "lorem";
        _itemDup1["Integer Workflow"] = "5";
      }

      _itemDup2 = _testRoot.Add("dup", _template);
      using (new EditContext(_itemDup2))
      {
        _itemDup2["Required Button"] = "lorem";
        _itemDup2["Required Gutter"] = "lorem";
        _itemDup2["Required Bar"] = "lorem";
        _itemDup2["Integer Workflow"] = "5";
      }
    }

    protected override void CleanUp()
    {
      
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      CleanUp();

      _template.InnerItem.Delete();
    }

    [SetUp]
    public void SetUp()
    {
      _context.CurrentDatabase = _database;
    }

    [Test]
    public void Gutter_Pass()
    {
      var cmd = new Cmd.ValidateItem();
      InitCommand(cmd);
      _context.CurrentItem = _itemPassing;
      cmd.ModeGutter = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

    }

    [Test]
    public void Button_Pass()
    {
      var cmd = new Cmd.ValidateItem();
      InitCommand(cmd);
      _context.CurrentItem = _itemPassing;
      cmd.ModeButton = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
    }

    [Test]
    public void Bar_Pass_ByPath()
    {
      var cmd = new Cmd.ValidateItem();
      InitCommand(cmd);
      _context.CurrentItem = _testRoot.Parent;
      cmd.ModeBar = true;
      cmd.Path = _testRoot.Name + "/" + _itemPassing.Name;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
    }

    [Test]
    public void Workflow_Pass()
    {
      var cmd = new Cmd.ValidateItem();
      InitCommand(cmd);
      _context.CurrentItem = _itemPassing;
      cmd.ModeWorkflow = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
    }

    [Test]
    public void Gutter_Fail_ByPath()
    {
      var cmd = new Cmd.ValidateItem();
      InitCommand(cmd);
      _context.CurrentItem = _database.GetRootItem();
      cmd.ModeGutter = true;
      cmd.Path = _itemFailing.Paths.FullPath;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void Button_Fail()
    {
      var cmd = new Cmd.ValidateItem();
      InitCommand(cmd);
      _context.CurrentItem = _itemFailing;
      cmd.ModeButton = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void Bar_Fail()
    {
      var cmd = new Cmd.ValidateItem();
      InitCommand(cmd);
      _context.CurrentItem = _itemFailing;
      cmd.ModeBar = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void Workflow_Fail_ByID()
    {
      var cmd = new Cmd.ValidateItem();
      InitCommand(cmd);
      _context.CurrentItem = _database.GetRootItem();
      cmd.ModeGutter = true;
      cmd.Path = _itemFailing.ID.ToString();

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void Item_Fail()
    {
      var cmd = new Cmd.ValidateItem();
      InitCommand(cmd);
      _context.CurrentItem = _itemDup1;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void All_Pass()
    {
      var cmd = new Cmd.ValidateItem();
      InitCommand(cmd);
      _context.CurrentItem = _itemPassing;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
    }

    [Test]
    public void All_Fail()
    {
      var cmd = new Cmd.ValidateItem();
      InitCommand(cmd);
      _context.CurrentItem = _itemFailing;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }
  }
}
