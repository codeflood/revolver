namespace Revolver.Core.Commands
{
  [Command("pwdb")]
  public class PrintCurrentDatabase : BaseCommand
  {
    public override CommandResult Run()
    {
      return new CommandResult(CommandStatus.Success, base.Context.CurrentDatabase.Name);
    }

    public override string Description()
    {
      return "Print the current database";
    }

    public override void Help(HelpDetails details)
    {
    }
  }
}
