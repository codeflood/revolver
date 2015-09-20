using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("RegexMatch")]
  public class RegexMatch : BaseCommandTest
  {
    [Test]
    public void EmptyInput()
    {
      var cmd = new Cmd.RegexMatch();
      InitCommand(cmd);

      cmd.Regex = ".*";

      var result = cmd.Run();
      
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
    }

    [Test]
    public void EmptyRegex()
    {
      var cmd = new Cmd.RegexMatch();
      InitCommand(cmd);

      cmd.Input = "I sat by the ocean";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void Match()
    {
      var cmd = new Cmd.RegexMatch();
      InitCommand(cmd);

      cmd.Input = "I sat by the ocean";
      cmd.Regex = "\\socean";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(" ocean"));
    }

    [Test]
    public void NoMatch()
    {
      var cmd = new Cmd.RegexMatch();
      InitCommand(cmd);

      cmd.Input = "I sat by the ocean";
      cmd.Regex = "moon";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.Empty);
    }

    [Test]
    public void Match_CaseSensitive()
    {
      var cmd = new Cmd.RegexMatch();
      InitCommand(cmd);

      cmd.Input = "I sat by the ocean";
      cmd.Regex = "\\socean";
      cmd.CaseSensitive = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(" ocean"));
    }

    [Test]
    public void NoMatch_CaseSensitive()
    {
      var cmd = new Cmd.RegexMatch();
      InitCommand(cmd);

      cmd.Input = "I sat by the Ocean";
      cmd.Regex = "\\socean";
      cmd.CaseSensitive = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.Empty);
    }
  }
}