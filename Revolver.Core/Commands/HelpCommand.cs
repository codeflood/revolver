using Revolver.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("help")]
  public class HelpCommand : BaseCommand
  {
    private Dictionary<string, MethodInfo> _exhelp;

    [NumberedParameter(0, "command")]
    [Optional]
    [Description("The name of the command or script to get help for")]
    public string CommandName { get; set; }

    public HelpCommand()
    {
      _exhelp = GetExtendedHelp();
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(CommandName))
      {
        var output = ListCommands();
        return new CommandResult(CommandStatus.Success, output);
      }
      else if (Context.CommandHandler.CoreCommands.ContainsKey(CommandName))
      {
        var cmd = (ICommand)Activator.CreateInstance(Context.CommandHandler.CoreCommands[CommandName]);
        cmd.Initialise(Context, Formatter);

        var details = CreateHelpForCommand(cmd);
        cmd.Help(details);
        return new CommandResult(CommandStatus.Success, Formatter.PrintHelp(details, CommandName));
      }
      else if (Context.CommandHandler.FindCommandAlias(CommandName) != null)
      {
        var alias = Context.CommandHandler.FindCommandAlias(CommandName);
        var cmd = (ICommand)Activator.CreateInstance(Context.CommandHandler.CoreCommands[alias.CommandName]);
        cmd.Initialise(Context, Formatter);

        var details = CreateHelpForCommand(cmd);
        cmd.Help(details);
        return new CommandResult(CommandStatus.Success, Formatter.PrintHelp(details, CommandName));
      }
      else if (_exhelp.ContainsKey(CommandName))
      {
        var details = (HelpDetails)_exhelp[CommandName].Invoke(null, null);
        return new CommandResult(CommandStatus.Success, Formatter.PrintHelp(details));
      }
      else if (Context.CommandHandler.CustomCommands.ContainsKey(CommandName))
      {
        var cmd = (ICommand)Activator.CreateInstance(Context.CommandHandler.CustomCommands[CommandName]);
        cmd.Initialise(Context, Formatter);
        var details = CreateHelpForCommand(cmd);
        cmd.Help(details);
        return new CommandResult(CommandStatus.Success, Formatter.PrintHelp(details, CommandName));
      }
      else
      {
        try
        {
          var details = Context.CommandHandler.ScriptLocator.GetScriptHelp(CommandName);
          if (details != null)          
            return new CommandResult(CommandStatus.Success, Formatter.PrintHelp(details));
        }
        catch (MultipleScriptsFoundException ex)
        {
          var message = new StringBuilder();
          foreach (var scriptName in ex.Names)
          {
            Formatter.PrintLine(scriptName, message);
          }

          message.Append("Multiple scripts found by name '");
          message.Append(CommandName);
          Formatter.PrintLine("'", message);
          message.Append("Script names must be unique");

          return new CommandResult(CommandStatus.Failure, message.ToString());
        }
      }

      return new CommandResult(CommandStatus.Failure, "Unknown command or script name " + CommandName);
    }

    public override string Description()
    {
      return "List available commands and provide detailed help information about them";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample(string.Empty);
      details.AddExample("ls");
      details.AddExample("ls-scripts");
    }

    protected string ListCommands()
    {
      var sb = new StringBuilder();

      Formatter.PrintLine("Available core commands:", sb);
      Formatter.PrintLine(new string('-', 50), sb);
      IEnumerable<KeyValuePair<string, Type>> com = Context.CommandHandler.CoreCommands.OrderBy(a => a.Key);
      foreach (KeyValuePair<string, Type> c in com)
      {
        var cmd = (ICommand)Activator.CreateInstance(c.Value);
        Formatter.PrintDefinition(c.Key, cmd.Description(), sb);
      }

      if (Context.CommandHandler.CustomCommands.Count > 0)
      {
        Formatter.PrintLine(string.Empty, sb);
        Formatter.PrintLine("Available custom commands:", sb);
        Formatter.PrintLine(new string('-', 50), sb);
        IEnumerable<KeyValuePair<string, Type>> ccom = Context.CommandHandler.CustomCommands.OrderBy(a => a.Key);
        foreach (KeyValuePair<string, Type> cc in ccom)
        {
          var cmd = (ICommand)Activator.CreateInstance(cc.Value);
          Formatter.PrintDefinition(cc.Key, cmd.Description(), sb);
        }
      }
      Formatter.PrintLine(string.Empty, sb);
      Formatter.PrintLine("help <cmd> for command specific help", sb);
      Formatter.PrintLine(string.Empty, sb);
      
      sb.Append("Other help topics: ");
      var e = _exhelp.Keys.GetEnumerator();
      while (e.MoveNext())
      {
        sb.Append((string)e.Current);
        sb.Append(", ");
      }

      if (sb.ToString().EndsWith(", "))
        sb.Remove(sb.Length - 2, 2);

      return sb.ToString();
    }

    private Dictionary<string, MethodInfo> GetExtendedHelp()
    {
      Dictionary<string, MethodInfo> toRet = new Dictionary<string, MethodInfo>(10);
      toRet.Add("expressions", typeof(ExtendedHelp).GetMethod("Expressions"));
      toRet.Add("prompt", typeof(ExtendedHelp).GetMethod("Prompt"));
      toRet.Add("subcommand", typeof(ExtendedHelp).GetMethod("SubCommand"));
      toRet.Add("useenvironmentvariable", typeof(ExtendedHelp).GetMethod("UseEnvironmentVariable"));

      // todo: Add help for functions

      return toRet;
    }

    private HelpDetails CreateHelpForCommand(ICommand command)
    {
      var type = command.GetType();

      var details = new HelpDetails
      {
        Description = command.Description(),
      };

      var usage = new StringBuilder();

      var commandAttr = CommandInspector.GetCommandAttribute(type);
      if (commandAttr != null)
        usage.Append(commandAttr.Binding);

      var properties = type.GetProperties();

      foreach (var prop in properties)
      {
        usage.Append(" ");

        var isOptionalProperty = CommandInspector.GetOptionalParameter(prop) != null;
        var descriptionAttribute = CommandInspector.GetDescriptionAttribute(prop);

        var flagParameterAttribute = CommandInspector.GetFlagParameter(prop);
        if (flagParameterAttribute != null)
        {
          FormatParameterForUsage(usage, "-" + flagParameterAttribute.Name, isOptionalProperty);
          AddParameterToHelp(details, "-" + flagParameterAttribute.Name, descriptionAttribute, isOptionalProperty);
        }

        var namedParameterAttribute = CommandInspector.GetNamedParameter(prop);
        if (namedParameterAttribute != null)
        {
          FormatParameterForUsage(usage, "-" + namedParameterAttribute.Name + " " + namedParameterAttribute.HelpValuePlaceholder, isOptionalProperty);
          AddParameterToHelp(details, "-" + namedParameterAttribute.Name, descriptionAttribute, isOptionalProperty);
        }

        var numberedParameterAttribute = CommandInspector.GetNumberedParameter(prop);
        if (numberedParameterAttribute != null)
        {
          FormatParameterForUsage(usage, numberedParameterAttribute.HelpValuePlaceholder, isOptionalProperty);
          AddParameterToHelp(details, numberedParameterAttribute.HelpValuePlaceholder, descriptionAttribute, isOptionalProperty);
        }

        var listParameterAttribute = CommandInspector.GetListParameter(prop);
        if (listParameterAttribute != null)
        {
          FormatParameterForUsage(usage, listParameterAttribute.HelpValuePlaceholder, isOptionalProperty);
          AddParameterToHelp(details, listParameterAttribute.HelpValuePlaceholder, descriptionAttribute, isOptionalProperty);
        }
      }

      details.Usage = usage.ToString();

      return details;
    }

    private void AddParameterToHelp(HelpDetails details, string name, DescriptionAttribute description, bool optional)
    {
      details.AddParameter(name, (optional ? "Optional. " : string.Empty) + (description != null ? description.Description : string.Empty));
    }

    private void FormatParameterForUsage(StringBuilder buffer, string name, bool optional)
    {
      if (optional)
        buffer.Append("[");

      buffer.Append(name);

      if (optional)
        buffer.Append("]");
    }
  }
}