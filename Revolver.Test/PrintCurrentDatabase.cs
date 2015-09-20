using NUnit.Framework;
using Revolver.Core;
using Sitecore.Configuration;

namespace Revolver.Test
{
  [TestFixture]
  [Category("PrintCurrentDatabase")]
  public class PrintCurrentDatabase : BaseCommandTest
  {
    private Revolver.Core.Commands.PrintCurrentDatabase _printCurrentDatabase = null;

    [TestFixtureSetUp]
    public void Init()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      _printCurrentDatabase = new Revolver.Core.Commands.PrintCurrentDatabase();
      base.InitCommand(_printCurrentDatabase);
    }

    [Test]
    public void Master()
    {
      _context.CurrentDatabase = Factory.GetDatabase("master");
      var result = _printCurrentDatabase.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual(_context.CurrentDatabase.Name, result.Message);
    }

    [Test]
    public void Web()
    {
      _context.CurrentDatabase = Factory.GetDatabase("web");
      var result = _printCurrentDatabase.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual(_context.CurrentDatabase.Name, result.Message);
    }

    [Test]
    public void Core()
    {
      _context.CurrentDatabase = Factory.GetDatabase("core");
      var result = _printCurrentDatabase.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual(_context.CurrentDatabase.Name, result.Message);
    }
  }
}
