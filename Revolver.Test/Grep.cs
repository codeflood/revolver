using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("Grep")]
  public class Grep : BaseCommandTest
  {
    [Test]
    public void InStrings()
    {
      var cmd = new Cmd.Grep();
      base.InitCommand(cmd);
      cmd.Regex = "\\bc.+";
      cmd.Input = "Contains matched line\r\nNo match\r\nNo match\r\nContains";
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual("Contains matched line\r\nContains", result.Message.Trim());
    }

    [Test]
    public void InStringsCaseSensitive()
    {
      var cmd = new Cmd.Grep();
      base.InitCommand(cmd);
      cmd.CaseSensitive = true;
      cmd.Regex = "^c.+";
      cmd.Input = "Contains matched line\r\nNo match\r\nNo match\r\ncontains";
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual("contains", result.Message.Trim());
    }

    [Test]
    public void InStringsRegexNumber()
    {
      var cmd = new Cmd.Grep();
      base.InitCommand(cmd);
      cmd.Regex = "\\d";
      cmd.Input = "Line 1\r\nLine Two\r\nLine Three";
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual("Line 1", result.Message.Trim());
    }

    [Test]
    public void NotMatchingLines()
    {
      var cmd = new Cmd.Grep();
      base.InitCommand(cmd);
      cmd.Regex = "\\bc.+";
      cmd.Input = "Contains matched line\r\nNo match\r\nNo match\r\nContains";
      cmd.NotMatching = true;
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual("No match\r\nNo match", result.Message.Trim());
    }

    [Test]
    public void NoMatches()
    {
      var cmd = new Cmd.Grep();
      base.InitCommand(cmd);
      cmd.Regex = "-";
      cmd.Input = "Line 1\r\nLine Two\r\nLine Three";
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual(string.Empty, result.Message.Trim());
    }

    [Test]
    public void MissingInput()
    {
      var cmd = new Cmd.Grep();
      base.InitCommand(cmd);
      cmd.Regex = "\\d";
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }

    [Test]
    public void MissingRegex()
    {
      var cmd = new Cmd.Grep();
      base.InitCommand(cmd);
      cmd.Input = "Lorem ipsum";
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }
  }
}
