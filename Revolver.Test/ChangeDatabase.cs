using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ChangeDatabase")]
  public class ChangeDatabase : BaseCommandTest
  {
    private Cmd.ChangeDatabase _changeDatabase = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;
    }

    [SetUp]
    public void SetUp()
    {
      _changeDatabase = new Revolver.Core.Commands.ChangeDatabase();
      base.InitCommand(_changeDatabase);
      _context.CurrentDatabase = Sitecore.Configuration.Factory.GetDatabase("master");
    }

    [Test]
    public void ToWeb()
    {
      _changeDatabase.DatabaseName = "web";
      
      var res = _changeDatabase.Run();

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual("Database web", res.Message);
      Assert.AreEqual(Sitecore.Configuration.Factory.GetDatabase("web").Name, _context.CurrentDatabase.Name);
    }

    [Test]
    public void ToCore()
    {
      _changeDatabase.DatabaseName = "core";

      var res = _changeDatabase.Run();

      Assert.AreEqual(CommandStatus.Success, res.Status);
      Assert.AreEqual("Database core", res.Message);
      Assert.AreEqual(Sitecore.Configuration.Factory.GetDatabase("core").Name, _context.CurrentDatabase.Name);
    }

    [Test]
    public void ToInvalid()
    {
      _changeDatabase.DatabaseName = "bler";

      CommandResult res = _changeDatabase.Run();
      Assert.AreEqual(CommandStatus.Failure, res.Status);
    }

    [Test]
    public void MissingName()
    {
      _changeDatabase.DatabaseName = "";

      CommandResult res = _changeDatabase.Run();
      Assert.AreEqual(CommandStatus.Failure, res.Status);
      Assert.AreEqual("Required parameter 'name' is missing", res.Message);
    }
  }
}
