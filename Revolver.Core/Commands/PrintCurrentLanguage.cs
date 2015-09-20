namespace Revolver.Core.Commands
{
  [Command("pwl")]
  public class PrintCurrentLanguage : BaseCommand
  {
    //todo: review tests
    public override CommandResult Run()
    {
      return new CommandResult(CommandStatus.Success, Context.CurrentLanguage.CultureInfo.DisplayName + " [" + Context.CurrentLanguage.CultureInfo.Name + "]");
    }

    public override string Description()
    {
      return "Print the current language";
    }

    public override void Help(HelpDetails details)
    {
    }
  }
}
