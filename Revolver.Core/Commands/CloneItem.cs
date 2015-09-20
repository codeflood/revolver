using Sitecore.Data.Items;

namespace Revolver.Core.Commands
{
  [Command("clone")]
  public class CloneItem : BaseCommand
  {
    [FlagParameter("s")]
    [Description("Single clone. Only clone the item, not it's descendants. Default is deep clone.")]
    [Optional]
    public bool SingleClone { get; set; }

    [FlagParameter("u")]
    [Description("Unclone. Unclones the item.")]
    [Optional]
    public bool Unclone { get; set; }

    [FlagParameter("a")]
    [Description("Accept changes from clone source.")]
    [Optional]
    public bool AcceptChanges { get; set; }

    [FlagParameter("r")]
    [Description("Reject changes from clone source.")]
    [Optional]
    public bool RejectChanges { get; set; }

    [NumberedParameter(0, "target")]
    [Description("The path to create the item clone in.")]
    [Optional]
    public string TargetPath { get; set; }

    [NumberedParameter(1, "path")]
    [Description("Path to the item to clone or unclone.")]
    [Optional]
    public string Path { get; set; }

    public CloneItem()
    {
      SingleClone = false;
      Unclone = false;
      AcceptChanges = false;
      RejectChanges = false;
      TargetPath = string.Empty;
      Path = string.Empty;
    }

    public override CommandResult Run()
    {
      if (AcceptChanges && RejectChanges)
        return new CommandResult(CommandStatus.Failure, "Only one of -a or -r can be used");

      if (Unclone)
      {
        using (new ContextSwitcher(Context, TargetPath))
        {
          if (Context.CurrentItem.IsClone)
          {
            new Sitecore.Data.Items.CloneItem(Context.CurrentItem).Unclone();
            return new CommandResult(CommandStatus.Success, "Uncloned item '" + Context.CurrentItem.Name + "'");
          }
          else
            return new CommandResult(CommandStatus.Failure, "'" + Context.CurrentItem.Name + "' is not an item clone");
        }
      }
      else if (RejectChanges)
      {
        ExecuteNotifications(Context.CurrentItem, false);
        return new CommandResult(CommandStatus.Success, "Changes rejected");
      }
      else if (AcceptChanges)
      {
        ExecuteNotifications(Context.CurrentItem, true);
        return new CommandResult(CommandStatus.Success, "Changes accepted");
      }
      else
      {
        var fullTargetPath = PathParser.EvaluatePath(Context, TargetPath);
        var targetItem = Context.CurrentDatabase.GetItem(fullTargetPath);
        if (targetItem == null)
          return new CommandResult(CommandStatus.Failure, "Failed to find target item");

        using (new ContextSwitcher(Context, Path))
        {
          var cloneItem = Context.CurrentItem.CloneTo(targetItem, !SingleClone);
          if(cloneItem != null)
            return new CommandResult(CommandStatus.Success, "Cloned item '" + Context.CurrentItem.Name + "'");
          else
            return new CommandResult(CommandStatus.Failure, "Failed to clone item '" + Context.CurrentItem.Name + "'");
        }
      }
    }

    /// <summary>
    /// Accept or reject notifications on the item
    /// </summary>
    /// <param name="item">The item to execute the notificaitons for</param>
    /// <param name="accept">True to accept notifications, false to reject</param>
    /// <returns>True if notifications were processed, otherwise false</returns>
    private bool ExecuteNotifications(Item item, bool accept)
    {
      if (item.Database.NotificationProvider != null && item.IsClone)
      {
        var notifications = item.Database.NotificationProvider.GetNotifications(item);
        foreach (var notification in notifications)
        {
          if (accept)
            notification.Accept(item);
          else
            notification.Reject(item);

          return true;
        }
      }

      return false;
    }

    public override string Description()
    {
      return "Manage item clones";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample(string.Empty);
      details.AddExample("myclone");
      details.AddExample("/sitecore/content/home/myclone");
      details.AddExample("-s myclone");
      details.AddExample("myclone ../a");
      details.AddExample("-u");
      details.AddExample("-u /sitecore/content/home/myclone");
      details.AddExample("-a");
      details.AddExample("-r /sitecore/content/home/myclone");
    }
  }
}
