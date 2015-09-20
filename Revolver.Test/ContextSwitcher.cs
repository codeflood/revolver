using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data.Items;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ContextSwitcher")]
  public class ContextSwitcher : BaseCommandTest
  {
    Item _testContent = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      InitContent();
      _testContent = TestUtil.CreateContentFromFile("TestResources\\narrow tree.xml", _testRoot);
    }

    [Test]
    public void RelativePathDescendOnly()
    {
      var target = _context.CurrentDatabase.GetItem(_testContent.Paths.FullPath + "/luna/carme");
      _context.CurrentItem = _testContent;
      using (new Revolver.Core.ContextSwitcher(_context, "luna/carme"))
      {
        Assert.That(_context.CurrentItem.ID, Is.EqualTo(target.ID));
      }
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void RelativePathTraverse()
    {
      var start = _context.CurrentDatabase.GetItem(_testContent.Paths.FullPath + "/phobos");
      var target = _context.CurrentDatabase.GetItem(_testContent.Paths.FullPath + "/luna/carme");

      _context.CurrentItem = start;
      using (new Revolver.Core.ContextSwitcher(_context, "../luna/carme"))
      {
        Assert.That(_context.CurrentItem.ID, Is.EqualTo(target.ID));
      }
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(start.ID));
    }

    [Test]
    public void AbsolutePath()
    {
      var target = _context.CurrentDatabase.GetItem(_testContent.Paths.FullPath + "/deimos");

      _context.CurrentItem = _testContent;
      using (new Revolver.Core.ContextSwitcher(_context, target.Paths.FullPath))
      {
        Assert.That(_context.CurrentItem.ID, Is.EqualTo(target.ID));
      }
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void ByID()
    {
      var target = _context.CurrentDatabase.GetItem(_testContent.Paths.FullPath + "/luna");

      _context.CurrentItem = _testContent;
      using (new Revolver.Core.ContextSwitcher(_context, target.ID.ToString()))
      {
        Assert.That(_context.CurrentItem.ID, Is.EqualTo(target.ID));
      }
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void Empty()
    {
      _context.CurrentItem = _testContent;
      using(new Revolver.Core.ContextSwitcher(_context, string.Empty))
      {
        Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
      }
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }

    [Test]
    public void Invalid()
    {
      _context.CurrentItem = _testContent;
      using (var cs = new Revolver.Core.ContextSwitcher(_context, "blah blah invalid item"))
      {
        Assert.That(cs.Result.Status, Is.EqualTo(CommandStatus.Failure));
        Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
      }
      Assert.That(_context.CurrentItem.ID, Is.EqualTo(_testContent.ID));
    }
  }
}
