using NUnit.Framework;
using Revolver.Core;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using System.IO;
using System.Web;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("CreateItem")]
  public class CreateItem : BaseCommandTest
  {
    private BranchItem _branch = null;

    [TestFixtureSetUp]
    public void Init()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;
      InitContent();

      var sampleBranch = _context.CurrentDatabase.Branches[Constants.Paths.Branch];
      if (sampleBranch == null)
      {
        var branchHome = _context.CurrentDatabase.GetItem("/sitecore/templates/Branches/User Defined");
        _branch = TestUtil.CreateContentFromFile("TestResources\\branch.xml", branchHome);
      }
    }

    [TearDown]
    public void TearDown()
    {
      _testRoot.DeleteChildren();
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      if(_branch != null)
        _branch.InnerItem.Delete();
    }

    [Test]
    public void MissingNameAndConstructor()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void MissingName()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Template = Constants.Paths.DocTemplate;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void ByValidTemplateName()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      var itemName = "newitem-" + DateUtil.IsoNow;

      _context.CurrentItem = _testRoot;
      cmd.Template = Constants.Paths.DocTemplate;
      cmd.Name = itemName;

      var result = cmd.Run();

      _testRoot.Reload();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_testRoot.Children.Count, Is.EqualTo(1));
      Assert.That(_testRoot.Children[0].Name, Is.EqualTo(itemName));
    }

    [Test]
    public void ByValidTemplateId()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      var itemName = "newitem-" + DateUtil.IsoNow;

      _context.CurrentItem = _testRoot;
      cmd.Template = Constants.IDs.DocTemplateId.ToString();
      cmd.Name = itemName;

      var result = cmd.Run();

      _testRoot.Reload();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_testRoot.Children.Count, Is.EqualTo(1));
      Assert.That(_testRoot.Children[0].Name, Is.EqualTo(itemName));
    }

    [Test]
    public void ByInvalidTemplate()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      var itemName = "newitem-" + DateUtil.IsoNow;

      _context.CurrentItem = _testRoot;
      cmd.Template = "not a real template";
      cmd.Name = itemName;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void ByValidBranch()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      var itemName = "newitem-" + DateUtil.IsoNow;

      _context.CurrentItem = _testRoot;
      cmd.Branch = Constants.Paths.Branch;
      cmd.Name = itemName;

      var result = cmd.Run();

      _testRoot.Reload();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_testRoot.Children.Count, Is.EqualTo(1));
      Assert.That(_testRoot.Children[0].Name, Is.EqualTo(itemName));
      Assert.That(_testRoot.Children[0].Children.Count, Is.EqualTo(2));
    }

    [Test]
    public void ByValidBranchID()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      var itemName = "newitem-" + DateUtil.IsoNow;

      var branchItem = _context.CurrentDatabase.Branches[Constants.Paths.Branch];

      _context.CurrentItem = _testRoot;
      cmd.Branch = branchItem.ID.ToString();
      cmd.Name = itemName;

      var result = cmd.Run();

      _testRoot.Reload();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_testRoot.Children.Count, Is.EqualTo(1));
      Assert.That(_testRoot.Children[0].Name, Is.EqualTo(itemName));
      Assert.That(_testRoot.Children[0].Children.Count, Is.EqualTo(2));
    }

    [Test]
    public void ByInvalidBranch()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      var itemName = "newitem-" + DateUtil.IsoNow;

      _context.CurrentItem = _testRoot;
      cmd.Branch = "not a real branch";
      cmd.Name = itemName;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void ByValidXML()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Xml = File.ReadAllText(HttpContext.Current.Server.MapPath("TestResources\\single item.xml"));

      var result = cmd.Run();

      _testRoot.Reload();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_testRoot.Children.Count, Is.EqualTo(1));
      Assert.That(_testRoot.Children[0].Name, Is.EqualTo("newitem"));
      Assert.That(_testRoot.Children[0]["title"], Is.EqualTo("lorem"));
      Assert.That(_testRoot.Children[0].ID.ToString(), Is.EqualTo("{F961C321-63A7-49F0-8041-B04DFC442827}"));
    }

    [Test]
    public void ByInvalidXML()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Xml = "<notvalid name=\"newitem\"></notvalid>";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void ByValidTemplateRelative()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      var itemName = "newitem-" + DateUtil.IsoNow;

      _context.CurrentItem = _testRoot.Parent;
      cmd.Template = Constants.Paths.DocTemplate;
      cmd.Name = itemName;
      cmd.Path = _testRoot.Name;

      var result = cmd.Run();

      _testRoot.Reload();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_testRoot.Children.Count, Is.EqualTo(1));
      Assert.That(_testRoot.Children[0].Name, Is.EqualTo(itemName));
    }

    [Test]
    public void ByValidTemplateAbsolute()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      var itemName = "newitem-" + DateUtil.IsoNow;

      _context.CurrentItem = _testRoot.Database.GetRootItem();
      cmd.Template = Constants.Paths.DocTemplate;
      cmd.Name = itemName;
      cmd.Path = _testRoot.Paths.FullPath;

      var result = cmd.Run();

      _testRoot.Reload();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_testRoot.Children.Count, Is.EqualTo(1));
      Assert.That(_testRoot.Children[0].Name, Is.EqualTo(itemName));
    }

    [Test]
    public void FromXMLChangeIds()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Xml = File.ReadAllText(HttpContext.Current.Server.MapPath("TestResources\\single item.xml"));
      cmd.ChangeIds = true;

      var result = cmd.Run();

      _testRoot.Reload();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_testRoot.Children.Count, Is.EqualTo(1));
      Assert.That(_testRoot.Children[0].Name, Is.EqualTo("newitem"));
      Assert.That(_testRoot.Children[0]["title"], Is.EqualTo("lorem"));
      Assert.That(_testRoot.Children[0].ID.ToString(), Is.Not.EqualTo("{F961C321-63A7-49F0-8041-B04DFC442827}"));
    }

    [Test]
    public void ByValidTemplateNameDifferentDB()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      var itemName = "newitem-" + DateUtil.IsoNow;

      _context.CurrentItem = Factory.GetDatabase("master").GetRootItem();
      cmd.Template = Constants.Paths.DocTemplate;
      cmd.Name = itemName;
      cmd.Path = "/web/" + _testRoot.Paths.FullPath;

      var result = cmd.Run();

      _testRoot.Reload();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_testRoot.Children.Count, Is.EqualTo(1));
      Assert.That(_testRoot.Children[0].Name, Is.EqualTo(itemName));
    }

    [Test]
    public void NewVersion()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      var itemName = "newitem-" + DateUtil.IsoNow;
      var testItem = _testRoot.Add(itemName, _testRoot.Database.Templates[Constants.Paths.DocTemplate]);

      _context.CurrentItem = testItem;
      cmd.NewVersion = true;

      var result = cmd.Run();

      testItem.Reload();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(testItem.Versions.Count, Is.EqualTo(2));
    }

    [Test]
    public void NewVersionRelative()
    {
      var cmd = new Cmd.CreateItem();
      InitCommand(cmd);

      var itemName = "newitem-" + DateUtil.IsoNow;
      var testItem = _testRoot.Add(itemName, _testRoot.Database.Templates[Constants.Paths.DocTemplate]);

      _context.CurrentItem = _testRoot;
      cmd.NewVersion = true;
      cmd.Path = testItem.Name;

      var result = cmd.Run();

      testItem.Reload();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(testItem.Versions.Count, Is.EqualTo(2));
    }
  }
}
