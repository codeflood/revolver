namespace Revolver.Core.Commands
{
  [Command("pwv")]
  public class PrintCurrentVersion : BaseCommand
  {
    // todo: review tests
    public override CommandResult Run()
    {
      return new CommandResult(CommandStatus.Success, "Version " + Context.CurrentItem.Version.Number.ToString());
    }

    public override string Description()
    {
      return "Print the current version";
    }

    public override void Help(HelpDetails details)
    {
    }
  }
}