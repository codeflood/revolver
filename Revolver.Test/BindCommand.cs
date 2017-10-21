using NUnit.Framework;
using Revolver.Core;
using System;
using System.Collections.Generic;
using Revolver.Core.Formatting;
using Mod = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("BindCommand")]
  public class BindCommand
  {
    [TestCase("c", "Revolver.Test.CustomCommand, Revolver.Test", "c", TestName = "Separate command name")]
    [TestCase("Revolver.Test.CustomCommand, Revolver.Test", null, "cc", TestName = "Command name from attribute")]
    public void BindCustomCommand(string commandName, string command, string expectedBoundCommandName)
    {
      var formatter = new TextOutputFormatter();

      var ctx = new Core.Context();
      ctx.CommandHandler = new Core.CommandHandler(ctx, formatter);

      var cmd = new Mod.BindCommand();
      cmd.Initialise(ctx, formatter);

      cmd.CommandName = commandName;

      if(command != null)
        cmd.Command = command;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(ctx.CommandHandler.CustomCommands, Contains.Item(new KeyValuePair<string, Type>(expectedBoundCommandName, typeof(CustomCommand))));
    }

    [Test]
    public void BindCustomCommand_InvalidType()
    {
      var formatter = new TextOutputFormatter();

      var ctx = new Core.Context();
      ctx.CommandHandler = new Core.CommandHandler(ctx, formatter);

      var cmd = new Mod.BindCommand();
      cmd.Initialise(ctx, formatter);

      cmd.Command = "Revolver.Test.BindCommand, Revolver.Test";
      cmd.CommandName = "c";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void BindCustomCommand_UnknownType()
    {
      var formatter = new TextOutputFormatter();

      var ctx = new Core.Context();
      ctx.CommandHandler = new Core.CommandHandler(ctx, formatter);

      var cmd = new Mod.BindCommand();
      cmd.Initialise(ctx, formatter);

      cmd.Command = "Lorem, Ipsum";
      cmd.CommandName = "c";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void BindCustomCommand_MissingCommand()
    {
      var formatter = new TextOutputFormatter();

      var ctx = new Core.Context();
      ctx.CommandHandler = new Core.CommandHandler(ctx, formatter);

      var cmd = new Mod.BindCommand();
      cmd.Initialise(ctx, formatter);

      cmd.CommandName = "c";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void BindAllCustomCommands()
    {
      var formatter = new TextOutputFormatter();

      var ctx = new Core.Context();
      ctx.CommandHandler = new Core.CommandHandler(ctx, formatter);

      var cmd = new Mod.BindCommand();
      cmd.Initialise(ctx, formatter);

      cmd.CommandName = "Revolver.Test";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(ctx.CommandHandler.CustomCommands, Contains.Item(new KeyValuePair<string, Type>("cc", typeof(CustomCommand))));
      Assert.That(ctx.CommandHandler.CustomCommands, Contains.Item(new KeyValuePair<string, Type>("cc2", typeof(CustomCommand2))));
    }

    [Test]
    public void RemoveCustomCommand()
    {
      var formatter = new TextOutputFormatter();

      var ctx = new Core.Context();
      ctx.CommandHandler = new Core.CommandHandler(ctx, formatter);
      ctx.CommandHandler.AddCustomCommand("c", typeof (CustomCommand));

      var cmd = new Mod.BindCommand();
      cmd.Initialise(ctx, formatter);

      cmd.CommandName = "c";
      cmd.Remove = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(ctx.CommandHandler.CustomCommands, Is.Not.Contains(new KeyValuePair<string, Type>("c", typeof(CustomCommand))));
    }

    [Test]
    public void ListBindings()
    {
      var formatter = new TextOutputFormatter();

      var ctx = new Core.Context();
      ctx.CommandHandler = new Core.CommandHandler(ctx, formatter);

      var cmd = new Mod.BindCommand();
      cmd.Initialise(ctx, formatter);

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success), result.Message);
      Assert.That(result.Message, Contains.Substring("cd"));
      Assert.That(result.Message, Contains.Substring("ls"));
    }
  }
}