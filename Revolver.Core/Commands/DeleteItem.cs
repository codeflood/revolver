
namespace Revolver.Core.Commands
{
  [Command("rm")]
  public class DeleteItem : BaseCommand
  {

    [NumberedParameter(0, "path")]
    [Description("The path of the item to delete. If not specified the current item is used.")]
    [Optional]
    public string Path { get; set; }

    [FlagParameter("nr")]
    [Description("No recycle. Delete the item instead of recycling it.")]
    [Optional]
    public bool NoRecycle { get; set; }

    public DeleteItem()
    {
      Path = string.Empty;
      NoRecycle = false;
    }

    public override CommandResult Run()
    {
      var count = 0;
      string msg;

      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        var inspector = new ItemInspector(Context.CurrentItem);
        count = inspector.CountDescendants();

        var parent = Context.CurrentItem.Parent;

        if (NoRecycle)
        {
          Context.CurrentItem.Delete();
          msg = "Deleted";
        }
        else
        {
          Context.CurrentItem.Recycle();
          msg = "Recycled";
        }

        if (Path.Length == 0)
          Context.CurrentItem = parent;
      }

      return new CommandResult(CommandStatus.Success, string.Format(msg + " {0} {1}", count, count == 1 ? "item" : "items"));
    }

    public override string Description()
    {
      return "Delete an item including it's children";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("../item1");
      details.AddExample("-nr ../item1");
    }
  }
}
