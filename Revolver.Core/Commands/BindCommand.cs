using Sitecore.StringExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("bind")]
  public class BindCommand : BaseCommand
  {
    [FlagParameter("r")]
    [Description("Remove the binding.")]
    [Optional]
    public bool Remove { get; set; }

    [FlagParameter("a")]
    [Description("Process as a command alias.")]
    [Optional]
    public bool ProcessAsAlias { get; set; }

    [NumberedParameter(0, "commandName")]
    [Description("The name to bind the command to.")]
    public string CommandName { get; set; }

    [NumberedParameter(1, "command")]
    [Description("The command to bind. If not an existing command, must be a fully qualified type.")]
    [Optional]
    public string Command { get; set; }

    [ListParameter("parameters")]
    [Description("Additional parameters to use for an alias. Cannot be used when binding a new command.")]
    [Optional]
    public IList<string> Parameters { get; set; }

    public BindCommand()
    {
      ProcessAsAlias = false;
      Remove = false;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Command) && string.IsNullOrEmpty(CommandName) && !Remove)
        return new CommandResult(CommandStatus.Success, PrintCurrentBindings());

      if(string.IsNullOrEmpty(CommandName))
          return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("commandName"));

      if (string.IsNullOrEmpty(Command) && !Remove)
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("commandClass"));

      if(!string.IsNullOrEmpty(Command) && Remove)
        return new CommandResult(CommandStatus.Failure, "Cannot use -r with 'command' parameter");

      if (ProcessAsAlias)
      {
        if (Remove)
          return Context.CommandHandler.RemoveCommandAlias(CommandName);
        else
          return BindAlias(CommandName, Command, Parameters != null ? Parameters.ToArray() : null);
      }

      if (Remove)
      {
        if (Context.CommandHandler.RemoveCustomCommand(CommandName))
          return new CommandResult(CommandStatus.Success, string.Format("Removed command '{0}'", CommandName));
        else
          return new CommandResult(CommandStatus.Failure, string.Format("Failed to find command '{0}'", CommandName));
      }

      if (Parameters != null && Parameters.Count > 0)
        return new CommandResult(CommandStatus.Failure, "Parameters are only valid for aliases");

      return BindNewCommand(Command, CommandName);
    }

    private string PrintCurrentBindings()
    {
      StringBuilder sb = new StringBuilder();

      Formatter.PrintLine("Available core commands:", sb);
      Formatter.PrintLine(new string('-', 50), sb);

      foreach(var command in Context.CommandHandler.CoreCommands.OrderBy(x => x.Key))
      {
        var value = command.Value.FullName;

        var aliases = Context.CommandHandler.GetCommandAliasesFor(command.Key).ToArray();
        if(aliases.Any())
          value += " (" + string.Join(", ", aliases) + ")";

        Formatter.PrintDefinition(command.Key, value, sb);
      }

      if (Context.CommandHandler.CustomCommands.Count > 0)
      {
        Formatter.PrintLine(string.Empty, sb);
        Formatter.PrintLine("Available custom commands:", sb);
        Formatter.PrintLine(new string('-', 50), sb);

        foreach(var command in Context.CommandHandler.CustomCommands.OrderBy(x => x.Key))
        {
          Formatter.PrintDefinition(command.Key, command.Value.FullName, sb);
        }
      }

      return sb.ToString();
    }

    public override string Description()
    {
      return "Bind a new command or rebind an existing command to a new name";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments = "-r cannot be used with 'command'";
      details.AddExample("-a dir ls");
      details.AddExample("-a geta ga");
      details.AddExample("mycommand (MyNamespace.MyClass, MyAssembly)");
      details.AddExample("-r geta");
    }

    /// <summary>
    /// Binds a new command for use
    /// </summary>
    /// <param name="typeName">The fully qualified name to find the type</param>
    /// <param name="moniker">The command name to bind to</param>
    /// <returns>A command result</returns>
    private CommandResult BindNewCommand(string typeName, string moniker)
    {
      // Try to load the type
      Type t = Type.GetType(typeName, false, true);
      if (t != null)
      {
        // Verify it implements the correct interface
        if (t.GetInterface(typeof(ICommand).Name) != null)
        {
          // Call into command handler to bind custom command
          if (Context.CommandHandler.AddCustomCommand(moniker, t))
            return new CommandResult(CommandStatus.Success, t.Name + " bound to " + moniker);
          else
            return new CommandResult(CommandStatus.Failure, "Failed to add command");
        }
        else
          return new CommandResult(CommandStatus.Failure, string.Format("Type {0} doesn't support interface {1}", t.Name, typeof(ICommand).Name));
      }
      else
        return new CommandResult(CommandStatus.Failure, "Failed to load type " + typeName);
    }

    private CommandResult BindAlias(string commandName, string command, string[] parameters)
    {
      return Context.CommandHandler.AddCommandAlias(commandName, command, parameters);
    }
  }
}
