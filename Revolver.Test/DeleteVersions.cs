
using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using System.Linq;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("DeleteVersions")]
  public class DeleteVersions : BaseCommandTest
  {
    Item _testItem = null;
    Item _germanLanguageDef = null;
    bool _revertLanguage = false;

    [TestFixtureSetUp]
    public void Init()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      _revertLanguage = !TestUtil.IsGermanRegistered(_context);

      if (_revertLanguage)
        _germanLanguageDef = TestUtil.RegisterGermanLanaguage(_context);

      InitContent();
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      base.CleanUp();

      if (_revertLanguage)
      {
        _germanLanguageDef.Delete();
      }
    }

    [SetUp]
    public void SetUp()
    {
      var template = _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate];
      _testItem = _testRoot.Add("versions item", template);

      _testItem = _testItem.Versions.AddVersion();
      _testItem = _testItem.Versions.AddVersion();
      _testItem = _testItem.Versions.AddVersion();

      // create the German version of the item
      var languageItem = _testItem.Database.GetItem(_testItem.ID, Language.Parse("de"));
      languageItem = languageItem.Versions.AddVersion();
      languageItem = languageItem.Versions.AddVersion();
      languageItem = languageItem.Versions.AddVersion();
      languageItem = languageItem.Versions.AddVersion();
      languageItem = languageItem.Versions.AddVersion();
    }

    [TearDown]
    public void TearDown()
    {
      _testRoot.DeleteChildren();
    }

    [Test]
    public void ContextVersion()
    {
      var cmd = new Cmd.DeleteVersions();
      InitCommand(cmd);

      _context.CurrentItem = _testItem.Database.GetItem(_testItem.ID, Language.Parse("en"), new Version(3));

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      AssertVersionNumbers(new[] { 1, 2, 4 }, new[] { 1, 2, 3, 4, 5 }, 4);
    }

    [Test]
    public void ContextVersionLatest()
    {
      var cmd = new Cmd.DeleteVersions();
      InitCommand(cmd);

      _context.CurrentItem = _testItem;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      AssertVersionNumbers(new[] { 1, 2, 3 }, new[] { 1, 2, 3, 4, 5 }, 3);
    }

    [Test]
    public void DifferentPath()
    {
      var cmd = new Cmd.DeleteVersions();
      InitCommand(cmd);

      _context.CurrentItem = _testItem.Parent.Parent;

      cmd.Path = _testItem.Parent.Name + "/" + _testItem.Name;
      var result = cmd.Run();
      
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      AssertVersionNumbers(new[] { 1, 2, 3 }, new[] { 1, 2, 3, 4, 5 }, 1);
    }

    [Test]
    public void VersionInPath()
    {
      var cmd = new Cmd.DeleteVersions();
      InitCommand(cmd);

      _context.CurrentItem = _testItem;

      cmd.Path = "::2";
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      AssertVersionNumbers(new[] { 1, 3, 4 }, new[] { 1, 2, 3, 4, 5 }, 4);
    }

    [Test]
    public void OtherVersions()
    {
      var cmd = new Cmd.DeleteVersions();
      InitCommand(cmd);

      _context.CurrentItem = _testItem;

      cmd.OtherVersions = true;
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      AssertVersionNumbers(new[] { 4 }, new[] { 1, 2, 3, 4, 5 }, 4);
    }

    [Test]
    public void AllLanguages()
    {
      var cmd = new Cmd.DeleteVersions();
      InitCommand(cmd);

      _context.CurrentItem = _testItem;

      cmd.Path = "::1";
      cmd.AllLanguages = true;
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      AssertVersionNumbers(new[] { 2, 3, 4 }, new[] { 2, 3, 4, 5 }, 4);
    }

    [Test]
    public void AllLanguagesAndOtherVerions()
    {
      var cmd = new Cmd.DeleteVersions();
      InitCommand(cmd);

      _context.CurrentItem = _testItem;

      cmd.Path = "::3";
      cmd.OtherVersions = true;
      cmd.AllLanguages = true;
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      AssertVersionNumbers(new[] { 3 }, new[] { 3 }, 3);
    }

    private void AssertVersionNumbers(int[] englishVersions, int[] germanVersions, int contextVersion)
    {
      _testItem.Reload();
      Assert.That(_testItem.Versions.GetVersionNumbers().Select(x => x.Number).ToArray(), Is.EqualTo(englishVersions));

      var germanVersion = _testItem.Database.GetItem(_testItem.ID, Language.Parse("de"));
      Assert.That(germanVersion.Versions.GetVersionNumbers().Select(x => x.Number).ToArray(), Is.EqualTo(germanVersions));

      Assert.That(_context.CurrentItem.Version.Number, Is.EqualTo(contextVersion));
    }
  }
}
