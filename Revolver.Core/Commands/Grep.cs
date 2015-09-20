using Sitecore.StringExtensions;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Revolver.Core.Commands
{
  [Command("grep")]
  public class Grep : BaseCommand
  {
    [FlagParameter("c")]
    [Description("Perform case sensitive regular expression matching.")]
    [Optional]
    public bool CaseSensitive { get; set; }

    [FlagParameter("nm")]
    [Description("Show lines not matching the regular expression.")]
    [Optional]
    public bool NotMatching { get; set; }

    [NumberedParameter(0, "regex")]
    [Description("The regular expression to use for matching input lines.")]
    public string Regex { get; set; }

    [NumberedParameter(1, "input")]
    [Description("The input to match on.")]
    public string Input { get; set; }

    public Grep()
    {
      CaseSensitive = false;
      NotMatching = false;
      Regex = string.Empty;
      Input = string.Empty;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Regex))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("regex"));

      if (string.IsNullOrEmpty(Input))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("input"));

      var options = RegexOptions.Compiled;
      if (!CaseSensitive)
        options |= RegexOptions.IgnoreCase;

      var regex = new Regex(Regex, options);

      var lines = Formatter.SplitLines(Input);
      var outputLines = new List<string>();

      foreach (var line in lines)
      {
        var isMatch = regex.IsMatch(line);
        if (isMatch ^ NotMatching)
          outputLines.Add(line);
      }

      var output = new StringBuilder();
      outputLines.ForEach(x => Formatter.PrintLine(x, output));
      return new CommandResult(CommandStatus.Success, output.ToString());
    }

    public override string Description()
    {
      return "Match lines which match a given regular expression";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("(my item) < ls");
      details.AddExample("publishing < ps");
    }
  }
}
