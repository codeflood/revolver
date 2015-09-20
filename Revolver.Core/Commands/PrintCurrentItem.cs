namespace Revolver.Core.Commands
{
  [Command("pwd")]
  public class PrintCurrentItem : BaseCommand
  {
    [NumberedParameter(0, "path")]
    [Description("The path of the item to list the links for.")]
    [Optional]
    public string Path { get; set; }

    public PrintCurrentItem()
    {
      Path = string.Empty;
    }

    public override CommandResult Run()
    {
      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status == CommandStatus.Success)
          return new CommandResult(CommandStatus.Success, base.Context.CurrentItem.Paths.FullPath);
        else
          return new CommandResult(CommandStatus.Failure, "Failed to find item '" + Path + "'");
      }
    }

    public override string Description()
    {
      return "Print the current item path";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("{493B3A83-0FA7-4484-8FC9-4680991CF743}");
    }
  }
}
