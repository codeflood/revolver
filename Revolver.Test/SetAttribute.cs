using NUnit.Framework;
using Revolver.Core;
using Sitecore;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("SetAttribute")]
  public class SetAttribute : BaseCommandTest
  {
    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      InitContent();
    }

    [Test]
    public void SetName()
    {
      var salt = DateUtil.IsoNow;
      var name = "nameitem" + salt;
      var item = _testRoot.Add(name, _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate]);

      var cmd = new Cmd.SetAttribute();
      InitCommand(cmd);

      _context.CurrentItem = item;
      cmd.Attribute = "name";
      cmd.Value = "updated " + name;

      var result = cmd.Run();
      item.Reload();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringContaining("name"));
      Assert.That(result.Message, Is.StringContaining("updated " + name));
      Assert.That(item.Name, Is.EqualTo("updated " + name));
    }

    [Test]
    public void InvalidAttribute()
    {
      var cmd = new Cmd.SetAttribute();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Attribute = "bler";
      cmd.Value = "hello";
      
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void SetNameRelative()
    {
      var salt = DateUtil.IsoNow;
      var name = "nameitem" + salt;
      var item = _testRoot.Add(name, _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate]);

      var cmd = new Cmd.SetAttribute();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot.Parent;
      cmd.Attribute = "name";
      cmd.Value = "updated " + name;
      cmd.Path = _testRoot.Name + "/" + item.Name;

      var result = cmd.Run();
      item.Reload();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringContaining("name"));
      Assert.That(result.Message, Is.StringContaining("updated " + name));
      Assert.That(item.Name, Is.EqualTo("updated " + name));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testRoot.Parent.ID));
    }

    [Test]
    public void SetNameWithToken()
    {
      var salt = DateUtil.IsoNow;
      var name = "nameitem" + salt;
      var item = _testRoot.Add(name, _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate]);

      var cmd = new Cmd.SetAttribute();
      InitCommand(cmd);

      _context.CurrentItem = item;
      cmd.Attribute = "name";
      cmd.Value = "updated $prev";

      var result = cmd.Run();
      item.Reload();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringContaining("name"));
      Assert.That(result.Message, Is.StringContaining("updated " + name));
      Assert.That(item.Name, Is.EqualTo("updated " + name));
    }

    [Test]
    public void SetTemplateByID()
    {
      var salt = DateUtil.IsoNow;
      var name = "templateitem" + salt;
      var item = _testRoot.Add(name, _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate]);

      var cmd = new Cmd.SetAttribute();
      InitCommand(cmd);

      _context.CurrentItem = item;
      cmd.Attribute = "template";
      cmd.Value = "{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}";
      
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringContaining("templateid"));
      Assert.That(result.Message, Is.StringContaining("{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}"));
      Assert.That(_context.CurrentItem.Template.Name, Is.EqualTo("Folder"));
    }

    [Test]
    public void NoParameters()
    {
      var cmd = new Cmd.SetAttribute();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    // todo: Cover all code execution paths of SetAttribute Command
  }
}
