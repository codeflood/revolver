using NUnit.Framework;
using Revolver.Core;
using Revolver.Core.Exceptions;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using System.Collections.Generic;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
	[TestFixture]
	[Category("FindItems")]
	public class FindItems : BaseCommandTest
	{
	  private Item _testContent = null;
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
        _branch = TestUtil.CreateContentFromFile("TestResources\\branch.xml", branchHome, false);
      }
      else
      {
        _branch = sampleBranch;
      }

      _testContent = TestUtil.CreateContentFromFile("TestResources\\find content.xml", _testRoot);
		}

	  [Test]
	  public void MissingCommand()
	  {
	    var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
	    cmd.Template = Constants.Paths.DocTemplate;

	    var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
	  }

    [Test]
    public void CommandWithStatsOnly()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Template = Constants.Paths.DocTemplate;
      cmd.StatisticsOnly = true;
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ValidTemplateIDMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Template = Constants.IDs.DocTemplateId.ToString();
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Triton"));
      Assert.That(result.Message, Contains.Substring("Thalassa"));
      Assert.That(result.Message, Contains.Substring("Proteus"));
      Assert.That(result.Message, Contains.Substring("Neso"));
      Assert.That(result.Message, Contains.Substring("Found 4 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ValidTemplatePathMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Template = Constants.Paths.FolderTemplate;
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Laomedeia"));
      Assert.That(result.Message, Contains.Substring("Found 1 item"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ValidTemplateIDNoMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Template = Sitecore.TemplateIDs.Alias.ToString();
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Found 0 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void InvalidTemplateID()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Template = ID.NewID.ToString();
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ValidBranchIDMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Branch = _branch.ID.ToString();
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Proteus"));
      Assert.That(result.Message, Contains.Substring("Found 1 item"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ValidBranchIDNoMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent.Parent;
      cmd.Branch = _branch.ID.ToString();
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Found 0 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.Parent.ID));
    }

    [Test]
    public void ValidBranchPathMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Branch = Constants.Paths.Branch;
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Proteus"));
      Assert.That(result.Message, Contains.Substring("Found 1 item"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void InvalidBranchPath()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Branch = "/does/not/exist";
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ValidAttributeMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.FindByAttribute = new KeyValuePair<string, string>("name", "neso");
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Neso"));
      Assert.That(result.Message, Contains.Substring("Found 1 item"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ValidAttributeNoMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.FindByAttribute = new KeyValuePair<string, string>("name", "nothing here");
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Found 0 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void InvalidAttribute()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.FindByAttribute = new KeyValuePair<string, string>("not a real attribute", "nothing here");
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Found 0 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ValidAttributeCaseSensitiveNoMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.FindByAttribute = new KeyValuePair<string, string>("name", "neso");
      cmd.FindByAttributeCaseSensitive = true;
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Found 0 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ValidFieldMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.FindByField = new KeyValuePair<string, string>("text", "Ipsum");
      cmd.Recursive = true;
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Triton"));
      Assert.That(result.Message, Contains.Substring("Sao"));
      Assert.That(result.Message, Contains.Substring("Found 2 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ValidFieldNoMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.FindByField = new KeyValuePair<string, string>("text", "foobar");
      cmd.Recursive = true;
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Found 0 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void WildcardFieldMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.FindByField = new KeyValuePair<string, string>("*", "Ipsum");
      cmd.Recursive = true;
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Triton"));
      Assert.That(result.Message, Contains.Substring("Sao"));
      Assert.That(result.Message, Contains.Substring("Proteus"));
      Assert.That(result.Message, Contains.Substring("Naiad"));
      Assert.That(result.Message, Contains.Substring("Found 4 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ValidFieldCaseSensitiveMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.FindByField = new KeyValuePair<string, string>("text", "DoloR");
      cmd.Recursive = true;
      cmd.FindByFieldCaseSensitive = true;
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Triton"));
      Assert.That(result.Message, Contains.Substring("Found 1 item"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ValidExpressionMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      
      // todo: Ensure the dates are valid for Sitecore 6.x
      cmd.Expression = "@__created > (2014-05-03) as date";
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Triton"));
      Assert.That(result.Message, Contains.Substring("Thalassa"));
      Assert.That(result.Message, Contains.Substring("Proteus"));
      Assert.That(result.Message, Contains.Substring("Neso"));
      Assert.That(result.Message, Contains.Substring("Laomedeia"));
      Assert.That(result.Message, Contains.Substring("Found 5 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ValidExpressionAdvancedMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Expression = "@text ? ipsum or @title ? ipsum";
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Triton"));
      Assert.That(result.Message, Contains.Substring("Proteus"));
      Assert.That(result.Message, Contains.Substring("Found 2 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ValidExpressionNoMatches()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      // todo: Ensure the dates are valid for Sitecore 6.x
      cmd.Expression = "@__created < (2014-05-03) as date";
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Found 0 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    [ExpectedException(typeof(ExpressionException))]
    public void InvalidExpression()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Expression = "@__created bnb";
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Found 0 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void NoStats()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Template = Constants.IDs.DocTemplateId.ToString();
      cmd.NoStatistics = true;
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Triton"));
      Assert.That(result.Message, Contains.Substring("Thalassa"));
      Assert.That(result.Message, Contains.Substring("Proteus"));
      Assert.That(result.Message, Contains.Substring("Neso"));
      Assert.That(result.Message, Is.Not.ContainsSubstring("Found"));
      Assert.That(result.Message, Is.Not.ContainsSubstring("item"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void StatsOnly()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Template = Constants.IDs.DocTemplateId.ToString();
      cmd.StatisticsOnly = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("4"));
      Assert.That(result.Message, Is.Not.ContainsSubstring("Triton"));
      Assert.That(result.Message, Is.Not.ContainsSubstring("Thalassa"));
      Assert.That(result.Message, Is.Not.ContainsSubstring("Neso"));
      Assert.That(result.Message, Is.Not.ContainsSubstring("Proteus"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void NoStandardValues()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.FindByField = new KeyValuePair<string, string>("__Default workflow", ".+");
      cmd.NoStandardValues = true;
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Found 0 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void NoStandardValuesRecipricol()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.FindByField = new KeyValuePair<string, string>("__Default workflow", ".+");
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Found 4 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void BySingleID()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Ids = ItemIDs.ContentRoot.ToString();
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("content"));
      Assert.That(result.Message, Contains.Substring("Found 1 item"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ByMultipleIDs()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Ids = string.Join("|", new[] { ItemIDs.ContentRoot.ToString(), ItemIDs.SystemRoot.ToString(), _testContent.ID.ToString() });
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("content"));
      Assert.That(result.Message, Contains.Substring("system"));
      Assert.That(result.Message, Contains.Substring(_testContent.Name));
      Assert.That(result.Message, Contains.Substring("Found 3 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ByMalformedID()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Ids = "not an id";
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Found 0 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void AbsolutePath()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent.Database.GetRootItem();
      cmd.Template = Constants.IDs.DocTemplateId.ToString();
      cmd.Path = _testContent.Paths.FullPath;
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Triton"));
      Assert.That(result.Message, Contains.Substring("Thalassa"));
      Assert.That(result.Message, Contains.Substring("Proteus"));
      Assert.That(result.Message, Contains.Substring("Neso"));
      Assert.That(result.Message, Contains.Substring("Found 4 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.Database.GetRootItem().ID));
    }

    [Test]
    public void RelativePath()
    {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent.Parent;
      cmd.Template = Constants.IDs.DocTemplateId.ToString();
      cmd.Path = _testContent.Name;
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Triton"));
      Assert.That(result.Message, Contains.Substring("Thalassa"));
      Assert.That(result.Message, Contains.Substring("Proteus"));
      Assert.That(result.Message, Contains.Substring("Neso"));
      Assert.That(result.Message, Contains.Substring("Found 4 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.Parent.ID));
    }

	  [Test]
	  public void MultipleFilters()
	  {
      var cmd = new Cmd.FindItems();
      base.InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Template = Constants.IDs.DocTemplateId.ToString();
      cmd.FindByField = new KeyValuePair<string, string>("text", "ipsum");
	    cmd.Recursive = true;
      cmd.Command = "ga -a name";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Triton"));
      Assert.That(result.Message, Contains.Substring("Sao"));
      Assert.That(result.Message, Contains.Substring("Found 2 items"));
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
	  }
	}
}
