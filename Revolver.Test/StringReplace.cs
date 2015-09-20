using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("StringReplace")]
  public class StringReplace : BaseCommandTest
  {
    private Cmd.StringReplace cmd = null;

    [TestFixtureSetUp]
    public void Init()
    {
      cmd = new Revolver.Core.Commands.StringReplace();
      base.InitCommand(cmd);
    }

    [Test]
    public void SimpleText()
    {
      cmd.Input = "input";
      cmd.RegexMatch = "in";
      cmd.RegexReplace = "out";
      string output = cmd.Run();
      Assert.AreEqual("output", output);
    }

    [Test]
    public void CaptureReplace()
    {
      cmd.Input = "input";
      cmd.RegexMatch = "(in)";
      cmd.RegexReplace = "$1put";
      string output = cmd.Run();
      Assert.AreEqual("inputput", output);
    }

    [Test]
    public void SimpleText2()
    {
      cmd.Input = "The companies name is WDG";
      cmd.RegexMatch = "WDG";
      cmd.RegexReplace = "Next Digital";
      string output = cmd.Run();
      Assert.AreEqual("The companies name is Next Digital", output);
    }

    [Test]
    public void CaptureReplace2()
    {
      cmd.Input = "The companies name is WDG";
      cmd.RegexMatch = "(WDG)";
      cmd.RegexReplace = "Next Digital and was $1";
      string output = cmd.Run();
      Assert.AreEqual("The companies name is Next Digital and was WDG", output);
    }

    [Test]
    public void Decompose()
    {
      cmd.Input = "day/month/year";
      cmd.RegexMatch = @"^(\w+)/(\w+)/(\w+)$";
      cmd.RegexReplace = "$3 $2 $1";
      string output = cmd.Run();
      Assert.AreEqual("year month day", output);
    }

    [Test]
    public void Remove()
    {
      cmd.Input = "de Title";
      cmd.RegexMatch = @"de\s";
      cmd.RegexReplace = "";
      string output = cmd.Run();
      Assert.AreEqual("Title", output);
    }

    [Test]
    public void InvalidRegex()
    {
      cmd.Input = "This is input";
      cmd.RegexMatch = "[s";
      cmd.RegexReplace = "";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }
  }
}
