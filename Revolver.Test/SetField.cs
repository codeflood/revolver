using NUnit.Framework;
using Revolver.Core;
using Sitecore;
using Sitecore.Data.Items;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("SetField")]
  public class SetField : BaseCommandTest
  {
    [TestFixtureSetUp]
    public void Init()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      InitContent();
    }

    [Test]
    public void MissingField()
    {
      var cmd = new Cmd.SetField();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Value = "lorem";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void MissingValue()
    {
      var cmd = new Cmd.SetField();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;
      cmd.Field = "title";
      cmd.Value = null;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void ClearField()
    {
      var item = _testRoot.Add("itemTOClearField" + DateUtil.IsoNow, _context.CurrentDatabase.Templates[Constants.IDs.DocTemplateId]);
      using (new EditContext(item))
      {
        item["title"] = "lorem";
      }

      var cmd = new Cmd.SetField();
      InitCommand(cmd);

      _context.CurrentItem = item;
      cmd.Field = "title";
      cmd.Reset = true;
      cmd.NoVersion = true;

      var result = cmd.Run();

      item.Reload();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(item["title"], Is.EqualTo("$name")); // This is the standard value fir the title field on the template
    }

    [Test]
    public void InvalidField()
    {
      var item = _testRoot.Add("itemInvalidField" + DateUtil.IsoNow, _context.CurrentDatabase.Templates[Constants.IDs.DocTemplateId]);

      var cmd = new Cmd.SetField();
      InitCommand(cmd);

      _context.CurrentItem = item;
      cmd.Field = "bler";
      cmd.Value = "lorem";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void Ideal()
    {
      var item = _testRoot.Add("itemIdeal" + DateUtil.IsoNow, _context.CurrentDatabase.Templates[Constants.IDs.DocTemplateId]);
      using (new EditContext(item))
      {
        item["title"] = "lorem";
      }

      var cmd = new Cmd.SetField();
      InitCommand(cmd);

      _context.CurrentItem = item;
      cmd.Field = "title";
      cmd.Value = "ipsum";

      var result = cmd.Run();
      item = item.Versions.GetLatestVersion();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(item["title"], Is.EqualTo("ipsum"));
      Assert.That(item.Version.Number, Is.EqualTo(2));
      Assert.That(result.Message, Is.StringContaining("title"));
      Assert.That(result.Message, Is.StringContaining("ipsum"));
    }

    [Test]
    public void IdealWithPathRelative()
    {
      var item = _testRoot.Add("itemIdealWithPathRelative" + DateUtil.IsoNow, _context.CurrentDatabase.Templates[Constants.IDs.DocTemplateId]);
      using (new EditContext(item))
      {
        item["title"] = "lorem";
      }

      var cmd = new Cmd.SetField();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot.Parent;
      cmd.Field = "title";
      cmd.Value = "dolor";
      cmd.Path = _testRoot.Name + "/" + item.Name;

      var result = cmd.Run();
      item = item.Versions.GetLatestVersion();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(item["title"], Is.EqualTo("dolor"));
      Assert.That(item.Version.Number, Is.EqualTo(2));
      Assert.That(result.Message, Is.StringContaining("title"));
      Assert.That(result.Message, Is.StringContaining("dolor"));
    }

    [Test]
    public void IdealWithToken()
    {
      var item = _testRoot.Add("itemIdealWithToken" + DateUtil.IsoNow, _context.CurrentDatabase.Templates[Constants.IDs.DocTemplateId]);
      using (new EditContext(item))
      {
        item["title"] = "lorem";
      }

      var cmd = new Cmd.SetField();
      InitCommand(cmd);

      _context.CurrentItem = item;
      cmd.Field = "title";
      cmd.Value = "update $prev";

      var result = cmd.Run();
      item = item.Versions.GetLatestVersion();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(item["title"], Is.EqualTo("update lorem"));
      Assert.That(item.Version.Number, Is.EqualTo(2));
      Assert.That(result.Message, Is.StringContaining("title"));
      Assert.That(result.Message, Is.StringContaining("update lorem"));
    }

    [Test]
    public void IdealNoVersion()
    {
      var item = _testRoot.Add("itemIdealNoVersion" + DateUtil.IsoNow, _context.CurrentDatabase.Templates[Constants.IDs.DocTemplateId]);
      using (new EditContext(item))
      {
        item["title"] = "lorem";
      }

      var cmd = new Cmd.SetField();
      InitCommand(cmd);

      _context.CurrentItem = item;
      cmd.Field = "title";
      cmd.Value = "amed";
      cmd.NoVersion = true;

      var result = cmd.Run();
      item = item.Versions.GetLatestVersion();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(item["title"], Is.EqualTo("amed"));
      Assert.That(item.Version.Number, Is.EqualTo(1));
      Assert.That(result.Message, Is.StringContaining("title"));
      Assert.That(result.Message, Is.StringContaining("amed"));
    }

    [Test]
    public void ResetField()
    {
      var template = _context.CurrentDatabase.Templates[Constants.IDs.DocTemplateId];
      var item = _testRoot.Add("itemResetField" + DateUtil.IsoNow, template);
      using (new EditContext(item))
      {
        item["title"] = "lorem";
      }

      var cmd = new Cmd.SetField();
      InitCommand(cmd);

      _context.CurrentItem = item;
      cmd.Field = "title";
      cmd.Reset = true;

      var result = cmd.Run();
      item = item.Versions.GetLatestVersion();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(item["title"], Is.EqualTo(template.StandardValues["title"]));
      Assert.That(item.Version.Number, Is.EqualTo(2));
    }

    [Test]
    public void IdealNoStats()
    {
      var item = _testRoot.Add("itemIdealNoStats" + DateUtil.IsoNow, _context.CurrentDatabase.Templates[Constants.IDs.DocTemplateId]);
      using (new EditContext(item))
      {
        item["title"] = "lorem";
      }

      var cmd = new Cmd.SetField();
      InitCommand(cmd);

      _context.CurrentItem = item;
      cmd.Field = "title";
      cmd.Value = "ipsum";
      cmd.NoStats = true;
      cmd.NoVersion = true; // A new version would get a different updated date

      var updatedDate = item.Statistics.Updated;

      var result = cmd.Run();
      item.Reload();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(item["title"], Is.EqualTo("ipsum"));
      Assert.That(item.Version.Number, Is.EqualTo(1));
      Assert.That(item.Statistics.Updated, Is.EqualTo(updatedDate));
      Assert.That(result.Message, Is.StringContaining("title"));
      Assert.That(result.Message, Is.StringContaining("ipsum"));
    }

    [Test]
    public void IdealWithPathID()
    {
      var item = _testRoot.Add("itemIdealWithPathID" + DateUtil.IsoNow, _context.CurrentDatabase.Templates[Constants.IDs.DocTemplateId]);
      using (new EditContext(item))
      {
        item["text"] = "lorem";
      }

      var cmd = new Cmd.SetField();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot.Parent;
      cmd.Field = "text";
      cmd.Value = "dolor";
      cmd.Path = item.ID.ToString();

      var result = cmd.Run();
      item = item.Versions.GetLatestVersion();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(item["text"], Is.EqualTo("dolor"));
      Assert.That(item.Version.Number, Is.EqualTo(2));
      Assert.That(result.Message, Is.StringContaining("text"));
      Assert.That(result.Message, Is.StringContaining("dolor"));
    }
  }
}
