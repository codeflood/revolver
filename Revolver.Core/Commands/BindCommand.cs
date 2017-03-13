using Sitecore.StringExtensions;
using System;
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

    [NumberedParameter(0, "commandName")]
    [Description("The name to bind the command to.")]
    public string CommandName { get; set; }

    [NumberedParameter(1, "command")]
    [Description("The command to bind. If not an existing command, must be a fully qualified type.")]
    [Optional]
    public string Command { get; set; }

    public BindCommand()
    {
      Remove = false;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Command) && string.IsNullOrEmpty(CommandName) && !Remove)
        return new CommandResult(CommandStatus.Success, PrintCurrentBindings());

      if(!string.IsNullOrEmpty(Command) && Remove)
        return new CommandResult(CommandStatus.Failure, "Cannot use -r with 'command' parameter");

      if (Remove)
      {
        if (Context.CommandHandler.RemoveCustomCommand(CommandName))
          return new CommandResult(CommandStatus.Success, string.Format("Removed command '{0}'", CommandName));
        else
          return new CommandResult(CommandStatus.Failure, string.Format("Failed to find command '{0}'", CommandName));
      }

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
      // typeName would be empty if using the default command binding on the class and the command name wasn't provided in the parameters
      if (string.IsNullOrEmpty(typeName))
      {
        typeName = moniker;
        moniker = string.Empty;
      }

      // Try to load the type
      Type t = Type.GetType(typeName, false, true);
      if (t != null)
      {
        // Verify it implements the correct interface
        if (t.GetInterface(typeof(ICommand).Name) != null)
        {
          if (string.IsNullOrEmpty(moniker))
          {
            // If no moniker is provided, grab the moniker from the command attribute
            var commandAttr = CommandInspector.GetCommandAttribute(t);
            if (commandAttr != null)
              moniker = commandAttr.Binding;
            else
              return new CommandResult(CommandStatus.Failure, string.Format("Type {0} does not have a command attribute. You must specify the commandName", t.Name));
          }

          if(string.IsNullOrEmpty(moniker))
            return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("commandName"));

          // Call into command handler to bind custom command
          if (Context.CommandHandler.AddCustomCommand(moniker, t))
            return new CommandResult(CommandStatus.Success, t.Name + " bound to " + moniker);
          else
            return new CommandResult(CommandStatus.Failure, "Failed to add command");
        }
        else
          return new CommandResult(CommandStatus.Failure, string.Format("Type {0} does not support interface {1}", t.Name, typeof(ICommand).Name));
      }
      else
        return new CommandResult(CommandStatus.Failure, "Failed to load type " + typeName);
    }
  }
}
