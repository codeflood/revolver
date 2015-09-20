using NUnit.Framework;
using Revolver.Core;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using System.IO;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("CopyItem")]
  public class CopyItem : BaseCommandTest
  {
    TemplateItem _template = null;
    Item _singleTestRoot = null;
    Item _masterTestRoot = null;
    Item _mediaTestRoot = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      _context.CurrentDatabase = Sitecore.Configuration.Factory.GetDatabase("web");
      _template = _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate];

      var masterDB = Sitecore.Configuration.Factory.GetDatabase("master");
      var masterContent = masterDB.GetItem(ItemIDs.ContentRoot);

      InitContent();  
      _masterTestRoot = masterContent.Add("test root-" + DateUtil.IsoNow, _template);

      var mediaFolderTemplate = masterDB.Templates["system/media/media folder"];
      _mediaTestRoot = masterDB.GetItem("/sitecore/media library/images").Add("test-" + DateUtil.IsoNow, mediaFolderTemplate);
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      if (_masterTestRoot != null)
        _masterTestRoot.Delete();

      if (_mediaTestRoot != null)
        _mediaTestRoot.Delete();
    }

    [TearDown]
    public void TearDown()
    {
      if (_singleTestRoot != null)
        _singleTestRoot.Delete();

      _masterTestRoot.DeleteChildren();
    }

    [SetUp]
    public void SetUp()
    {
      _context.CurrentDatabase = Sitecore.Configuration.Factory.GetDatabase("web");
      _singleTestRoot = _testRoot.Add("single test root" + DateUtil.IsoNow, _template);
    }

    [Test]
    public void NoArguments()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var res = cmd.Run();

      Assert.AreEqual(CommandStatus.Failure, res.Status);
    }

    [Test]
    public void ContextToExistingPath()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var sourceItem = _singleTestRoot.Add("source", _template);
      var targetItem = _singleTestRoot.Add("target", _template);

      _context.CurrentItem = sourceItem;

      cmd.TargetPath = targetItem.Paths.FullPath;

      var res = cmd.Run();
      targetItem.Reload();

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual(1, targetItem.GetChildren().Count);
      Assert.AreEqual(sourceItem.Name, targetItem.Children[0].Name);
    }

    [Test]
    public void ContextToSelf()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var sourceItem = _singleTestRoot.Add("source", _template);
      _context.CurrentItem = sourceItem;

      cmd.TargetPath = ".";

      var res = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual(1, sourceItem.GetChildren().Count);
      Assert.AreEqual(sourceItem.Name, sourceItem.Children[0].Name);
    }

    [Test]
    public void ContextToNewPath()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var sourceItem = _singleTestRoot.Add("source", _template);
      var targetItem = _singleTestRoot.Add("target", _template);

      _context.CurrentItem = sourceItem;

      cmd.TargetPath = targetItem.Paths.FullPath + "/newpath";

      var res = cmd.Run();
      targetItem.Reload();

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual(1, targetItem.GetChildren().Count);
      Assert.AreEqual("newpath", targetItem.Children[0].Name);
    }

    [Test]
    public void ContextToNewPathRecursive()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var sourceItem = _singleTestRoot.Add("source", _template);
      var sourceChildItem = sourceItem.Add("child1", _template);
      var targetItem = _singleTestRoot.Add("target", _template);

      _context.CurrentItem = sourceItem;

      cmd.TargetPath = targetItem.Paths.FullPath + "/newpath";
      cmd.Recursive = true;

      var res = cmd.Run();
      targetItem.Reload();

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual(1, targetItem.GetChildren().Count);
      Assert.AreEqual("newpath", targetItem.Children[0].Name);
      Assert.AreEqual(1, targetItem.Children[0].GetChildren().Count);
      Assert.AreEqual(sourceChildItem.Name, targetItem.Children[0].Children[0].Name);
    }

    [Test]
    public void ContextToNewPathFromTreeNonRecursive()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var sourceItem = _singleTestRoot.Add("source", _template);
      var sourceChildItem = sourceItem.Add("child1", _template);
      var targetItem = _singleTestRoot.Add("target", _template);

      _context.CurrentItem = sourceItem;

      cmd.TargetPath = targetItem.Paths.FullPath + "/newpath";

      var res = cmd.Run();
      targetItem.Reload();

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual(1, targetItem.GetChildren().Count);
      Assert.AreEqual("newpath", targetItem.Children[0].Name);
      Assert.AreEqual(0, targetItem.Children[0].GetChildren().Count);
    }

    [Test]
    public void ContextToDBSameID()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var sourceItem = _singleTestRoot.Add("source", _template);
      _context.CurrentItem = sourceItem;

      cmd.TargetPath = "/master/sitecore/content/" + _masterTestRoot.Name;

      var res = cmd.Run();

      _masterTestRoot.Reload();
      var masterCopy = _masterTestRoot.Axes.GetChild(sourceItem.ID);

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.IsNotNull(masterCopy);
      Assert.AreEqual(sourceItem.ID, masterCopy.ID);
    }

    [Test]
    public void ContextToDBDifferentID()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var sourceItem = _singleTestRoot.Add("source", _template);
      _context.CurrentItem = sourceItem;

      cmd.TargetPath = "/master/sitecore/content/" + _masterTestRoot.Name;
      cmd.NewId = true;

      var res = cmd.Run();

      _masterTestRoot.Reload();
      var masterCopy = _masterTestRoot.Axes.GetChild(sourceItem.Name);

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.IsNotNull(masterCopy);
      Assert.AreNotEqual(sourceItem.ID, masterCopy.ID);
    }

    [Test]
    public void DifferentItemToExistingPathRelative()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var sourceItem = _singleTestRoot.Add("source", _template);
      var targetItem = _singleTestRoot.Add("target", _template);

      _context.CurrentItem = _singleTestRoot.Parent;

      cmd.SourcePath = _singleTestRoot.Name + "/" + sourceItem.Name;
      cmd.TargetPath = _singleTestRoot.Name + "/" + targetItem.Name;

      var res = cmd.Run();
      targetItem.Reload();

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual(1, targetItem.GetChildren().Count);
      Assert.AreEqual(sourceItem.Name, targetItem.Children[0].Name);
      Assert.AreEqual(0, targetItem.Children[0].GetChildren().Count);
    }

    [Test]
    public void DifferentItemToExistingPathAbsolute()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var sourceItem = _singleTestRoot.Add("source", _template);
      var targetItem = _singleTestRoot.Add("target", _template);

      _context.CurrentItem = _singleTestRoot.Parent;

      cmd.SourcePath = _singleTestRoot.Name + "/" + sourceItem.Name;
      cmd.TargetPath = targetItem.Paths.FullPath;

      var res = cmd.Run();
      targetItem.Reload();

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual(1, targetItem.GetChildren().Count);
      Assert.AreEqual(sourceItem.Name, targetItem.Children[0].Name);
      Assert.AreEqual(0, targetItem.Children[0].GetChildren().Count);
    }

    [Test]
    public void InvalidSource()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var targetItem = _singleTestRoot.Add("target", _template);

      _context.CurrentItem = _singleTestRoot.Parent;

      cmd.SourcePath = "/sitecore/content/thia/path/doesnt/exist";
      cmd.TargetPath = targetItem.Paths.FullPath;

      var res = cmd.Run();

      Assert.AreEqual(CommandStatus.Failure, res.Status);
    }

    [Test]
    public void InvalidTarget()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var sourceItem = _singleTestRoot.Add("source", _template);

      _context.CurrentItem = _singleTestRoot.Parent;

      cmd.SourcePath = _singleTestRoot.Name + "/" + sourceItem.Name;
      cmd.TargetPath = "/sitecore/content/thia/path/doesnt/exist";

      var res = cmd.Run();

      Assert.AreEqual(CommandStatus.Failure, res.Status);
    }
    
    [Test]
    public void DisallowCopyToLanguage()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);
      cmd.TargetPath = ":de";
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void DisallowCopyToVersion()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);
      cmd.TargetPath = "::2";
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void CopyMedia()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var image = AddTestImageToMaster();
      var targetItem = image.InnerItem.Add("target", _template);

      _context.CurrentItem = image;

      cmd.TargetPath = targetItem.Paths.FullPath + "/newpath";

      var res = cmd.Run();
      targetItem.Reload();

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual(1, targetItem.GetChildren().Count);
      Assert.AreEqual("newpath", targetItem.Children[0].Name);

      Stream stream = ((MediaItem)targetItem.Children[0]).GetMediaStream();
      Assert.IsNotNull(stream);
      Assert.Greater(stream.Length, 0);
    }

    [Test]
    public void CopyMediaRecursive()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var mediaFolder = CreateImageFoldersInMaster();
      var targetItem = _mediaTestRoot.Add("target-" + DateUtil.IsoNow, _template);

      _context.CurrentItem = mediaFolder;

      cmd.TargetPath = targetItem.Paths.FullPath + "/newpath";
      cmd.Recursive = true;

      var res = cmd.Run();
      targetItem.Reload();

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual(1, targetItem.GetChildren().Count);
      Assert.AreEqual("newpath", targetItem.Children[0].Name);

      Assert.AreEqual(2, targetItem.Children[0].GetChildren().Count);

      var stream = ((MediaItem)targetItem.Children[0].Children[0]).GetMediaStream();
      Assert.IsNotNull(stream);
      Assert.Greater(stream.Length, 0);

      var stream2 = ((MediaItem)targetItem.Children[0].Children[1]).GetMediaStream();
      Assert.IsNotNull(stream2);
      Assert.Greater(stream2.Length, 0);
    }

    [Test]
    public void CopyMediaToDifferentDB()
    {
      var cmd = new Cmd.CopyItem();
      InitCommand(cmd);

      var targetPath = "/sitecore/media library/images/test-image";

      var image = AddTestImageToMaster();
      _context.CurrentItem = image;

      cmd.TargetPath = "/web" + targetPath;

      var res = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, res.Status);

      var webImage = Factory.GetDatabase("web").GetItem(targetPath);
      Assert.IsNotNull(webImage);

      Stream stream = ((MediaItem)webImage).GetMediaStream();
      Assert.IsNotNull(stream);
      Assert.Greater(stream.Length, 0);
    }

    private MediaItem AddTestImageToMaster()
    {
      return AddTestImageToMaster(_mediaTestRoot.Paths.FullPath + "/sometest-" + DateUtil.IsoNow);
    }

    private MediaItem AddTestImageToMaster(string parent)
    {
      var masterDatabase = Sitecore.Configuration.Factory.GetDatabase("master");
      var options = new MediaCreatorOptions
      {
        Database = masterDatabase,
        Destination = parent
      };

      return MediaManager.Creator.CreateFromFile("~/TestResources/visual_list.png", options);
    }

    private Item CreateImageFoldersInMaster()
    {
      var masterDatabase = Sitecore.Configuration.Factory.GetDatabase("master");
      var folderTemplate = masterDatabase.Templates["system/media/media folder"];

      var folder = _mediaTestRoot.Add("test folder-" + DateUtil.IsoNow, folderTemplate);

      var image1 = AddTestImageToMaster(folder.Paths.FullPath + "/image1");
      var image2 = AddTestImageToMaster(folder.Paths.FullPath + "/image2");

      return folder;
    }
  }
}
