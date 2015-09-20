using NUnit.Framework;
using Sitecore.Data.Items;
using System;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ItemInspector")]
  public class ItemInspector : BaseCommandTest
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
    [ExpectedException(typeof(ArgumentNullException))]
    public void NullItem()
    {
      var inspector = new Revolver.Core.ItemInspector(null);
    }

    [Test]
    public void GetAttribute_Name()
    {
      var inspector = new Revolver.Core.ItemInspector(_testTreeRoot);
      var result = inspector.GetItemAttribute("name");

      Assert.That(result, Is.EqualTo(_testTreeRoot.Name));
    }

    [Test]
    public void GetAttribute_ID()
    {
      var inspector = new Revolver.Core.ItemInspector(_testTreeRoot);
      var result = inspector.GetItemAttribute("id");

      Assert.That(result, Is.EqualTo(_testTreeRoot.ID.ToString()));
    }

    [Test]
    public void GetAttribute_Version()
    {
      var inspector = new Revolver.Core.ItemInspector(_testTreeRoot);
      var result = inspector.GetItemAttribute("version");

      Assert.That(result, Is.EqualTo(_testTreeRoot.Version.ToString()));
    }

    [Test]
    public void GetAttribute_MixedCaseAttribute()
    {
      var inspector = new Revolver.Core.ItemInspector(_testTreeRoot);
      var result = inspector.GetItemAttribute("TemplateId");

      Assert.That(result, Is.EqualTo(_testTreeRoot.TemplateID.ToString()));
    }

    [Test]
    public void GetAttribute_LeadingAt()
    {
      var inspector = new Revolver.Core.ItemInspector(_testTreeRoot);
      var result = inspector.GetItemAttribute("@@key");

      Assert.That(result, Is.EqualTo(_testTreeRoot.Key));
    }

    [Test]
    public void GetAttribute_InvalidAttribute()
    {
      var inspector = new Revolver.Core.ItemInspector(_testTreeRoot);
      var result = inspector.GetItemAttribute("blah");

      Assert.That(result, Is.Null);
    }

    [Test]
    public void CountDescendants_HasChildren()
    {
      var inspector = new Revolver.Core.ItemInspector(_testTreeRoot);
      var result = inspector.CountDescendants();

      Assert.That(result, Is.EqualTo(8));
    }

    [Test]
    public void CountDescendants_NoChildren()
    {
      var inspector = new Revolver.Core.ItemInspector(_testTreeRoot.Axes.GetChild("Sycorax"));
      var result = inspector.CountDescendants();

      Assert.That(result, Is.EqualTo(1));
    }
  }
}