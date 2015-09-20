using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ChangeTemplate")]
  public class ChangeTemplate : BaseCommandTest
  {
    Item _testDocumentItem = null;
    Item _testFolderItem = null;
    Cmd.ChangeTemplate _command = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      using (new SecurityDisabler())
      {
        InitContent();
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

    [SetUp]
    public void SetUp()
    {
      using (new SecurityDisabler())
      {
        _testDocumentItem = _testRoot.Add("test item", _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate]);
        using (new EditContext(_testDocumentItem))
        {
          _testDocumentItem["title"] = "lorem";
          _testDocumentItem["text"] = "ipsum";
        }

        _testFolderItem = _testRoot.Add("test folder", _context.CurrentDatabase.Templates[Constants.Paths.FolderTemplate]);
      }

      _command = new Cmd.ChangeTemplate();
      InitCommand(_command);
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
    public void MissingTemplate()
    {
      using (new SecurityDisabler())
      {
        var result = _command.Run();
        Assert.AreEqual(CommandStatus.Failure, result.Status);
        Assert.AreEqual("Required parameter 'template' is missing", result.Message);
      }
    }
    
    [Test]
    public void ToCompatibleTemplate()
    {
      using (new SecurityDisabler())
      {
        _context.CurrentItem = _testFolderItem;

        _command.Template = Constants.Paths.DocTemplate;
        var result = _command.Run();
        _testFolderItem.Reload();

        Assert.AreEqual(CommandStatus.Success, result.Status);
        Assert.AreEqual("Sample Item", _testFolderItem.TemplateName);
      }
    }

    [Test]
    public void ToIncompatibleTemplate()
    {
      using (new SecurityDisabler())
      {
        _context.CurrentItem = _testDocumentItem;

        _command.Template = "system/publishing target";
        var result = _command.Run();

        Assert.AreEqual(CommandStatus.Failure, result.Status);
        Assert.IsTrue(result.Message.Contains("Incompatible template"));
        Assert.IsTrue(result.Message.Contains("Title"));
        Assert.IsTrue(result.Message.Contains("Text"));

        // Ensure missing field names are displayed
        Assert.IsTrue(result.Message.Contains("Title"));
      }
    }

    [Test]
    public void ForceIncompatibleTemplate()
    {
      using (new SecurityDisabler())
      {
        _context.CurrentItem = _testDocumentItem;

        _command.Template = "system/publishing target";
        _command.Force = true;
        var result = _command.Run();
        _testDocumentItem.Reload();

        Assert.AreEqual(CommandStatus.Success, result.Status);
        Assert.AreEqual("Folder", _testFolderItem.TemplateName);
      }
    }

    [Test]
    public void ToCompatibleTemplateRelativePath()
    {
      using (new SecurityDisabler())
      {
        _context.CurrentItem = _testRoot;

        _command.Template = Constants.Paths.DocTemplate;
        _command.Path = _testFolderItem.Name;
        var result = _command.Run();
        _testFolderItem.Reload();

        Assert.AreEqual(CommandStatus.Success, result.Status);
        Assert.AreEqual("Sample Item", _testFolderItem.TemplateName);
      }
    }

    [Test]
    public void ToCompatibleTemplateAbsolutePath()
    {
      using (new SecurityDisabler())
      {
        _context.CurrentItem = _context.CurrentDatabase.GetRootItem();

        _command.Template = Constants.Paths.DocTemplate;
        _command.Path = _testFolderItem.Paths.FullPath;
        var result = _command.Run();
        _testFolderItem.Reload();

        Assert.AreEqual(CommandStatus.Success, result.Status);
        Assert.AreEqual("Sample Item", _testFolderItem.TemplateName);
      }
    }

    [Test]
    public void InvalidPath()
    {
      using (new SecurityDisabler())
      {
        _context.CurrentItem = _context.CurrentDatabase.GetRootItem();

        _command.Template = Constants.Paths.DocTemplate;
        _command.Path = "/sitecore/blah/doesnt/exist";
        var result = _command.Run();

        Assert.AreEqual(CommandStatus.Failure, result.Status);
      }
    }

    [Test]
    public void InvalidTemplate()
    {
      using (new SecurityDisabler())
      {
        _context.CurrentItem = _testFolderItem;

        _command.Template = "not-existing-template";
        var result = _command.Run();

        Assert.AreEqual(CommandStatus.Failure, result.Status);
        Assert.IsTrue(result.Message.Contains("Failed to find the template"));
      }
    }

    [Test]
    public void ToCompatibleTemplatePathAsId()
    {
      using (new SecurityDisabler())
      {
        _context.CurrentItem = _context.CurrentDatabase.GetRootItem();

        _command.Template = Constants.Paths.DocTemplate;
        _command.Path = _testFolderItem.ID.ToString();
        var result = _command.Run();
        _testFolderItem.Reload();

        Assert.AreEqual(CommandStatus.Success, result.Status);
        Assert.AreEqual("Sample Item", _testFolderItem.TemplateName);
      }
    }
  }
}
