using Sitecore.Data.Items;
using Sitecore.StringExtensions;
using System;

namespace Revolver.Core.Commands
{
  [Command("create")]
  public class CreateItem : BaseCommand
  {
    [NamedParameter("t", "template")]
    [Description("The template to create the item from. Can either be an ID or a path.")]
    [Optional]
    public string Template { get; set; }

    [NamedParameter("b", "branch")]
    [Description("The branch to create the item from. Can either be an ID or a name.")]
    [Optional]
    public string Branch { get; set; }

    [NamedParameter("x", "xml")]
    [Description("Sitecore item XML string.")]
    [Optional]
    public string Xml { get; set; }

    [FlagParameter("id")]
    [Description("Change IDs in the XML string.")]
    [Optional]
    public bool ChangeIds { get; set; }

    [FlagParameter("v")]
    [Description("Create a new numbered version of the item.")]
    [Optional]
    public bool NewVersion { get; set; }

    [NumberedParameter(0, "name")]
    [Description("The name of the new item to create.")]
    [Optional]
    public string Name { get; set; }

    [NumberedParameter(1, "path")]
    [Description("The path of the parent to create the item under. If not specified the current item is used.")]
    public string Path { get; set; }

    public override CommandResult Run()
    {
      if (NewVersion)
        return AddNewVersion();

      if (string.IsNullOrWhiteSpace(Xml))
      {
        if (string.IsNullOrWhiteSpace(Template) && string.IsNullOrWhiteSpace(Branch))
          return new CommandResult(CommandStatus.Failure, "Missing either 'template' or 'branch 'parameter.");
       
        if (string.IsNullOrWhiteSpace(Name))
          return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("name"));
      }

      Item created = null;

      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        // Resolve the temaplate or master
        Item constructor = null;

        if (!string.IsNullOrWhiteSpace(Template))
          constructor = Context.CurrentDatabase.Templates[Template];

        if (!string.IsNullOrWhiteSpace(Branch))
          constructor = Context.CurrentDatabase.Branches[Branch];

        if (!string.IsNullOrEmpty(Xml))
        {
          try
          {
            created = Context.CurrentItem.PasteItem(Xml, ChangeIds, PasteMode.Merge);
            if (created == null)
              return new CommandResult(CommandStatus.Failure, "Failed to create item from XML.");
          }
          catch (Exception ex)
          {
            return new CommandResult(CommandStatus.Failure, "Failed to create item from XML: " + ex.ToString());
          }
        }
        else
        {
          if (constructor == null)
          {
            return new CommandResult(CommandStatus.Failure, "Failed to find the template or branch.");
          }

          // create the item
          if (!string.IsNullOrWhiteSpace(Template))
            created = Context.CurrentItem.Add(Name, (TemplateItem)constructor);

          if (!string.IsNullOrWhiteSpace(Branch))
            created = Context.CurrentItem.Add(Name, (BranchItem)constructor);
        }
      }

      return new CommandResult(CommandStatus.Success, created.ID.ToString());
    }

    protected virtual CommandResult AddNewVersion()
    {
      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        Context.CurrentItem = Context.CurrentItem.Versions.AddVersion();
        return new CommandResult(CommandStatus.Success, "Added version " + Context.CurrentItem.Version.Number);
      }
    }

    public override string Description()
    {
      return "Create a new item";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments = "Either one of 'template', 'branch', 'xml' or -v argument must be provided. Name must be provided for template or branch.";
      details.AddExample("-t {493B3A83-0FA7-4484-8FC9-4680991CF743} newitem1");
      details.AddExample("-t Document newitem1");
      details.AddExample("-b Article newitem1");
      details.AddExample("-b Article newitem1 /item1");
      details.AddExample("-x <item xml data>");
      details.AddExample("-id -x <item xml data>");
      details.AddExample("-v");
    }
  }
}

