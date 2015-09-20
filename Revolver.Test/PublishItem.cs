using NUnit.Framework;
using Revolver.Core;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Publishing;
using System.Linq;

namespace Revolver.Test
{
  [TestFixture]
  [Category("PublishItem")]
  public class PublishItem : BaseCommandTest
  {
    private Item _publishableItem = null;
    private Item _itemInWorkflow = null;
    private Item _noWorkflow = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      // There is no workflow provider in web. Ensure tests are run in master
      var db = Sitecore.Configuration.Factory.GetDatabase("master");
      InitContent(db);

      Sitecore.Context.Site = Sitecore.Configuration.Factory.GetSite(Sitecore.Constants.ShellSiteName);
    }

    [SetUp]
    public void SetUp()
    { 
      var home = _context.CurrentDatabase.GetItem("/sitecore/content/home");

      var contentRootItem = TestUtil.CreateContentFromFile("TestResources\\items in workflow.xml", _testRoot);
      _publishableItem = contentRootItem.Axes.GetChild("publishable");
      _itemInWorkflow = contentRootItem.Axes.GetChild("in draft");
      _noWorkflow = contentRootItem.Axes.GetChild("no workflow");

      _noWorkflow.Editing.BeginEdit();
      _noWorkflow[FieldIDs.Workflow] = string.Empty;
      _noWorkflow[FieldIDs.WorkflowState] = string.Empty;
      _noWorkflow.Editing.EndEdit();

      // publish the root item only so publishing on any item works
      var dbs = new Database[]{ Sitecore.Configuration.Factory.GetDatabase("web")};
      var langs = new Language[]{ Sitecore.Context.Language};
      var handle = PublishManager.PublishItem(_testRoot, dbs, langs, false, true);

      PublishManager.PublishItem(contentRootItem, dbs, langs, false, true);

      var jobs = from j in Sitecore.Jobs.JobManager.GetJobs()
                 where j.Name.Contains("Publish") && j.Name.Contains("web")
                 select j;

      foreach (var job in jobs)
        job.Wait();
    }

    [TearDown]
    public void TearDown()
    {
      if (_testRoot != null)
      {
        // ensure any instances of the items in the web DB are also removed
        var webDb = Sitecore.Configuration.Factory.GetDatabase("web");
        var webrootitem = webDb.GetItem(_testRoot.ID);

        if(webrootitem != null)
          webrootitem.Delete();

        _testRoot.DeleteChildren();
      }

      Sitecore.Context.Item = Sitecore.Context.Database.GetItem("/sitecore/content/home");
    }

    [Test]
    public void CanPublish()
    {
      var cmd = new Revolver.Core.Commands.PublishItem();
      InitCommand(cmd);

      _context.CurrentItem = _publishableItem;

      var webDb = Sitecore.Configuration.Factory.GetDatabase("web");
      Assert.IsNull(webDb.GetItem(_context.CurrentItem.ID), "The item exists in the web DB already");

      var result = cmd.Run();
      WaitForPublish();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.IsNotNull(webDb.GetItem(_context.CurrentItem.ID));
    }

    // todo: selective publishing target

    // todo: selective language

    // todo: incremental publish

    // todo: smart publish

    [Test]
    public void StoppedByWorkflow()
    {
      var cmd = new Revolver.Core.Commands.PublishItem();
      InitCommand(cmd);

      _context.CurrentItem = _itemInWorkflow;

      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }

    [Test]
    public void NoWorkflow()
    {
      var cmd = new Revolver.Core.Commands.PublishItem();
      InitCommand(cmd);

      _context.CurrentItem = _noWorkflow;

      var webDb = Sitecore.Configuration.Factory.GetDatabase("web");
      Assert.IsNull(webDb.GetItem(_context.CurrentItem.ID), "The item exists in the web DB already");

      var result = cmd.Run();
      WaitForPublish();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.IsNotNull(webDb.GetItem(_context.CurrentItem.ID));
    }

    private void WaitForPublish()
    {
      // Make sure the publish job has had time to start
      System.Threading.Thread.Sleep(1000);

      // Wait for publish job to end
      var jobs = from j in Sitecore.Jobs.JobManager.GetJobs()
                 where j.Name.Contains("Publish") && j.Name.Contains("web")
                 select j;

      foreach (var job in jobs)
        job.Wait();

      // todo: fix this test. Shouldn't need to sleep
      System.Threading.Thread.Sleep(1500);
    }
  }
}