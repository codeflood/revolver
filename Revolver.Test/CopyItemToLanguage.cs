using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("CopyItemToLanguage")]
  public class CopyItemToLanguage : BaseCommandTest
  {
    TemplateItem _template = null;
    Item _germanLanguageDef = null;
    bool _revertLanguage = false;
    Language _defaultLanguage = null;
    Language _germanLanguage = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      _revertLanguage = !TestUtil.IsGermanRegistered(_context);

      if (_revertLanguage)
        _germanLanguageDef = TestUtil.RegisterGermanLanaguage(_context);

      _context.CurrentDatabase = Sitecore.Configuration.Factory.GetDatabase("web");
      _template = _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate];

      _defaultLanguage = _context.CurrentLanguage;
      _germanLanguage = Language.Parse("de");

      InitContent();
    }

    [SetUp]
    public void SetUp()
    {
      _context.CurrentLanguage = _defaultLanguage;
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      if (_revertLanguage)
        _germanLanguageDef.Delete();
    }

    [Test]
    public void MissingLanguage()
    {
      var cmd = new Cmd.CopyItemToLanguage();
      InitCommand(cmd);

      var result = cmd.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }

    [Test]
    public void InvalidLanguage()
    {
      var cmd = new Cmd.CopyItemToLanguage();
      InitCommand(cmd);

      cmd.LanguageName = "this is not a language";
      var result = cmd.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }

    [Test]
    public void ValidLanguageNotInDB()
    {
      var cmd = new Cmd.CopyItemToLanguage();
      InitCommand(cmd);

      cmd.LanguageName = "zh";
      var result = cmd.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }

    [Test]
    public void CopyToGermanWithoutOverride()
    {
      var englishTitle = "Tea and crumpets";
      var englishText = "A crumpet is flattened round bread that is cooked on a griddle or in a skillet.";
      var germanTitle = "Larger and sauerkraut";
      var germanText = "Sauerkraut is traditionally made in stoneware crocks and in fairly large batches.";

      var cmd = new Cmd.CopyItemToLanguage();
      InitCommand(cmd);

      var defaultItem = _testRoot.Add("CopyToGerman", _template);
      using (new EditContext(defaultItem))
      {
        defaultItem["title"] = englishTitle;
        defaultItem["text"] = englishText;
      }

      var germanItem = defaultItem.Database.GetItem(defaultItem.ID, _germanLanguage);
      using (new EditContext(germanItem))
      {
        germanItem["title"] = germanTitle;
        germanItem["text"] = germanText;
      }

      _context.CurrentItem = defaultItem;

      cmd.LanguageName = "de";
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      germanItem.Reload();

      Assert.AreEqual(germanTitle, germanItem["title"]);
      Assert.AreEqual(germanText, germanItem["text"]);
    }

    [Test]
    public void CopyToGermanWithOverride()
    {
      var englishTitle = "Tea and crumpets";
      var englishText = "A crumpet is flattened round bread that is cooked on a griddle or in a skillet.";
      var germanTitle = "Larger and sauerkraut";
      var germanText = "Sauerkraut is traditionally made in stoneware crocks and in fairly large batches.";

      var cmd = new Cmd.CopyItemToLanguage();
      InitCommand(cmd);

      var defaultItem = _testRoot.Add("CopyToGerman", _template);
      using (new EditContext(defaultItem))
      {
        defaultItem["title"] = englishTitle;
        defaultItem["text"] = englishText;
      }

      var germanItem = defaultItem.Database.GetItem(defaultItem.ID, _germanLanguage);
      using (new EditContext(germanItem))
      {
        germanItem["title"] = germanTitle;
        germanItem["text"] = germanText;
      }

      _context.CurrentItem = defaultItem;

      cmd.LanguageName = "de";
      cmd.Overwrite = true;
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      germanItem.Reload();

      Assert.AreEqual(englishTitle, germanItem["title"]);
      Assert.AreEqual(englishText, germanItem["text"]);
    }

    [Test]
    public void CopyToGermanEmptyTarget()
    {
      var englishTitle = "Tea and crumpets";
      var englishText = "A crumpet is flattened round bread that is cooked on a griddle or in a skillet.";

      var cmd = new Cmd.CopyItemToLanguage();
      InitCommand(cmd);

      var defaultItem = _testRoot.Add("CopyToGerman", _template);
      using (new EditContext(defaultItem))
      {
        defaultItem["title"] = englishTitle;
        defaultItem["text"] = englishText;
      }

      var germanItem = defaultItem.Database.GetItem(defaultItem.ID, _germanLanguage);

      _context.CurrentItem = defaultItem;

      cmd.LanguageName = "de";
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      germanItem.Reload();

      Assert.AreEqual(englishTitle, germanItem["title"]);
      Assert.AreEqual(englishText, germanItem["text"]);
    }

    [Test]
    public void CopyToGermanSingleField()
    {
      var englishTitle = "Tea and crumpets";
      var englishText = "A crumpet is flattened round bread that is cooked on a griddle or in a skillet.";
      var germanTitle = "Larger and sauerkraut";
      var germanText = "Sauerkraut is traditionally made in stoneware crocks and in fairly large batches.";

      var cmd = new Cmd.CopyItemToLanguage();
      InitCommand(cmd);

      var defaultItem = _testRoot.Add("CopyToGerman", _template);
      using (new EditContext(defaultItem))
      {
        defaultItem["title"] = englishTitle;
        defaultItem["text"] = englishText;
      }

      var germanItem = defaultItem.Database.GetItem(defaultItem.ID, _germanLanguage);
      using (new EditContext(germanItem))
      {
        germanItem["title"] = germanTitle;
        germanItem["text"] = germanText;
      }

      _context.CurrentItem = defaultItem;

      cmd.LanguageName = "de";
      cmd.Overwrite = true;
      cmd.FieldName = "title";
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      germanItem.Reload();

      Assert.AreEqual(englishTitle, germanItem["title"]);
      Assert.AreEqual(germanText, germanItem["text"]);
    }

    [Test]
    public void CopyToGermanSingleFieldWithoutOverride()
    {
      var englishTitle = "Tea and crumpets";
      var englishText = "A crumpet is flattened round bread that is cooked on a griddle or in a skillet.";
      var germanTitle = "Larger and sauerkraut";
      var germanText = "Sauerkraut is traditionally made in stoneware crocks and in fairly large batches.";

      var cmd = new Cmd.CopyItemToLanguage();
      InitCommand(cmd);

      var defaultItem = _testRoot.Add("CopyToGerman", _template);
      using (new EditContext(defaultItem))
      {
        defaultItem["title"] = englishTitle;
        defaultItem["text"] = englishText;
      }

      var germanItem = defaultItem.Database.GetItem(defaultItem.ID, _germanLanguage);
      using (new EditContext(germanItem))
      {
        germanItem["title"] = germanTitle;
        germanItem["text"] = germanText;
      }

      _context.CurrentItem = defaultItem;

      cmd.LanguageName = "de";
      cmd.FieldName = "title";
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Failure, result.Status);
      germanItem.Reload();

      Assert.AreEqual(germanTitle, germanItem["title"]);
      Assert.AreEqual(germanText, germanItem["text"]);
    }

    [Test]
    public void CopyToGermanSingleFieldInvalidFieldName()
    {
      var englishTitle = "Tea and crumpets";
      var germanTitle = "Larger and sauerkraut";

      var cmd = new Cmd.CopyItemToLanguage();
      InitCommand(cmd);

      var defaultItem = _testRoot.Add("CopyToGerman", _template);
      using (new EditContext(defaultItem))
      {
        defaultItem["title"] = englishTitle;
      }

      var germanItem = defaultItem.Database.GetItem(defaultItem.ID, _germanLanguage);
      using (new EditContext(germanItem))
      {
        germanItem["title"] = germanTitle;
      }

      _context.CurrentItem = defaultItem;

      cmd.LanguageName = "de";
      cmd.FieldName = "some field that doesnt exist";
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Failure, result.Status);
      germanItem.Reload();

      Assert.AreEqual(germanTitle, germanItem["title"]);
    }

    [Test]
    public void NonContextCopyToGerman()
    {
      var englishTitle = "Tea and crumpets";
      var germanTitle = "Larger and sauerkraut";

      var cmd = new Cmd.CopyItemToLanguage();
      InitCommand(cmd);

      _context.CurrentItem = _context.CurrentDatabase.GetRootItem();

      var defaultItem = _testRoot.Add("CopyToGerman", _template);
      using (new EditContext(defaultItem))
      {
        defaultItem["title"] = englishTitle;
      }

      var germanItem = defaultItem.Database.GetItem(defaultItem.ID, _germanLanguage);
      using (new EditContext(germanItem))
      {
        germanItem["title"] = germanTitle;
      }

      cmd.LanguageName = "de";
      cmd.Overwrite = true;
      cmd.Path = defaultItem.ID.ToString();
      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      germanItem.Reload();

      Assert.AreEqual(englishTitle, germanItem["title"]);
    }
  }
}
