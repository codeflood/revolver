using Sitecore.StringExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Lucene.Net.Search.Function;

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

      // If we're missing the assembly delimiter, treat the typeName as an assemblyName
      if (!typeName.Contains(","))
        return BindCommandsFromAssembly(typeName);

      return BindCommandFromType(typeName, moniker);
    }

    /// <summary>
    /// Bind a single command from a type name.
    /// </summary>
    /// <param name="typeName">The name of the type implementing the command.</param>
    /// <param name="moniker">The name of the command to bind to. If empty, use the default command name from the command attribute on the type.</param>
    /// <returns>A result indicating whether the type was bound successfully.</returns>
    private CommandResult BindCommandFromType(string typeName, string moniker)
    {
      // Try to load the type
      var type = Type.GetType(typeName, false, true);
      if (type == null)
        return new CommandResult(CommandStatus.Failure, "Failed to load type " + typeName);

      return BindCommandFromType(type, moniker);
    }

    /// <summary>
    /// Bind a single command from a type name.
    /// </summary>
    /// <param name="typeName">The name of the type implementing the command.</param>
    /// <param name="moniker">The name of the command to bind to. If empty, use the default command name from the command attribute on the type.</param>
    /// <returns>A result indicating whether the type was bound successfully.</returns>
    private CommandResult BindCommandFromType(Type type, string moniker)
    { 
      // Verify it implements the correct interface
      if (type.GetInterface(typeof(ICommand).Name) != null)
      {
        if (string.IsNullOrEmpty(moniker))
        {
          // If no moniker is provided, grab the moniker from the command attribute
          var commandAttr = CommandInspector.GetCommandAttribute(type);
          if (commandAttr != null)
            moniker = commandAttr.Binding;
          else
            return new CommandResult(CommandStatus.Failure, string.Format("Type {0} does not have a command attribute. You must specify the commandName", type.Name));
        }

        if (string.IsNullOrEmpty(moniker))
          return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("commandName"));

        // Call into command handler to bind custom command
        if (Context.CommandHandler.AddCustomCommand(moniker, type))
          return new CommandResult(CommandStatus.Success, type.Name + " bound to " + moniker);
          
        return new CommandResult(CommandStatus.Failure, "Failed to add command");
      }
        
      return new CommandResult(CommandStatus.Failure, string.Format("Type {0} does not support interface {1}", type.Name, typeof(ICommand).Name));
    }

    /// <summary>
    /// Bind all commands from an assembly.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to load the commands from.</param>
    /// <returns>A result indicating whether at least 1 command was found and whether all found commands where bound successfully.</returns>
    private CommandResult BindCommandsFromAssembly(string assemblyName)
    {
      Assembly assembly = null;

      try
      {
        assembly = Assembly.Load(assemblyName);
      }
      catch (FileNotFoundException)
      {
        return new CommandResult(CommandStatus.Failure, "Failed to load assembly " + assemblyName);
      }

      if(assembly == null)
        return new CommandResult(CommandStatus.Failure, "Failed to load assembly " + assemblyName);

      var commands = new Dictionary<string, Type>();
      CommandInspector.FindAllCommands(commands, assembly);

      var results = new List<CommandResult>();

      foreach (var command in commands)
      {
        var result = BindCommandFromType(command.Value, command.Key);
        results.Add(result);
      }

      var message = Formatter.JoinLines(results.Select(x => x.Message));
      var status = results.All(x => x.Status == CommandStatus.Success) ? CommandStatus.Success : CommandStatus.Failure;

      return new CommandResult(status, message);
    }
  }
}
