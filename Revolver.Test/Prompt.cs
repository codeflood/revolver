using NUnit.Framework;
using Sitecore.Data.Items;

namespace Revolver.Test
{
  [TestFixture]
  [Category("Prompt")]
  public class Prompt : BaseCommandTest
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
      _context.EnvironmentVariables.Clear();
    }

    [Test]
    public void Item()
    {
      _context.CurrentItem = _testTreeRoot.Axes.SelectSingleItem("Juliet").Versions[new Sitecore.Data.Version(3)];
      var output = Revolver.Core.Prompt.EvaluatePrompt(_context, "%path% %itemname% %ver%");
      Assert.That(output, Is.EqualTo(_context.CurrentItem.Paths.FullPath + " Juliet 3"));
    }

    [Test]
    public void DB()
    {
      _context.CurrentItem = _testTreeRoot.Axes.SelectSingleItem("Juliet").Versions[new Sitecore.Data.Version(3)];
      var output = Revolver.Core.Prompt.EvaluatePrompt(_context, "%db%");
      Assert.That(output, Is.EqualTo(_context.CurrentDatabase.Name));
    }

    [Test]
    public void Language()
    {
      _context.CurrentItem = _testTreeRoot.Axes.SelectSingleItem("Juliet").Versions[new Sitecore.Data.Version(3)];
      var output = Revolver.Core.Prompt.EvaluatePrompt(_context, "%lang% %langcode%");
      Assert.That(output, Is.EqualTo("English en"));
    }

    [Test]
    public void EnvironmentVariable()
    {
      _context.CurrentItem = _testTreeRoot.Axes.SelectSingleItem("Juliet").Versions[new Sitecore.Data.Version(3)];
      _context.EnvironmentVariables.Add("a", "b");
      var output = Revolver.Core.Prompt.EvaluatePrompt(_context, "$a$");
      Assert.That(output, Is.EqualTo("b"));
    }

    [Test]
    public void EnvironmentVariable_NotPresent()
    {
      _context.CurrentItem = _testTreeRoot.Axes.SelectSingleItem("Juliet").Versions[new Sitecore.Data.Version(3)];
      _context.EnvironmentVariables.Add("a", "b");
      var output = Revolver.Core.Prompt.EvaluatePrompt(_context, "$c$");
      Assert.That(output, Is.EqualTo("$c$"));
    }
  }
}