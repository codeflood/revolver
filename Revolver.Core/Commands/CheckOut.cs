
namespace Revolver.Core.Commands
{
  [Command("co")]
  public class CheckOut : BaseCommand
  {
    [NumberedParameter(0, "path")]
    [Description("The path of the item to check out. If not specified the current item is used")]
    [Optional]
    public string Path { get; set; }

    public CheckOut()
    {
      Path = string.Empty;
    }

    public override CommandResult Run()
    {
      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        var res = Context.CurrentItem.Locking.Lock();

        if (res)
          return new CommandResult(CommandStatus.Success, "'" + Context.CurrentItem.Name + "' checked out");
        else
          return new CommandResult(CommandStatus.Failure, "'" + Context.CurrentItem.Name + "' check out failed");
      }
    }

    public override string Description()
    {
      return "Check out a given item";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample(string.Empty);
      details.AddExample("/item1/item2");
    }
  }
}
