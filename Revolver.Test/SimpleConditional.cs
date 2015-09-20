using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("SimpleConditional")]
  public class SimpleConditional : BaseCommandTest
  {
    [Test]
    public void MissingExpression()
    {
      var cmd = new Cmd.SimpleConditional();
      InitCommand(cmd);

      cmd.Command = "echo ok";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void MissingCommand()
    {
      var cmd = new Cmd.SimpleConditional();
      InitCommand(cmd);

      cmd.Expression = "a = a";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void Passes()
    {
      var cmd = new Cmd.SimpleConditional();
      InitCommand(cmd);

      cmd.Expression = "a = a";
      cmd.Command = "echo ok";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringContaining("ok"));
    }

    [Test]
    public void NotPasses()
    {
      var cmd = new Cmd.SimpleConditional();
      InitCommand(cmd);

      cmd.Expression = "a != a";
      cmd.Command = "echo ok";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.Empty);
    }
  }
}