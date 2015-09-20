using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data.Items;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
	[TestFixture]
	[Category("Links")]
	public class Links : BaseCommandTest
	{
	  private Item _nolinks = null;
    private Item _outlink = null;
    private Item _inlink = null;
    private Item _badlink = null;
    private Item _inoutlink = null;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

			InitContent();
		  var itemsRoot = TestUtil.CreateContentFromFile("TestResources\\links.xml", _testRoot, false);
      _nolinks = itemsRoot.Axes.GetChild("nolinks");
      _outlink = itemsRoot.Axes.GetChild("outlink");
      _inlink = itemsRoot.Axes.GetChild("inlink");
      _badlink = itemsRoot.Axes.GetChild("badlink");
      _inoutlink = itemsRoot.Axes.GetChild("outinlink");
		}

	  [Test]
	  public void NoAdditionalLinks()
	  {
	    var cmd = new Cmd.Links();
      InitCommand(cmd);

	    _context.CurrentItem = _nolinks;

	    var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      // Even with no added links, the item will link to it's template
      Assert.That(result.Message, Contains.Substring(Constants.Paths.DocTemplate));
	  }

    [Test]
    public void OutgoingLink()
    {
      var cmd = new Cmd.Links();
      InitCommand(cmd);

      _context.CurrentItem = _outlink;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      // Even with no added links, the item will link to it's template
      Assert.That(result.Message, Contains.Substring(Constants.Paths.DocTemplate));
      Assert.That(result.Message, Contains.Substring(_inlink.Paths.FullPath));
    }

    [Test]
    public void IncomingLink()
    {
      var cmd = new Cmd.Links();
      InitCommand(cmd);

      _context.CurrentItem = _inlink;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      // Even with no added links, the item will link to it's template
      Assert.That(result.Message, Contains.Substring(Constants.Paths.DocTemplate));
      Assert.That(result.Message, Contains.Substring(_outlink.Paths.FullPath));
    }

    [Test]
    public void BadLink()
    {
      var cmd = new Cmd.Links();
      InitCommand(cmd);

      _context.CurrentItem = _badlink;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      // Even with no added links, the item will link to it's template
      Assert.That(result.Message, Contains.Substring(Constants.Paths.DocTemplate));
      Assert.That(result.Message, Contains.Substring("out "));
      Assert.That(result.Message, Contains.Substring(" no "));
    }

    [Test]
    public void IncomingAndOutgoing_ShowIncomingOnly()
    {
      var cmd = new Cmd.Links();
      InitCommand(cmd);

      _context.CurrentItem = _inoutlink;
      cmd.ShowIncomingLinks = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      Assert.That(result.Message, Is.Not.Contains(" out "));
      Assert.That(result.Message, Contains.Substring(_outlink.Paths.FullPath));
    }

    [Test]
    public void IncomingAndOutgoing_ShowOutgoingOnly()
    {
      var cmd = new Cmd.Links();
      InitCommand(cmd);

      _context.CurrentItem = _inoutlink;
      cmd.ShowOutgoingLinks = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      // Even with no added links, the item will link to it's template
      Assert.That(result.Message, Contains.Substring(Constants.Paths.DocTemplate));
      Assert.That(result.Message, Is.Not.Contains(" in "));
      Assert.That(result.Message, Contains.Substring(_inlink.Paths.FullPath));
    }

    [Test]
    public void SourceField()
    {
      var cmd = new Cmd.Links();
      InitCommand(cmd);

      _context.CurrentItem = _inlink;
      cmd.OriginFieldName = "text";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      // Even with no added links, the item will link to it's template
      Assert.That(result.Message, Is.Not.Contains(Constants.Paths.DocTemplate));
      Assert.That(result.Message, Contains.Substring(_outlink.Paths.FullPath));
    }

    [Test]
    public void OutgoingLink_IdList()
    {
      var cmd = new Cmd.Links();
      InitCommand(cmd);

      _context.CurrentItem = _outlink;
      cmd.ShowOutgoingLinks = true;
      cmd.OutputIds = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      Assert.That(result.Message, Is.Not.Contains(_inlink.Paths.FullPath));
      Assert.That(result.Message, Contains.Substring(_inlink.ID.ToString()));
    }

    [Test]
    public void OutgoingLink_Relative()
    {
      var cmd = new Cmd.Links();
      InitCommand(cmd);

      _context.CurrentItem = _outlink.Parent.Parent;
      cmd.ShowOutgoingLinks = true;
      cmd.Path = _outlink.Parent.Name + "/" + _outlink.Name;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      // Even with no added links, the item will link to it's template
      Assert.That(result.Message, Contains.Substring(Constants.Paths.DocTemplate));
      Assert.That(result.Message, Contains.Substring(_inlink.Paths.FullPath));
    }

    [Test]
    public void OutgoingLink_Absolute()
    {
      var cmd = new Cmd.Links();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.ShowOutgoingLinks = true;
      cmd.Path = _outlink.Paths.FullPath;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      // Even with no added links, the item will link to it's template
      Assert.That(result.Message, Contains.Substring(Constants.Paths.DocTemplate));
      Assert.That(result.Message, Contains.Substring(_inlink.Paths.FullPath));
    }
	}
}
