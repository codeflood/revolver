using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data.Items;

namespace Revolver.Test
{
  [TestFixture]
  [Category("Context")]
  public class Context : BaseCommandTest
  {
    Item _testTreeRoot = null;
    Item _germanLanguageDef = null;
    bool _revertLanguage = false;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      _revertLanguage = !TestUtil.IsGermanRegistered(_context);

      if (_revertLanguage)
        _germanLanguageDef = TestUtil.RegisterGermanLanaguage(_context);

      InitContent();
      _testTreeRoot = TestUtil.CreateContentFromFile("TestResources\\narrow tree with duplicate names.xml", _testRoot);
    }

    protected override void CleanUp()
    {
      base.CleanUp();

      if (_revertLanguage)
        _germanLanguageDef.Delete();
    }

    [Test]
    public void SetContext_Relative()
    {
      var context = new Revolver.Core.Context();
      context.CurrentItem = _testTreeRoot;

      var result = context.SetContext("Sycorax");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(context.CurrentDatabase.Name, Is.EqualTo("web"));
      Assert.That(context.CurrentItem.ID, Is.EqualTo(_testTreeRoot.Axes.GetChild("Sycorax").ID));
      Assert.That(context.CurrentLanguage.Name, Is.EqualTo("en"));
    }

    [Test]
    public void SetContext_Absolute()
    {
      var context = new Revolver.Core.Context();
      context.CurrentItem = _testTreeRoot;

      var item = _testTreeRoot.Axes.GetChild("Sycorax");

      var result = context.SetContext(item.Paths.FullPath);

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(context.CurrentDatabase.Name, Is.EqualTo("web"));
      Assert.That(context.CurrentItem.ID, Is.EqualTo(_testTreeRoot.Axes.GetChild("Sycorax").ID));
      Assert.That(context.CurrentLanguage.Name, Is.EqualTo("en"));
    }

    [Test]
    public void SetContext_ID()
    {
      var context = new Revolver.Core.Context();
      context.CurrentItem = _testTreeRoot;

      var item = _testTreeRoot.Axes.GetChild("Sycorax");

      var result = context.SetContext(item.ID.ToString());

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(context.CurrentDatabase.Name, Is.EqualTo("web"));
      Assert.That(context.CurrentItem.ID, Is.EqualTo(_testTreeRoot.Axes.GetChild("Sycorax").ID));
      Assert.That(context.CurrentLanguage.Name, Is.EqualTo("en"));
    }

    [Test]
    public void SetContext_NumberedChild()
    {
      var startItem = _testTreeRoot.Axes.SelectSingleItem("Umbriel");

      var context = new Revolver.Core.Context();
      context.CurrentItem = startItem;

      var expectedItem = _testTreeRoot.Axes.SelectSingleItem("Umbriel/*[@title='Skoll 1']");

      var result = context.SetContext("skoll[1]");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(context.CurrentDatabase.Name, Is.EqualTo("web"));
      Assert.That(context.CurrentItem.ID, Is.EqualTo(expectedItem.ID));
      Assert.That(context.CurrentLanguage.Name, Is.EqualTo("en"));
    }

    [Test]
    public void SetContext_NumberedChildNegative()
    {
      var startItem = _testTreeRoot.Axes.SelectSingleItem("Umbriel");

      var context = new Revolver.Core.Context();
      context.CurrentItem = startItem;

      var result = context.SetContext("skoll[-1]");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
      Assert.That(context.CurrentDatabase.Name, Is.EqualTo("web"));
      Assert.That(context.CurrentItem.ID, Is.EqualTo(startItem.ID));
      Assert.That(context.CurrentLanguage.Name, Is.EqualTo("en"));
    }

    [Test]
    public void SetContext_NumberedChildTooHigh()
    {
      var startItem = _testTreeRoot.Axes.SelectSingleItem("Umbriel");

      var context = new Revolver.Core.Context();
      context.CurrentItem = startItem;

      var result = context.SetContext("skoll[5]");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
      Assert.That(context.CurrentDatabase.Name, Is.EqualTo("web"));
      Assert.That(context.CurrentItem.ID, Is.EqualTo(startItem.ID));
      Assert.That(context.CurrentLanguage.Name, Is.EqualTo("en"));
    }

    [Test]
    public void SetContext_BadDB()
    {
      var context = new Revolver.Core.Context();
      context.CurrentItem = _testTreeRoot;

      var item = _testTreeRoot.Axes.GetChild("Sycorax");

      var result = context.SetContext(item.Paths.FullPath, "nodb");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
      Assert.That(context.CurrentDatabase.Name, Is.EqualTo("web"));
      Assert.That(context.CurrentItem.ID, Is.EqualTo(_testTreeRoot.ID));
      Assert.That(context.CurrentLanguage.Name, Is.EqualTo("en"));
    }

    [Test]
    public void SetContext_BadLanguage()
    {
      var context = new Revolver.Core.Context();
      context.CurrentItem = _testTreeRoot;

      var item = _testTreeRoot.Axes.GetChild("Sycorax");

      var result = context.SetContext(item.Paths.FullPath + ":zz", null);

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
      Assert.That(context.CurrentDatabase.Name, Is.EqualTo("web"));
      Assert.That(context.CurrentItem.ID, Is.EqualTo(_testTreeRoot.ID));
      Assert.That(context.CurrentLanguage.Name, Is.EqualTo("en"));
    }

    [Test]
    public void SetContext_BadVersionNumber()
    {
      var context = new Revolver.Core.Context();
      context.CurrentItem = _testTreeRoot;

      var item = _testTreeRoot.Axes.GetChild("Sycorax");

      var result = context.SetContext(item.Paths.FullPath + "::&", null);

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
      Assert.That(context.CurrentDatabase.Name, Is.EqualTo("web"));
      Assert.That(context.CurrentItem.ID, Is.EqualTo(_testTreeRoot.ID));
      Assert.That(context.CurrentLanguage.Name, Is.EqualTo("en"));
    }

    // version too high

    // invalid path

    // invalid id

    // invalid language

    // leading db

    // separate db parameter

    // language in path

    // separate language parameter

    // version in path

    // separate version parameter

    // relative up tree

    // relative up tree with version

    // db, absolute, language, version

    
  }
}