using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ChangeVersion")]
  public class ChangeVersion : BaseCommandTest
  {
    Cmd.ChangeVersion _changeVersion = null;
    Item _testItem = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;

      using (new SecurityDisabler())
      {
        InitContent();
        _testItem = _testRoot.Add("test item", _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate]);
        _testItem = _testItem.Versions.AddVersion();
        _testItem = _testItem.Versions.AddVersion();
        _testItem = _testItem.Versions.AddVersion();
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
    }

    [SetUp]
    public void SetUp()
    {
      _changeVersion = new Cmd.ChangeVersion();
      InitCommand(_changeVersion);
      _context.CurrentItem = _testItem;
    }

    [Test]
    public void NoParameters()
    {
      var result = _changeVersion.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }

    [Test]
    public void NumberedVersion_Valid()
    {
      _changeVersion.Version = 2;
      var result = _changeVersion.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual(2, _context.CurrentItem.Version.Number);
    }

    [Test]
    public void NumberedVersion_FromLatest()
    {
      _changeVersion.Version = -1;
      var result = _changeVersion.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual(3, _context.CurrentItem.Version.Number);
    }

    [Test]
    public void NumberedVersion_ToLatest()
    {
      _changeVersion.Latest = true;
      var result = _changeVersion.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual(4, _context.CurrentItem.Version.Number);
    }
  }
}