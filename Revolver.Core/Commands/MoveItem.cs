using Sitecore.Data.Items;
using Sitecore.StringExtensions;

namespace Revolver.Core.Commands
{
  [Command("mv")]
  public class MoveItem : BaseCommand
  {
    [NumberedParameter(0, "targetPath")]
    [Description("The target path to move the source item to.")]
    public string TargetPath { get; set; }

    [NumberedParameter(1, "path")]
    [Description("The path of the source item to move. If not specified the current item is used.")]
    [Optional]
    public string Path { get; set; }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(TargetPath))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("targetPath"));

      // Evaulate the target path
      var evalTargetPath = PathParser.EvaluatePath(Context, TargetPath);
      Item parent = null;

      using (var targetSwitcher = new ContextSwitcher(Context, evalTargetPath))
      {
        if (targetSwitcher.Result.Status != CommandStatus.Success)
          return targetSwitcher.Result;

        parent = Context.CurrentItem;
        if (parent == null)
          return new CommandResult(CommandStatus.Failure, "Failed to find the target path '" + TargetPath + "'");
      }

      int count = 0;
      Item movedItem = null;

      using (var sourceSwitcher = new ContextSwitcher(Context, Path))
      {
        if (sourceSwitcher.Result.Status != CommandStatus.Success)
          return sourceSwitcher.Result;

        // Now perform the move
        string sourceName = Context.CurrentItem.Name;
        if (parent.Database == Context.CurrentItem.Database)
          Context.CurrentItem.MoveTo(parent);
        else
        {
          string xml = Context.CurrentItem.GetOuterXml(true);
          parent.Paste(xml, false, PasteMode.Overwrite);

          // Now the item has been copied, delete the original

          var deleteCommand = new DeleteItem();
          deleteCommand.Initialise(Context, Formatter);
          deleteCommand.Path = Context.CurrentItem.Paths.FullPath;
          deleteCommand.Run();
        }

        movedItem = parent.Children[Context.CurrentItem.ID];

        if(movedItem == null)
          movedItem = parent.Children[sourceName];

        if (movedItem != null)
        {
          var inspector = new ItemInspector(movedItem);
          count = inspector.CountDescendants();
        }
      }

      if (movedItem == null)
        return new CommandResult(CommandStatus.Failure, string.Format("Failed to move item"));

      if (Context.CurrentItem == null)
        Context.CurrentItem = movedItem;

      return new CommandResult(CommandStatus.Success, string.Format("Moved {0} {1}", count, count == 1 ? "item" : "items"));
    }

    public override string Description()
    {
      return "Move an item including it's children";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("../folder");
      details.AddExample("../folder item2");
    }
  }
}
