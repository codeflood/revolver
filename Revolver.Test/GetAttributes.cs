using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data.Items;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
	[TestFixture]
	[Category("GetAttributes")]
	public class GetAttributes : BaseCommandTest
	{
    private Item _testContent = null;

		[TestFixtureSetUp]
		public void Init()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      InitContent();
      _testContent = TestUtil.CreateContentFromFile("TestResources\\find content.xml", _testRoot);
    }

    [Test]
    public void AllAttributes()
    {
      var cmd = new Cmd.GetAttributes();
      InitCommand(cmd);

      _context.CurrentItem = _testContent;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringMatching(@".*id\s+" + _testContent.ID.ToString() + ".*"));
      Assert.That(result.Message, Is.StringMatching(@".*name\s+" + _testContent.Name + ".*"));
      Assert.That(result.Message, Is.StringMatching(@".*key\s+" + _testContent.Key + ".*"));
      Assert.That(result.Message, Is.StringMatching(@".*template\s+" + _testContent.Template.InnerItem.Paths.FullPath + ".*"));
      Assert.That(result.Message, Is.StringMatching(@".*templateid\s+" + _testContent.TemplateID.ToString() + ".*"));
      Assert.That(result.Message, Is.StringMatching(@".*branch\s+" + Revolver.Core.Constants.NotDefinedLiteral + ".*"));
      Assert.That(result.Message, Is.StringMatching(@".*branchid\s+" + _testContent.BranchId.ToString() + ".*"));
      Assert.That(result.Message, Is.StringMatching(@".*language\s+.*" + _testContent.Language.Name + ".*"));
      Assert.That(result.Message, Is.StringMatching(@".*version\s+" + _testContent.Version.Number + ".*"));
    }

    [Test]
    public void IDOnly()
    {
      var cmd = new Cmd.GetAttributes();
      InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Attribute = "id";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(_testContent.ID.ToString()));
    }

    [Test]
    public void NameOnlyRelative()
    {
      var cmd = new Cmd.GetAttributes();
      InitCommand(cmd);

      _context.CurrentItem = _testContent.Parent;
      cmd.Attribute = "NAME";
      cmd.Path = _testContent.Name;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(_testContent.Name));
    }

    [Test]
    public void TemplateOnlyAbsolute()
    {
      var cmd = new Cmd.GetAttributes();
      InitCommand(cmd);

      _context.CurrentItem = _context.CurrentDatabase.GetRootItem();
      cmd.Attribute = "templatE";
      cmd.Path = _testContent.Paths.FullPath;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(_testContent.Template.InnerItem.Paths.FullPath));
    }

    [Test]
    public void InvalidAttribute()
    {
      var cmd = new Cmd.GetAttributes();
      InitCommand(cmd);

      _context.CurrentItem = _testContent;
      cmd.Attribute = "bler";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void KeyOnlyRelativeAncestor()
    {
      var cmd = new Cmd.GetAttributes();
      InitCommand(cmd);

      _context.CurrentItem = _testContent.Axes.SelectSingleItem("Proteus/child1");
      cmd.Attribute = "key";
      cmd.Path = "../..";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(_testContent.Key));
    }
	}
}
