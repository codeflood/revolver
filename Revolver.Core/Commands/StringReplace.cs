using Sitecore.StringExtensions;
using System;
using System.Text.RegularExpressions;

namespace Revolver.Core.Commands
{
  [Command("replace")]
  public class StringReplace : BaseCommand
  {
    [NumberedParameter(0, "input")]
    [Description("The string input to perform replacement on.")]
    public string Input { get; set; }

    [NumberedParameter(1, "regexMatch")]
    [Description("Regular expression to match text to replace.")]
    public string RegexMatch { get; set; }

    [NumberedParameter(2, "regexReplace")]
    [Description("Regular expression to replace matched text.")]
    public string RegexReplace { get; set; }

    [FlagParameter("c")]
    [Description("Perform case sensitive regular expression matching.")]
    [Optional]
    public bool CaseSensitiveRegex { get; set; }

    public StringReplace()
    {
      Input = string.Empty;
      RegexMatch = string.Empty;
      RegexReplace = string.Empty;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Input))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("input"));

      if (string.IsNullOrEmpty(RegexMatch))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("regexMatch"));

      if (RegexReplace == null)
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("regexReplace"));

      var options = RegexOptions.Compiled;
      if (!CaseSensitiveRegex)
        options |= RegexOptions.IgnoreCase;

      try
      {
        var regex = new Regex(RegexMatch, options);
        return new CommandResult(CommandStatus.Success, regex.Replace(Input, RegexReplace));
      }
      catch(ArgumentException ex)
      {
        return new CommandResult(CommandStatus.Failure, ex.Message);
      }
    }

    public override string Description()
    {
      return "Replace text in string input using regular expressions";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("(this is input) (^is$) (id)");
      details.AddExample("(this is input) (^(is)$) ($1id)");
      details.AddExample("(this is input) \\s -");
      details.AddExample("(This Is Input) is in -c");
    }
  }
}
