using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("SplitString")]
	public class SplitString : BaseCommandTest
	{
		[Test]
    public void MissingInput()
    {
      var cmd = new Cmd.SplitString();
      InitCommand(cmd);

      cmd.Command = "echo $current$";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void MissingCommand()
    {
      var cmd = new Cmd.SplitString();
      InitCommand(cmd);

      cmd.Input = "id1|id2|id3";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void SplitOnNewline()
    {
      var cmd = new Cmd.SplitString();
      InitCommand(cmd);

      cmd.Input = "id1\r\nid2\r\nid3";
      cmd.Command = "echo $current$";
      cmd.SplitOnNewLine = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringMatching("\\bid1\\b"));
      Assert.That(result.Message, Is.StringMatching("\\bid2\\b"));
      Assert.That(result.Message, Is.StringMatching("\\bid3\\b"));
      Assert.That(result.Message, Contains.Substring("Processed 3 strings"));
    }

    [Test]
    public void SplitOnTabWithEmpties()
    {
      var cmd = new Cmd.SplitString();
      InitCommand(cmd);

      cmd.Input = "id1\tid2\t\tid3";
      cmd.Command = "echo $current$";
      cmd.SplitOnTab = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringMatching("\\bid1\\b"));
      Assert.That(result.Message, Is.StringMatching("\\bid2\\b"));
      Assert.That(result.Message, Is.StringMatching("\\bid3\\b"));
      Assert.That(result.Message, Contains.Substring("Processed 3 strings"));
    }

    [Test]
    public void SplitOnSymbol()
    {
      var cmd = new Cmd.SplitString();
      InitCommand(cmd);

      cmd.Input = "a|b|c";
      cmd.Command = "echo $current$";
      cmd.SplitSymbol = "|";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringMatching("\\ba\\b"));
      Assert.That(result.Message, Is.StringMatching("\\bb\\b"));
      Assert.That(result.Message, Is.StringMatching("\\bc\\b"));
      Assert.That(result.Message, Contains.Substring("Processed 3 strings"));
    }

    [Test]
    public void SplitOnMultiple()
    {
      var cmd = new Cmd.SplitString();
      InitCommand(cmd);

      cmd.Input = "a\t1|b\t2|c\t3";
      cmd.Command = "echo $current$";
      cmd.SplitOnTab = true;
      cmd.SplitSymbol = "|";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringMatching("\\ba\\b"));
      Assert.That(result.Message, Is.StringMatching("\\b1\\b"));
      Assert.That(result.Message, Is.StringMatching("\\bb\\b"));
      Assert.That(result.Message, Is.StringMatching("\\b2\\b"));
      Assert.That(result.Message, Is.StringMatching("\\bc\\b"));
      Assert.That(result.Message, Is.StringMatching("\\b3\\b"));
      Assert.That(result.Message, Contains.Substring("Processed 6 strings"));
    }

    [Test]
    public void NoStats()
    {
      var cmd = new Cmd.SplitString();
      InitCommand(cmd);

      cmd.Input = "a|b|c";
      cmd.Command = "echo $current$";
      cmd.SplitSymbol = "|";
      cmd.NoStatistics = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringMatching("\\ba\\b"));
      Assert.That(result.Message, Is.StringMatching("\\bb\\b"));
      Assert.That(result.Message, Is.StringMatching("\\bc\\b"));
      Assert.That(result.Message, Is.Not.ContainsSubstring("3"));
    }

    [Test]
    public void VariableAlreadyExists()
    {
      var cmd = new Cmd.SplitString();
      InitCommand(cmd);

      _context.EnvironmentVariables.Add("current", "existing");
      cmd.Input = "a|b|c";
      cmd.Command = "echo $current$";
      cmd.SplitSymbol = "|";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringMatching("\\ba\\b"));
      Assert.That(result.Message, Is.StringMatching("\\bb\\b"));
      Assert.That(result.Message, Is.StringMatching("\\bc\\b"));
      Assert.That(result.Message, Contains.Substring("WARNING"));
    }
	}
}
