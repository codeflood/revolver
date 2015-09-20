using NUnit.Framework;
using Revolver.Core;
using Revolver.Core.Commands;
using Revolver.UI;
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
    [Test]
    public void BindCustomCommand()
    {
      var formatter = new TextOutputFormatter();

      var ctx = new Core.Context();
      ctx.CommandHandler = new Core.CommandHandler(ctx, formatter);

      var cmd = new Mod.BindCommand();
      cmd.Initialise(ctx, formatter);

      cmd.Command = "Revolver.Test.BindCommand+CustomCommand, Revolver.Test";
      cmd.CommandName = "c";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(ctx.CommandHandler.CustomCommands, Contains.Item(new KeyValuePair<string, Type>("c", typeof(CustomCommand))));
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
    public void BindCustomCommand_MissingCommandName()
    {
      var formatter = new TextOutputFormatter();

      var ctx = new Core.Context();
      ctx.CommandHandler = new Core.CommandHandler(ctx, formatter);

      var cmd = new Mod.BindCommand();
      cmd.Initialise(ctx, formatter);

      cmd.Command = "Revolver.Test.BindCommand+CustomCommand, Revolver.Test";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
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
    public void AddAlias()
    {
      var formatter = new TextOutputFormatter();

      var ctx = new Core.Context();
      ctx.CommandHandler = new Core.CommandHandler(ctx, formatter);
      ctx.CommandHandler.AddCustomCommand("c", typeof(CustomCommand));

      var cmd = new Mod.BindCommand();
      cmd.Initialise(ctx, formatter);

      cmd.Command = "c";
      cmd.CommandName = "cc";
      cmd.ProcessAsAlias = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success), result.Message);
      Assert.That(ctx.CommandHandler.CustomCommands, Contains.Item(new KeyValuePair<string, Type>("c", typeof(CustomCommand))));

      var cmdArgs = ctx.CommandHandler.FindCommandAlias("cc");

      Assert.That(cmdArgs, Is.Not.Null);
      Assert.That(cmdArgs.CommandName, Is.EqualTo("c"));
    }

    [Test]
    public void RemoveAlias()
    {
      var formatter = new TextOutputFormatter();

      var ctx = new Core.Context();
      ctx.CommandHandler = new Core.CommandHandler(ctx, formatter);
      ctx.CommandHandler.AddCustomCommand("c", typeof(CustomCommand));
      ctx.CommandHandler.AddCommandAlias("cc", "c");

      var cmd = new Mod.BindCommand();
      cmd.Initialise(ctx, formatter);

      cmd.Remove = true;
      cmd.CommandName = "cc";
      cmd.ProcessAsAlias = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success), result.Message);
      Assert.That(ctx.CommandHandler.CustomCommands, Contains.Item(new KeyValuePair<string, Type>("c", typeof(CustomCommand))));

      var cmdArgs = ctx.CommandHandler.FindCommandAlias("cc");

      Assert.That(cmdArgs, Is.Null);
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

    private class CustomCommand : ICommand
    {
      public string Description()
      {
        return "A custom command";
      }

      public void Help(Core.HelpDetails details)
      {
        details.Description = Description();
      }

      public void Initialise(Core.Context context, ICommandFormatter formatter)
      {
      }

      public Core.CommandResult Run()
      {
        return new CommandResult(CommandStatus.Success, "boo");
      }
    }
  }
}