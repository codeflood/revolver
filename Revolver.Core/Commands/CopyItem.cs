using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using Sitecore.StringExtensions;
using System.Text.RegularExpressions;

namespace Revolver.Core.Commands
{
  [Command("cp")]
  public class CopyItem : BaseCommand
  {
    [FlagParameter("r")]
    [Description("Recursive. Copy the source items children as well.")]
    [Optional]
    public bool Recursive { get; set; }

    [FlagParameter("n")]
    [Description("Assign a new ID to the target item when copying to a different database")]
    [Optional]
    public bool NewId { get; set; }

    [NumberedParameter(0, "targetPath")]
    [Description("The target path to copy the source item to, including the new name")]
    public string TargetPath { get; set; }

    [NumberedParameter(1, "sourcePath")]
    [Description("The path of the source item to copy. If not specified the current item is used.")]
    [Optional]
    public string SourcePath { get; set; }

    public CopyItem()
    {
      Recursive = false;
      NewId = false;
      TargetPath = string.Empty;
      SourcePath = string.Empty;
    }
    
    public override CommandResult Run()
    {
      if(string.IsNullOrEmpty(TargetPath))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("targetPath"));

      if (!string.IsNullOrEmpty(PathParser.ParseVersionFromPath(TargetPath)))
        return new CommandResult(CommandStatus.Failure, "Version in target path is not supported");

      if (!string.IsNullOrEmpty(PathParser.ParseVersionFromPath(SourcePath)))
        return new CommandResult(CommandStatus.Failure, "Version in source path is not supported");

      if (!string.IsNullOrEmpty(PathParser.ParseLanguageFromPath(TargetPath)))
        return new CommandResult(CommandStatus.Failure, "Language in target path is not supported");

      if (!string.IsNullOrEmpty(PathParser.ParseLanguageFromPath(SourcePath)))
        return new CommandResult(CommandStatus.Failure, "Language in source path is not supported");

      // Evaulate the target path
      var fullTargetPath = PathParser.EvaluatePath(Context, TargetPath);

      var count = 0;

      using (var cs = new ContextSwitcher(Context, SourcePath))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        // Parse out path and name from targetPath argument
        var tpPath = string.Empty;
        var tpName = string.Empty;

        Item parent = null;
        try
        {
          tpName = Context.CurrentItem.Name;
          var targetSwitchResult = Context.SetContext(fullTargetPath);
          if (targetSwitchResult.Status == CommandStatus.Success)
          {
            tpPath = fullTargetPath;
            Context.Revert();
          }
          else
            ParseTargetPath(fullTargetPath, out tpPath, out tpName);

          CommandResult contextres = Context.SetContext(tpPath);
          if (contextres.Status != CommandStatus.Success)
            return contextres;

          parent = Context.CurrentItem;
          if (parent == null)
            return new CommandResult(CommandStatus.Failure, "Failed to find the target path '" + tpPath + "'");
        }
        finally
        {
          Context.Revert();
        }

        // Now perform the copy
        Item copy = null;

        var sourceName = Context.CurrentItem.Name;
        if (parent.Database == Context.CurrentItem.Database)
          copy = Context.CurrentItem.CopyTo(parent, tpName, Sitecore.Data.ID.NewID, Recursive);
        else
        {
          // Check if target database contains an item with that ID
          var alreadyExists = parent.Database.GetItem(Context.CurrentItem.ID) != null;
          var useNewId = true;

          if (!NewId || alreadyExists)
            useNewId = false;

          var xml = Context.CurrentItem.GetOuterXml(Recursive);
          var regex = new Regex("name=\"[^\"]*\"");
          xml = regex.Replace(xml, "name=\"" + tpName + "\"");
          parent.Paste(xml, useNewId, PasteMode.Overwrite);
          copy = parent.Children[tpName];
        }

        count = CountChildren(copy);

        // If item is a media item copy the blob
        CopyMediaBlobs(Context.CurrentItem, copy, Recursive);
      }

      return new CommandResult(CommandStatus.Success, string.Format("Copied {0} item{1}", count, count == 1 ? string.Empty : "s"));
    }

    /// <summary>
    /// Copy media blobs of this and optionally descendant items
    /// </summary>
    /// <param name="mediaItem">The media item to copy from</param>
    /// <param name="recursive">If true copy all descendant item blobs as well</param>
    private void CopyMediaBlobs(MediaItem source, MediaItem target, bool recursive)
    {
      if (target.InnerItem["blob"] != string.Empty)
      {
        var targetMedia = MediaManager.GetMedia(target);
        targetMedia.SetStream(MediaManager.GetMedia(source).GetStream());
      }

      if (recursive)
      {
        foreach (Item child in source.InnerItem.GetChildren())
        {
          var targetItem = target.InnerItem.Children[child.Name];
          if (targetItem != null)
          {
            MediaItem nextSource = null;
            MediaItem nextTarget = null;

            try
            {
              nextSource = (MediaItem)child;
              nextTarget = (MediaItem)targetItem;
            }
            catch
            {
              // Swallow exception as the item isn't a media item. Probably a folder
            }

            if (nextSource != null && nextTarget != null)
              CopyMediaBlobs(nextSource, nextTarget, recursive);
          }
        }
      }
    }

    /// <summary>
    /// Count the number of items below the given item including the item itself
    /// </summary>
    /// <param name="item">The item to start counting children at</param>
    /// <returns>The number of items found</returns>
    private int CountChildren(Item item)
    {
      int count = 1;
      if (item.HasChildren)
      {
        for (int i = 0; i < item.Children.Count; i++)
        {
          count += CountChildren(item.Children[i]);
        }
      }

      return count;
    }

    /// <summary>
    /// Parse the name and path out of a full path string
    /// </summary>
    /// <param name="targetPath">The path to parse</param>
    /// <param name="path">The output path</param>
    /// <param name="name">The output name</param>
    private void ParseTargetPath(string targetPath, out string path, out string name)
    {
      // Grab the final slash character
      var ind = targetPath.LastIndexOf('/');
      path = targetPath.Substring(0, ind);

      if (targetPath.Length > ind + 1)
        name = targetPath.Substring(ind + 1);
      else
        name = string.Empty;
    }

    public override string Description()
    {
      return "Copy an item with or without it's children";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments = "If an item doesn't exist in the target database then the ID will not be changed unless the -n flag is passed.";
      details.AddExample("../folder/newitem");
      details.AddExample("../folder/newitem item2");
      details.AddExample("-r ../folder");
    }
  }
}
