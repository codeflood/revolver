using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("set")]
  public class SetEnvironmentVariable : BaseCommand
  {
    [NumberedParameter(0, "variable")]
    [Description("The name of the variable to set. If ommitted all variables are displayed")]
    [Optional]
    public string Name { get; set; }

    [NumberedParameter(1, "value")]
    [Description("The value to set the variable to. If ommitted the variable is cleared. Can contain the '$prev' token to use the previous value.")]
    [Optional]
    public string Value { get; set; }

    public override CommandResult Run()
    {
      // Make sure we're not setting a reserved variable name
      if (Constants.ReservedVariables.Contains(Name) || Name == "prev")
        return new CommandResult(CommandStatus.Failure, "Cannot set a reserved variable");

      // Are we enumerating vars or setting / clearing them?
      if (string.IsNullOrEmpty(Name))
      {
        // Enumerate the variables
        var buffer = new StringBuilder();
        var entries = new DictionaryEntry[Context.EnvironmentVariables.Count];
        Context.EnvironmentVariables.CopyTo(entries, 0);
        
        Array.Sort<DictionaryEntry>(entries, delegate(DictionaryEntry a, DictionaryEntry b)
        {
          return string.Compare(a.Key.ToString(), b.Key.ToString());
        });

        foreach(var entry in entries)
        {
          Formatter.PrintDefinition(entry.Key.ToString(), entry.Value.ToString(), buffer);
        }

        buffer.AppendLine(string.Format("{0} variable{1} currently set", Context.EnvironmentVariables.Count, Context.EnvironmentVariables.Count == 1 ? string.Empty : "s"));

        return new CommandResult(CommandStatus.Success, buffer.ToString());
      }
      else
      {
        // Check if we have the current environment variable set
        // If the value is empty, clear the variable
        if(string.IsNullOrEmpty(Value))
        {
          if (Context.EnvironmentVariables.ContainsKey(Name))
          {
            Context.EnvironmentVariables.Remove(Name);
            return new CommandResult(CommandStatus.Success, string.Format("Cleared '{0}'", Name));
          }
          else
            return new CommandResult(CommandStatus.Success, string.Format("Variable '{0}' wasn't set", Name));
        }
        else
        {
          if (Value.Contains("$prev"))
            Value = Value.Replace("$prev", Context.EnvironmentVariables[Name]);

          Context.EnvironmentVariables[Name] = Value;
          return new CommandResult(CommandStatus.Success, Value);
        }
      }
    }

    public override string Description()
    {
      return "Set an environment variable or see all currently set variables";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("bler");
      details.AddExample("bler hello");
    }
  }
}
