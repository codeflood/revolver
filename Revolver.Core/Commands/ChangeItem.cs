using Sitecore.Data.Items;
using Sitecore.StringExtensions;

namespace Revolver.Core.Commands
{
  [Command("cd")]
  public class ChangeItem : BaseCommand
  {
    [NumberedParameter(0, "(path or ID)")]
    [Description("The path or ID of the item to change to. Path can either be relative or absolute.")]
    public string PathOrID { get; set; }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(PathOrID))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("path or ID"));

      var prev = "/" + Context.CurrentDatabase.Name + Context.CurrentItem.Paths.FullPath;
#if NET35
      var next = string.Join(" ", new string[] { PathOrID }).Trim();
#else
      var next = string.Join(" ", PathOrID).Trim();
#endif

      var res = Context.SetContext(next);

      if (res.Status == CommandStatus.Success)
      {
        Context.EnvironmentVariables["prevpath"] = prev;
        return new CommandResult(CommandStatus.Success, base.Context.CurrentItem.Paths.FullPath);
      }
      else
      {
        // no item by that name exists. Look for child starting with the input
        var children = Context.CurrentItem.GetChildren();
        Item match = null;
        var comparitor = next.ToLower();

        foreach (Item child in children)
        {
          if ((child != null) && (child.Name.ToLower().StartsWith(comparitor)))
          {
            match = child;
            break;
          }
        }

        if (match != null)
        {
          Context.EnvironmentVariables["prevpath"] = prev;
          Context.CurrentItem = match;
          return new CommandResult(CommandStatus.Success, base.Context.CurrentItem.Paths.FullPath);
        }
        else
        {
          return new CommandResult(CommandStatus.Failure, "Failed to find item '" + next + "'");
        }
      }
    }

    public override string Description()
    {
      return "Change the context item";
    }

    public override void Help(HelpDetails details)
    {     
      details.Comments = "If using a path, zero based indexes can be used for instances of items having the same name. Indexes can also be used without names to select children by index. Brackets are not required to escape paths with spaces in item names";
      details.AddExample("item1");
      details.AddExample("item1/item2");
      details.AddExample("../item1");
      details.AddExample("{4763CBB5-54A2-4D50-BF41-64AA4DA881A5}");
      details.AddExample("samename[2]");
      details.AddExample("[3]");
      details.AddExample("/path/to my/ item");
    }
  }
}
