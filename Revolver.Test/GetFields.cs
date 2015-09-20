using NUnit.Framework;
using Revolver.Core;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
	[TestFixture]
	[Category("GetFields")]
	public class GetFields : BaseCommandTest
	{
	  Item _testItem = null;
	  private const string TITLE = "Lorem ipsum dolor sit amet";
    private const string TEXT = "consectetur adipisicing elit, sed do eiusmod tempor incididunt";

		[TestFixtureSetUp]
		public void Init()
		{
		  Sitecore.Context.IsUnitTesting = true;
		  Sitecore.Context.SkipSecurityInUnitTests = true;

      InitContent();

		  _testItem = _testRoot.Add("test item", _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate]);
		  using (new EditContext(_testItem))
		  {
        _testItem["title"] = TITLE;
        _testItem["text"] = TEXT;
		  }
		}

	  [Test]
	  public void GetAllFields()
	  {
	    var cmd = new Cmd.GetFields();
      InitCommand(cmd);

      _context.CurrentItem = _testItem;

	    var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Title"));
      Assert.That(result.Message, Contains.Substring(TITLE));
      Assert.That(result.Message, Contains.Substring("Text"));
      Assert.That(result.Message, Contains.Substring("consectetur adipisicing elit, sed do eiusmod tempor incididunt"));
      Assert.That(result.Message, Contains.Substring("__Created"));
      Assert.That(result.Message, Contains.Substring("__Created by"));
	  }

    [Test]
    public void GetSingleValidField()
    {
      var cmd = new Cmd.GetFields();
      InitCommand(cmd);

      _context.CurrentItem = _testItem;
      cmd.FieldName = "title";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(TITLE));
    }

    [Test]
    public void GetSingleInvalidField()
    {
      var cmd = new Cmd.GetFields();
      InitCommand(cmd);

      _context.CurrentItem = _testItem;
      cmd.FieldName = "a field that doesn't exist";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void GetSingleValidFieldById()
    {
      var cmd = new Cmd.GetFields();
      InitCommand(cmd);

      _context.CurrentItem = _testItem;
      cmd.FieldName = _testItem.Fields["text"].ID.ToString();

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(TEXT));
    }

    [Test]
    public void GetSingleInvalidFieldById()
    {
      var cmd = new Cmd.GetFields();
      InitCommand(cmd);

      _context.CurrentItem = _testItem;
      cmd.FieldName = ID.NewID.ToString();

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void GetSingleValidFieldWithoutSV()
    {
      var cmd = new Cmd.GetFields();
      InitCommand(cmd);

      _context.CurrentItem = _testItem;
      cmd.FieldName = "__default workflow";
      cmd.NoStandardValues = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.Empty);
    }

    [Test]
    public void GetSingleValidFieldWithoutSVRecipricol()
    {
      var cmd = new Cmd.GetFields();
      InitCommand(cmd);

      _context.CurrentItem = _testItem;
      cmd.FieldName = "__default workflow";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(Constants.IDs.SampleWorkflowId.ToString()));
    }

    [Test]
    public void GetSingleFieldAbsolutePathByID()
    {
      var cmd = new Cmd.GetFields();
      InitCommand(cmd);

      _context.CurrentItem = _testItem.Database.GetRootItem();
      cmd.FieldName = "text";
      cmd.Path = _testItem.ID.ToString();

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(TEXT));
    }

    [Test]
    public void GetSingleFieldAbsolutePath()
    {
      var cmd = new Cmd.GetFields();
      InitCommand(cmd);

      _context.CurrentItem = _testItem.Database.GetItem(ItemIDs.ContentRoot);
      cmd.FieldName = "Text";
      cmd.Path = _testItem.Paths.FullPath;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(TEXT));
    }

    [Test]
    public void GetSingleFieldRelativePath()
    {
      var cmd = new Cmd.GetFields();
      InitCommand(cmd);

      _context.CurrentItem = _testItem.Parent.Parent;
      cmd.FieldName = "text";
      cmd.Path = _testItem.Parent.Name + "/" + _testItem.Name;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(TEXT));
    }
	}
}
