namespace Revolver.Core.Commands
{
  [Command("ci")]
  public class CheckIn : BaseCommand
  {
    [NumberedParameter(0, "path")]
    [Description("The path of the item to check in. If not specified the current item is used")]
    [Optional]
    public string Path { get; set; }

    public CheckIn()
    {
      Path = string.Empty;
    }

    public override CommandResult Run()
    {
      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        var res = Context.CurrentItem.Locking.Unlock();

        if(res)
          return new CommandResult(CommandStatus.Success, "'" + Context.CurrentItem.Name + "' checked in");
        else
          return new CommandResult(CommandStatus.Failure, "'" + Context.CurrentItem.Name + "' check in failed");
      }
    }

    public override string Description()
    {
      return "Check in a given item";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample(string.Empty);
      details.AddExample("/item1/item2");
    }
  }
}
