using NUnit.Framework;
using Sitecore.Data.Items;

namespace Revolver.Test
{
  [TestFixture]
  [Category("PathParser")]
  public class PathParser : BaseCommandTest
  {
    Item _testTreeRoot = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      InitContent();
      _testTreeRoot = TestUtil.CreateContentFromFile("TestResources\\narrow tree with duplicate names.xml", _testRoot);
    }

    [SetUp]
    public void SetUp()
    {
      _context.CurrentItem = _testTreeRoot;
    }

    [Test]
    public void EvaluatePath_DownTreeRelative()
    {
      var path = Revolver.Core.PathParser.EvaluatePath(_context, "Umbriel/Ymir");
      Assert.That(path, Is.EqualTo(_testTreeRoot.Paths.FullPath + "/Umbriel/Ymir"));
    }

    [Test]
    public void EvaluatePath_UpTreeRelative()
    {
      _context.CurrentItem = _testTreeRoot.Axes.SelectSingleItem("Umbriel/Ymir");
      var path = Revolver.Core.PathParser.EvaluatePath(_context, "../..");
      Assert.That(path, Is.EqualTo(_testTreeRoot.Paths.FullPath));
    }

    [Test]
    public void EvaluatePath_UpAndDownRelative()
    {
      _context.CurrentItem = _testTreeRoot.Axes.SelectSingleItem("Juliet");
      var path = Revolver.Core.PathParser.EvaluatePath(_context, "../Sycorax");
      Assert.That(path, Is.EqualTo(_testTreeRoot.Paths.FullPath + "/Sycorax"));
    }

    [Test]
    public void EvaluatePath_UpAndDownMultiple()
    {
      _context.CurrentItem = _testTreeRoot.Axes.SelectSingleItem("Umbriel");
      var path = Revolver.Core.PathParser.EvaluatePath(_context, "../Sycorax/../Juliet");
      Assert.That(path, Is.EqualTo(_testTreeRoot.Paths.FullPath + "/Juliet"));
    }

    [Test]
    public void EvaluatePath_Absolute()
    {
      var item = _testTreeRoot.Axes.SelectSingleItem("Umbriel");
      var path = Revolver.Core.PathParser.EvaluatePath(_context, item.Paths.FullPath);
      Assert.That(path, Is.EqualTo(item.Paths.FullPath));
    }

    [Test]
    public void ParseLanguageFromPath_Present()
    {
      var result = Revolver.Core.PathParser.ParseLanguageFromPath("/sitecore/content:de");
      Assert.That(result, Is.EqualTo("de"));
    }

    [Test]
    public void ParseLanguageFromPath_Missing()
    {
      var result = Revolver.Core.PathParser.ParseLanguageFromPath("/sitecore/content:");
      Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParseLanguageFromPath_NotPresent()
    {
      var result = Revolver.Core.PathParser.ParseLanguageFromPath("/sitecore/content");
      Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParseLanguageFromPath_VersionPresent()
    {
      var result = Revolver.Core.PathParser.ParseLanguageFromPath("/sitecore/content:de:4");
      Assert.That(result, Is.EqualTo("de"));
    }

    [Test]
    public void ParseLanguageFromPath_VersionOnly()
    {
      var result = Revolver.Core.PathParser.ParseLanguageFromPath("/sitecore/content::4");
      Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParseLanguageFromPath_NoPath()
    {
      var result = Revolver.Core.PathParser.ParseLanguageFromPath(":de");
      Assert.That(result, Is.EqualTo("de"));
    }

    [Test]
    public void ParseVersionFromPath_Present()
    {
      var result = Revolver.Core.PathParser.ParseVersionFromPath("/sitecore/content::4");
      Assert.That(result, Is.EqualTo("4"));
    }

    [Test]
    public void ParseVersionFromPath_Missing()
    {
      var result = Revolver.Core.PathParser.ParseVersionFromPath("/sitecore/content::");
      Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParseVersionFromPath_NotPresent()
    {
      var result = Revolver.Core.PathParser.ParseVersionFromPath("/sitecore/content");
      Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParseVersionFromPath_LanguagePresent()
    {
      var result = Revolver.Core.PathParser.ParseVersionFromPath("/sitecore/content:fr:7");
      Assert.That(result, Is.EqualTo("7"));
    }

    [Test]
    public void ParseVersionFromPath_NoPath()
    {
      var result = Revolver.Core.PathParser.ParseVersionFromPath("::4");
      Assert.That(result, Is.EqualTo("4"));
    }
  }
}