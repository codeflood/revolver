using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data.Items;
using System;
using System.Threading;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ListPublishQueue")]
  public class ListPublishQueue : BaseCommandTest
  {
    TemplateItem _template = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      _context.CurrentDatabase = Sitecore.Configuration.Factory.GetDatabase("master");
      InitContent(_context.CurrentDatabase);
      _template = _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate];
    }

    [Test]
    public void InvalidDatabase()
    {
      var cmd = new Cmd.ListPublishQueue();
      InitCommand(cmd);
      cmd.DatabaseName = "blah";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void EarlyDateFilter()
    {
      var cmd = new Cmd.ListPublishQueue();
      InitCommand(cmd);
      cmd.ToDate = new DateTime(1970, 1, 1);
      cmd.NoStats = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.Empty);
    }

    [Test]
    public void _1Update()
    {
      var fromDate = DateTime.Now;
      var update1 = _testRoot.Add("update1", _template);

      var cmd = new Cmd.ListPublishQueue();
      InitCommand(cmd);
      cmd.FromDate = fromDate;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("1 item found"));
      Assert.That(result.Message, Contains.Substring(update1.Paths.FullPath));
    }

    [Test]
    public void _3Updates()
    {
      var fromDate = DateTime.Now;
      Thread.Sleep(300);
      var update2 = _testRoot.Add("update2", _template);
      var update3 = _testRoot.Add("update3", _template);
      var update4 = _testRoot.Add("update4", _template);

      var cmd = new Cmd.ListPublishQueue();
      InitCommand(cmd);
      cmd.FromDate = fromDate;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("3 items found"));
      Assert.That(result.Message, Contains.Substring(update2.Paths.FullPath));
      Assert.That(result.Message, Contains.Substring(update3.Paths.FullPath));
      Assert.That(result.Message, Contains.Substring(update4.Paths.FullPath));
    }

    [Test]
    public void _3UpdatesDateFilter()
    {
      var update5 = _testRoot.Add("update5", _template);
      Thread.Sleep(300);
      var fromDate = DateTime.Now;
      var update6 = _testRoot.Add("update6", _template);
      var update7 = _testRoot.Add("update7", _template);

      var cmd = new Cmd.ListPublishQueue();
      InitCommand(cmd);
      cmd.FromDate = fromDate;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("2 items found"));
      Assert.That(result.Message, Contains.Substring(update6.ID.ToString()));
      Assert.That(result.Message, Contains.Substring(update7.ID.ToString()));
    }

    [Test]
    public void NoStats()
    {
      var fromDate = DateTime.Now;
      var update1 = _testRoot.Add("update1", _template);

      var cmd = new Cmd.ListPublishQueue();
      InitCommand(cmd);
      cmd.FromDate = fromDate;
      cmd.NoStats = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.Not.Contains("item found"));
      Assert.That(result.Message, Contains.Substring(update1.Paths.FullPath));
    }

    [Test]
    public void IdOnly()
    {
      var fromDate = DateTime.Now;
      var update1 = _testRoot.Add("update1", _template);

      var cmd = new Cmd.ListPublishQueue();
      InitCommand(cmd);
      cmd.FromDate = fromDate;
      cmd.IdOnly = true;
      cmd.NoStats = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.Not.Contains("item found"));
      Assert.That(result.Message, Is.Not.Contains(update1.Paths.FullPath));
      Assert.That(result.Message, Contains.Substring(update1.ID.ToString()));
    }
  }
}