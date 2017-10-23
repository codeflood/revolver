using Sitecore.StringExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("split")]
  public class SplitString : BaseCommand
  {
    [FlagParameter("ns")]
    [Description("No Statistics. Don't show how many items were found.")]
    [Optional]
    public bool NoStatistics { get; set; }

    [FlagParameter("n")]
    [Description("Split on newline character.")]
    [Optional]
    public bool SplitOnNewLine { get; set; }

    [FlagParameter("t")]
    [Description("Split on tab character.")]
    [Optional]
    public bool SplitOnTab { get; set; }

    [NamedParameter("s", "symbol")]
    [Description("The symbol to split on.")]
    [Optional]
    public string SplitSymbol { get; set; }

    [NumberedParameter(0, "input")]
    [Description("The input string to split.")]
    public string Input { get; set; }

    [NumberedParameter(1, "command")]
    [Description("The command to execute against each element. The environment variable 'current' contains the current string.")]
    [Optional]
    [NoSubstitutionAttribute]
    public string Command { get; set; }

    public SplitString()
    {
      NoStatistics = false;
      SplitOnNewLine = false;
      SplitOnTab = false;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Input))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("input"));

      if (string.IsNullOrEmpty(Command))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("command"));

      // Create the split token array
      var tokens = new List<string>();

      if (SplitOnNewLine)
        tokens.Add("\r\n");

      if (SplitOnTab)
        tokens.Add("\t");

      if (!string.IsNullOrEmpty(SplitSymbol))
        tokens.Add(SplitSymbol);

      // Split the input string
      var elements = Input.Split(tokens.ToArray(), StringSplitOptions.RemoveEmptyEntries);
      var output = new StringBuilder();

      if (Context.EnvironmentVariables.ContainsKey("current"))
        output.AppendLine("WARNING: Environment variable 'current' contains a value. It has been overwritten.");

      // Process command against each string element
      Context.EnvironmentVariables.Remove("current");
      
      foreach(var element in elements)
      {
        Context.EnvironmentVariables.Add("current", element);
        output.Append(Context.ExecuteCommand(Command, Formatter));
        Formatter.PrintLine(string.Empty, output);
        Context.EnvironmentVariables.Remove("current");
      }

      if (!NoStatistics)
        output.Append(string.Format("Processed {0} {1}", elements.Length, (elements.Length == 1 ? "string" : "strings")));

      return new CommandResult(CommandStatus.Success, output.ToString());
    }

    public override string Description()
    {
      return "Split a string and iterate over the results";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("-n < (echo -i -f file.txt) (echo $current$)");
      details.AddExample("-s , 1,2,3,4 (create -t document $current$)");
      details.AddExample("-s | < (gf -f multilist) (ga -a name $current$)");
    }
  }
}
