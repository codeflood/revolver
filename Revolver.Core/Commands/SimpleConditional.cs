using Sitecore.StringExtensions;

namespace Revolver.Core.Commands
{
  [Command("if")]
  public class SimpleConditional : BaseCommand
  {
    [NumberedParameter(0, "expression")]
    [Description("The expression to evaluate.")]
    [NoSubstitution]
    public string Expression { get; set; }

    [NumberedParameter(1, "command")]
    [Description("The command to execute.")]
    [NoSubstitution]
    public string Command { get; set; }

    public SimpleConditional()
    {
      Expression = string.Empty;
      Command = string.Empty;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Expression))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("expression"));

      if (string.IsNullOrEmpty(Command))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("command"));

      if (ExpressionParser.EvaluateExpression(Context, Expression))
      {
        return Context.ExecuteCommand(Command, Formatter);
      }

      return new CommandResult(CommandStatus.Success, string.Empty);
    }

    public override string Description()
    {
      return "Executes the given command if the given expression evaluates to true.";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments = "This command makes use of Revolver expressions. See the extended help topic 'expressions' for more detail (help expressions)";

      details.AddExample("($var$ > 15 as number) (echo is it larger)");
      details.AddExample("(@title ? company) (sf title (the new title))");
    }
  }
}