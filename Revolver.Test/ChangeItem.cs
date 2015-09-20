using NUnit.Framework;
using Revolver.Core;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ChangeItem")]
  public class ChangeItem : BaseCommandTest
  {
    Item _testTreeRoot = null;
    Item _germanLanguageDef = null;
    bool _revertLanguage = false;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      _revertLanguage = !TestUtil.IsGermanRegistered(_context);

      if (_revertLanguage)
        _germanLanguageDef = TestUtil.RegisterGermanLanaguage(_context);

      using(new SecurityDisabler())
      {
        InitContent();
        _testTreeRoot = TestUtil.CreateContentFromFile("TestResources\\narrow tree with duplicate names.xml", _testRoot);
      }
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      if (_testRoot != null)
      {
        using (new SecurityDisabler())
        {
          _testRoot.Delete();
        }
      }

      if (_revertLanguage)
      {
        using (new SecurityDisabler())
        {
          _germanLanguageDef.Delete();
        }
      }
    }

    protected void ProcessTest(Item context, string path, CommandStatus expectedStatus, ID expectedContext, Language language = null, Sitecore.Data.Version version = null)
    {
      var cmd = new Cmd.ChangeItem();
      InitCommand(cmd);

      cmd.PathOrID = path;

      _context.CurrentItem = context;
      var prevPath = "/" + context.Database.Name + context.Paths.FullPath;

      var result = cmd.Run();
      Assert.AreEqual(expectedStatus, result.Status);
      Assert.AreEqual(expectedContext, _context.CurrentItem.ID);

      if (expectedStatus == CommandStatus.Success)
        Assert.AreEqual(prevPath, _context.EnvironmentVariables["prevpath"]);

      if (language != null)
        Assert.AreEqual(language, _context.CurrentItem.Language);

      if (version != null)
        Assert.AreEqual(version, _context.CurrentItem.Version);
    }

    [Test]
    public void NoPath()
    {
      ProcessTest(_testTreeRoot, string.Empty, CommandStatus.Failure, _testTreeRoot.ID);
    }

    [Test]
    public void RelativeValidChild()
    {
      ProcessTest(_testTreeRoot, "Sycorax", CommandStatus.Success, _testTreeRoot.Axes.GetChild("Sycorax").ID);
    }

    [Test]
    public void RelativeInvalidChild()
    {
      ProcessTest(_testTreeRoot, "i hope this name doesnt exist", CommandStatus.Failure, _testTreeRoot.ID);
    }

    [Test]
    public void RelativeValidGrandChild()
    {
      ProcessTest(_testTreeRoot, "Umbriel/Ymir", CommandStatus.Success, _testTreeRoot.Axes.SelectSingleItem("Umbriel/Ymir").ID);
    }

    [Test]
    public void RelativeInvalidGrandChild()
    {
      ProcessTest(_testTreeRoot, "Umbriel/i hope this name doesnt exist", CommandStatus.Failure, _testTreeRoot.ID);
    }

    [Test]
    public void RelativeValidAncestor()
    {
      var item = _testTreeRoot.Axes.SelectSingleItem("Umbriel/Ymir");
      ProcessTest(item, "../..", CommandStatus.Success, _testTreeRoot.ID);
    }

    [Test]
    public void RelativeValid()
    {
      var item = _testTreeRoot.Axes.SelectSingleItem("Umbriel/Ymir");
      ProcessTest(item, "../../Juliet", CommandStatus.Success, _testTreeRoot.Axes.SelectSingleItem("Juliet").ID);
    }

    [Test]
    public void RelativeInvalid()
    {
      var item = _testTreeRoot.Axes.SelectSingleItem("Umbriel/Ymir");
      ProcessTest(item, "../../boo", CommandStatus.Failure, item.ID);
    }

    [Test]
    public void ValidId()
    {
      var item = _testTreeRoot.Axes.SelectSingleItem("Umbriel/Ymir");
      ProcessTest(_context.CurrentDatabase.GetRootItem(), item.ID.ToString(), CommandStatus.Success, item.ID);
    }

    [Test]
    public void InvalidId()
    {
      var item = _context.CurrentDatabase.GetRootItem();
      ProcessTest(item, ID.NewID.ToString(), CommandStatus.Failure, item.ID);
    }

    [Test]
    public void MalformedId()
    {
      ProcessTest(_testTreeRoot, "{bbb-eee-444-aaa}", CommandStatus.Failure, _testTreeRoot.ID);
    }

    [Test]
    public void SameNameValidIndex()
    {
      var item = _testTreeRoot.Axes.SelectSingleItem("Umbriel");
      var expectedItem = _testTreeRoot.Axes.SelectSingleItem("Umbriel/*[@title='Skoll 1']");
      ProcessTest(item, "skoll[1]", CommandStatus.Success, expectedItem.ID);
    }

    [Test]
    public void SameNameInvalidIndex()
    {
      var item = _testTreeRoot.Axes.SelectSingleItem("Umbriel");
      ProcessTest(item, "skoll[5]", CommandStatus.Failure, item.ID);
    }

    [Test]
    public void NotSameNameWithValidIndex()
    {
      var item = _testTreeRoot.Axes.SelectSingleItem("Umbriel");
      var expectedItem = _testTreeRoot.Axes.SelectSingleItem("Umbriel/Ymir");
      ProcessTest(item, "Ymir[0]", CommandStatus.Success, expectedItem.ID);
    }

    [Test]
    public void NotSameNameWithInvalidIndex()
    {
      var item = _testTreeRoot.Axes.SelectSingleItem("Umbriel");
      ProcessTest(item, "Ymir[1]", CommandStatus.Failure, item.ID);
    }

    [Test]
    public void RelativeByValidChildIndex()
    {
      var expectedItem = _testTreeRoot.Axes.SelectSingleItem("Sycorax");
      ProcessTest(_testTreeRoot, "[1]", CommandStatus.Success, expectedItem.ID);
    }

    [Test]
    public void RelativeByInvalidChildIndex()
    {
      var expectedItem = _testTreeRoot.Axes.SelectSingleItem("Sycorax");
      ProcessTest(_testTreeRoot, "[7]", CommandStatus.Failure, _testTreeRoot.ID);
    }

    [Test]
    public void AbsoluteValid()
    {
      var expectedItem = _testTreeRoot.Axes.SelectSingleItem("Sycorax");
      ProcessTest(_testTreeRoot, expectedItem.Paths.FullPath, CommandStatus.Success, expectedItem.ID);
    }

    [Test]
    public void AbsoluteInvalid()
    {
      ProcessTest(_testTreeRoot, "/sitecore/content/nothing/doesnt/exist", CommandStatus.Failure, _testTreeRoot.ID);
    }

    [Test]
    public void RelativeShorthandChildExists()
    {
      ProcessTest(_testTreeRoot, "s", CommandStatus.Success, _testTreeRoot.Axes.GetChild("Sycorax").ID);
    }

    [Test]
    public void RelativeShorthandChildDoesntExist()
    {
      ProcessTest(_testTreeRoot, "z", CommandStatus.Failure, _testTreeRoot.ID);
    }

    [Test]
    public void ToRoot()
    {
      ProcessTest(_testTreeRoot, "/", CommandStatus.Success, _context.CurrentDatabase.GetRootItem().ID);
    }

    [Test]
    public void ToCoreDB()
    {
      using (new SecurityDisabler())
      {
        var db = Factory.GetDatabase("core");
        var coreContent = db.GetItem("/sitecore/content");

        ProcessTest(_testTreeRoot, "/core/sitecore/content", CommandStatus.Success, coreContent.ID);
      }
    }

    [Test]
    public void ToWebDB()
    {
      var db = Factory.GetDatabase("web");
      var webMediaLibrary = db.GetItem("/sitecore/media library");

      ProcessTest(_testTreeRoot, "/web/sitecore/media library", CommandStatus.Success, webMediaLibrary.ID);
    }

    [Test]
    public void ToMasterDB()
    {
      var db = Factory.GetDatabase("master");
      var masterSystem = db.GetItem("/sitecore/system");

      ProcessTest(_testTreeRoot, "/master/sitecore/system", CommandStatus.Success, masterSystem.ID);
    }

    [Test]
    public void ChangeContextLanguage()
    {
      var language = Language.Parse("de");
      var germanVersion = _context.CurrentDatabase.GetItem(_testTreeRoot.ID, language);

      ProcessTest(_testTreeRoot, ":de", CommandStatus.Success, germanVersion.ID, language);
    }

    [Test]
    public void ChangeLanguageRelative()
    {
      var language = Language.Parse("de");
      var currentLanguageItem = _testTreeRoot.Axes.SelectSingleItem("Umbriel/Ymir");
      var germanVersion = _context.CurrentDatabase.GetItem(currentLanguageItem.ID, language);

      ProcessTest(_testTreeRoot, "umbriel/ymir:de", CommandStatus.Success, germanVersion.ID, language);
    }

    [Test]
    public void ChangeLanguageAbsolute()
    {
      var language = Language.Parse("de");
      var currentLanguageItem = _testTreeRoot.Axes.SelectSingleItem("Umbriel/Ymir");
      var germanVersion = _context.CurrentDatabase.GetItem(currentLanguageItem.ID, language);

      ProcessTest(_testTreeRoot, currentLanguageItem.Paths.FullPath + ":de", CommandStatus.Success, germanVersion.ID, language);
    }

    [Test]
    public void ChangeLanguageInvalid()
    {
      ProcessTest(_testTreeRoot, ":foo", CommandStatus.Failure, _testTreeRoot.ID, _testTreeRoot.Language, _testTreeRoot.Version);
    }

    [Test]
    public void ChangeContextVersion()
    {
      var item = _testTreeRoot.Axes.SelectSingleItem("Juliet");
      ProcessTest(item, "::1", CommandStatus.Success, item.ID, null, Sitecore.Data.Version.First);
    }

    [Test]
    public void ChangeContextVersionFromLatest()
    {
      var item = _testTreeRoot.Axes.SelectSingleItem("Juliet");
      ProcessTest(item, "::-1", CommandStatus.Success, item.ID, null, Sitecore.Data.Version.Parse(2));
    }

    [Test]
    public void ChangeVersionRelative()
    {
      var item = _testTreeRoot.Axes.SelectSingleItem("Juliet");
      ProcessTest(_testTreeRoot, "Juliet::2", CommandStatus.Success, item.ID, null, Sitecore.Data.Version.Parse(2));
    }

    [Test]
    public void ChangeVersionAbsolute()
    {
      var item = _testTreeRoot.Axes.SelectSingleItem("Juliet");
      ProcessTest(_testTreeRoot, item.Paths.FullPath + "::2", CommandStatus.Success, item.ID, null, Sitecore.Data.Version.Parse(2));
    }

    [Test]
    public void ChangeVersionInvalidFromEnd()
    {
      ProcessTest(_testTreeRoot, "::-300", CommandStatus.Failure, _testTreeRoot.ID, _testTreeRoot.Language, _testTreeRoot.Version);
    }

    [Test]
    public void ChangeVersionAndLanguageRelative()
    {
      var item = _testTreeRoot.Axes.SelectSingleItem("Juliet");
      ProcessTest(_testTreeRoot, "Juliet:de:1", CommandStatus.Success, item.ID, Language.Parse("de"), Sitecore.Data.Version.Parse(1));
    }
  }
}

