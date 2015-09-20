using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ChangeLanguage")]
  public class ChangeLanguageTest : BaseCommandTest
  {
    Cmd.ChangeLanguage _command = null;
    Item _germanLanguageDef = null;
    bool _revertLanguage = false;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      _revertLanguage = !TestUtil.IsGermanRegistered(_context);

      if (_revertLanguage)
        _germanLanguageDef = TestUtil.RegisterGermanLanaguage(_context);
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      if (_revertLanguage)
      {
        using (new SecurityDisabler())
        {
          _germanLanguageDef.Delete();
        }
      }
    }

    [SetUp]
    public void SetUp()
    {
      _context.CurrentLanguage = Language.Parse("en");
      _command = new Cmd.ChangeLanguage();
      InitCommand(_command);
    }

    [Test]
    public void ParametersNotValid()
    {
      var res = _command.Run();
      Assert.AreEqual(CommandStatus.Failure, res.Status);
      Assert.AreEqual("Either 'language' or -d is required", res.Message);
    }

    [Test]
    public void ValidLanguage()
    {
      _command.Language = "de";

      var res = _command.Run();
      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual("de", _context.CurrentLanguage.Name);
    }

    [Test]
    public void InvalidLanguage()
    {
      _command.Language = "zh";

      var res = _command.Run();
      Assert.AreEqual(CommandStatus.Failure, res.Status);
    }

    [Test]
    public void BadLanguageParameter()
    {
      _command.Language = "not a language";

      var res = _command.Run();
      Assert.AreEqual(CommandStatus.Failure, res.Status);
      Assert.AreEqual("Failed to parse language 'not a language'", res.Message);
    }

    [Test]
    public void InvalidLanguageForce()
    {      
      _command.Language = "zh";
      _command.Force = true;

      var res = _command.Run();
      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual("zh", _context.CurrentLanguage.Name);
    }

    [Test]
    public void DefaultLanguage()
    {
      _command.DefaultLanguage = true;

      var res = _command.Run();
      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual(Sitecore.Context.Site.Language, _context.CurrentLanguage.Name);
    }
  }
}
