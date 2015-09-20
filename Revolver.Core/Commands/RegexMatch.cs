using Sitecore.StringExtensions;
using System.Text.RegularExpressions;

namespace Revolver.Core.Commands
{
  [Command("regex")]
  public class RegexMatch : BaseCommand
  {
    [FlagParameter("c")]
    [Description("Case Sensitive. Perform a case sensitive match.")]
    [Optional]
    public bool CaseSensitive { get; set; }

    [NumberedParameter(0, "input")]
    [Description("The input to match within.")]
    public string Input { get; set; }

    [NumberedParameter(1, "regex")]
    [Description("A regular expression used to extract text from input.")]
    public string Regex { get; set; }

    public RegexMatch()
    {
      CaseSensitive = false;
      Input = string.Empty;
      Regex = string.Empty;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Regex))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("regex"));

      var options = RegexOptions.Compiled;
      if (!CaseSensitive)
        options |= RegexOptions.IgnoreCase;

      var regex = new Regex(Regex, options);
      return new CommandResult(CommandStatus.Success, regex.Match(Input).Value);
    }

    public override string Description()
    {
      return "Extract matches from a string using regular expressions";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("(item number 27) (\\d+)");
      details.AddExample("(aitem bitem citem) ((a|b)item)");
      details.AddExample("-c (Lorem ipsum lorem) lorem");
    }
  }
}