using Sitecore.StringExtensions;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("rep")]
  public class Repeat : BaseCommand
  {
    [NumberedParameter(0, "number")]
    [Description("The number of times to repeat the command.")]
    public string Number { get; set; }

    [NumberedParameter(1, "command")]
    [Description("The command to execute multiple times.")]
    [NoSubstitutionAttribute]
    public string Command { get; set; }

    [NumberedParameter(2, "path")]
    [Description("The path of the item to execute this command against.")]
    [Optional]
    public string Path { get; set; }

    public Repeat()
    {
      Number = string.Empty;
      Command = string.Empty;
      Path = string.Empty;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Number))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("number"));

      if (string.IsNullOrEmpty(Command))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("command"));

      int num = 0;
      if (!int.TryParse(Number, out num))
        return new CommandResult(CommandStatus.Failure, "Parameter 'number' must be a positive integer");

      if (num < 0)
        return new CommandResult(CommandStatus.Failure, "Parameter 'number' must be a positive integer");

      var output = new StringBuilder();

      using (new ContextSwitcher(Context, Path))
      {
        Context.EnvironmentVariables.Remove("num");
        for (int i = 0; i < num; i++)
        {
          Context.EnvironmentVariables.Add("num", (i + 1).ToString());
          output.Append(Context.ExecuteCommand(Command, Formatter));
          Formatter.PrintLine(string.Empty, output);
          Context.EnvironmentVariables.Remove("num");
        }
      }

      return new CommandResult(CommandStatus.Success, output.ToString());
    }

    public override string Description()
    {
      return "Repeat a command";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments = "On each repetition the $num$ environment variable contains the current count of runs";
      details.AddExample("3 (create -t (sample/sample item) $num$)");
      details.AddExample("5 (echo $num$) /sitecore/content/home");
    }
  }
}
