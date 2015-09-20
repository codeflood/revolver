using Sitecore.StringExtensions;
using System;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("timer")]
  public class Timer : BaseCommand
  {
    [NumberedParameter(0, "command")]
    [Description("The command to execute.")]
    [NoSubstitutionAttribute]
    public string Command { get; set; }

    public Timer()
    {
      Command = string.Empty;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Command))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("command"));

      var output = new StringBuilder();
      var start = DateTime.Now;
      output.Append(Context.ExecuteCommand(Command, Formatter));
      var end = DateTime.Now;

      Formatter.PrintLine(string.Empty, output);
      Formatter.PrintLine("Start: " + start, output);
      Formatter.PrintLine("End: " + end, output);
      
      output.Append("Total: ");
      output.Append((end - start).ToString());

      return new CommandResult(CommandStatus.Success, output.ToString());
    }

    public override string Description()
    {
      return "Time how long a command taskes to run";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("(find -r pwd)");
    }
  }
}
