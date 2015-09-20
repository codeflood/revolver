using NUnit.Framework;
using Revolver.Core;
using Sitecore;
using Sitecore.Data.Items;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("WorkflowCommand")]
  public class WorkflowCommand : BaseCommandTest
  {
    private Item _contentRootItem = null;
    private Item _publishableItem = null;
    private Item _itemInWorkflow = null;
    private Item _noWorkflow = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      // There is no workflow provider in web. Ensure tests are run in master
      Sitecore.Context.Site = Sitecore.Configuration.Factory.GetSite(Sitecore.Constants.ShellSiteName);

      var db = Sitecore.Configuration.Factory.GetDatabase("master");
      InitContent(db);
    }

    [SetUp]
    public void SetUp()
    {
      _contentRootItem = TestUtil.CreateContentFromFile("TestResources\\items in workflow.xml", _testRoot);
      _publishableItem = _contentRootItem.Axes.GetChild("publishable");
      _itemInWorkflow = _contentRootItem.Axes.GetChild("in draft");
      _noWorkflow = _contentRootItem.Axes.GetChild("no workflow");

      _noWorkflow.Editing.BeginEdit();
      _noWorkflow[FieldIDs.Workflow] = string.Empty;
      _noWorkflow[FieldIDs.WorkflowState] = string.Empty;
      _noWorkflow.Editing.EndEdit();
    }

    [TearDown]
    public void TearDown()
    {
      if (_contentRootItem != null)
      {
        _contentRootItem.Delete();
      }
    }

    [Test]
    public void List_NoneAvailable()
    {
      var cmd = new Cmd.WorkflowCommand();
      InitCommand(cmd);

      _context.CurrentItem = _publishableItem;
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.IsTrue(result.Message.Contains("No commands available"));
    }

    [Test]
    public void List_CommandsAvailable()
    {
      var cmd = new Cmd.WorkflowCommand();
      InitCommand(cmd);

      _context.CurrentItem = _itemInWorkflow;
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual("Submit\r\n__OnSave\r\n", result.Message);
    }

    [Test]
    public void List_NoWorkflow()
    {
      var cmd = new Cmd.WorkflowCommand();
      InitCommand(cmd);

      _context.CurrentItem = _noWorkflow;
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.IsTrue(result.Message.Contains("No commands available"));
    }

    [Test]
    public void Execute_ValidCommand()
    {
      var cmd = new Cmd.WorkflowCommand();
      InitCommand(cmd);
      cmd.Command = "submit";

      _context.CurrentItem = _itemInWorkflow;
      var result = cmd.Run();
      _itemInWorkflow.Reload();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual("{46DA5376-10DC-4B66-B464-AFDAA29DE84F}", _itemInWorkflow[FieldIDs.WorkflowState]);
    }

    [Test]
    public void Execute_InvalidCommand()
    {
      var cmd = new Cmd.WorkflowCommand();
      InitCommand(cmd);
      cmd.Command = "blah";

      _context.CurrentItem = _itemInWorkflow;
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }

    [Test]
    public void Execute_NoWorkflow()
    {
      var cmd = new Cmd.WorkflowCommand();
      InitCommand(cmd);
      cmd.Command = "submit";

      _context.CurrentItem = _noWorkflow;
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }
  }
}