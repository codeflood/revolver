using NUnit.Framework;
using Revolver.Core;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System.Text.RegularExpressions;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("List")]
  public class List : BaseCommandTest
  {
    Item _listRoot = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      InitContent();

      _listRoot = TestUtil.CreateContentFromFile("TestResources\\narrow tree.xml", _testRoot);

      // Ensure correct sort order
      using (new SecurityDisabler())
      {
        var item = _listRoot.Axes.GetChild("Luna");
        item.Editing.BeginEdit();
        item.Appearance.Sortorder = 100;
        item.Editing.EndEdit();

        item = _listRoot.Axes.GetChild("Deimos");
        item.Editing.BeginEdit();
        item.Appearance.Sortorder = 200;
        item.Editing.EndEdit();

        item = _listRoot.Axes.GetChild("phobos");
        item.Editing.BeginEdit();
        item.Appearance.Sortorder = 300;
        item.Editing.EndEdit();

        item = _listRoot.Axes.GetChild("Adrastea Phobos");
        item.Editing.BeginEdit();
        item.Appearance.Sortorder = 400;
        item.Editing.EndEdit();
      }
    }

    [Test]
    public void ListRootNode()
    {
      var cmd = new Cmd.List();
      InitCommand(cmd);

      _context.CurrentItem = _listRoot;
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);

      Assert.IsTrue(result.Message.Contains("+ Luna"));
      Assert.IsTrue(result.Message.Contains("Deimos"));
      Assert.IsTrue(result.Message.Contains("phobos"));
      Assert.IsTrue(result.Message.Contains("Adrastea Phobos"));
    }

    [Test]
    public void ContainsChildren()
    {
      var cmd = new Cmd.List();
      InitCommand(cmd);

      _context.CurrentItem = _listRoot.Axes.GetChild("Luna");

      var result = cmd.Run();

      Assert.IsTrue(result.Message.Contains("Carme"));
      Assert.IsTrue(result.Message.Contains("Ganymede"));
      Assert.IsTrue(result.Message.Contains("Metis"));
    }

    [Test]
    public void NoChildren()
    {
      var cmd = new Cmd.List();
      InitCommand(cmd);

      _context.CurrentItem = _listRoot.Axes.GetChild("Deimos");

      var result = cmd.Run();

      Assert.IsTrue(result.Message.Contains("zero items found"));
    }

    [Test]
    public void ByRegex()
    {
      var cmd = new Cmd.List();
      InitCommand(cmd);

      cmd.Regex = "phobos";

      _context.CurrentItem = _listRoot;
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);

      Assert.IsTrue(result.Message.Contains("phobos"));
      Assert.IsTrue(result.Message.Contains("Adrastea Phobos"));
    }

    [Test]
    public void ByCaseSensitiveRegex()
    {
      var cmd = new Cmd.List();
      InitCommand(cmd);

      cmd.Regex = "phobos";
      cmd.CaseSensitiveRegex = true;

      _context.CurrentItem = _listRoot;
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);

      Assert.IsTrue(result.Message.Contains("phobos"));
    }

    [Test]
    public void Alphabetical()
    {
      var cmd = new Cmd.List();
      InitCommand(cmd);

      cmd.Alphabetical = true;

      _context.CurrentItem = _listRoot;
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.IsTrue(Regex.IsMatch(result.Message, @"\s+Adrastea Phobos\s+Deimos\s+\+\sLuna\s+phobos"));
      
    }

    [Test]
    public void ReverseAlphabetical()
    {
      var cmd = new Cmd.List();
      InitCommand(cmd);

      cmd.Alphabetical = true;
      cmd.ReverseOrder = true;

      _context.CurrentItem = _listRoot;
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);

      Assert.IsTrue(Regex.IsMatch(result.Message, @"\s+phobos\s+\+\sLuna\s+Deimos\s+Adrastea Phobos"));
    }

    [Test]
    public void RelativePathDown()
    {
      var cmd = new Cmd.List();
      InitCommand(cmd);

      cmd.Path = "Luna";

      _context.CurrentItem = _listRoot;
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);

      Assert.IsTrue(result.Message.Contains("Carme"));
      Assert.IsTrue(result.Message.Contains("Ganymede"));
      Assert.IsTrue(result.Message.Contains("Metis"));
    }

    [Test]
    public void RelativePathUp()
    {
      var cmd = new Cmd.List();
      InitCommand(cmd);

      cmd.Path = "..";

      _context.CurrentItem = _listRoot.Axes.GetChild("Luna");
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);

      Assert.IsTrue(result.Message.Contains("+ Luna"));
      Assert.IsTrue(result.Message.Contains("Deimos"));
      Assert.IsTrue(result.Message.Contains("phobos"));
      Assert.IsTrue(result.Message.Contains("Adrastea Phobos"));
    }

    [Test]
    public void AbsolutePath()
    {
      var cmd = new Cmd.List();
      InitCommand(cmd);

      cmd.Path = _listRoot.Paths.FullPath;

      _context.CurrentItem = _context.CurrentDatabase.GetRootItem();
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);

      Assert.IsTrue(result.Message.Contains("+ Luna"));
      Assert.IsTrue(result.Message.Contains("Deimos"));
      Assert.IsTrue(result.Message.Contains("phobos"));
      Assert.IsTrue(result.Message.Contains("Adrastea Phobos"));
    }
  }
}
