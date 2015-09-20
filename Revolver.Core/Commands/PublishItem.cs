using Sitecore;
using Sitecore.Publishing;
using System;
using System.Linq;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("publish")]
  public class PublishItem : BaseCommand
  {
    [FlagParameter("i")]
    [Description("Perform an incremental publish.")]
    [Optional]
    public bool IncrementalPublish { get; set; }

    [FlagParameter("s")]
    [Description("Perform a smart publish.")]
    [Optional]
    public bool SmartPublish { get; set; }

    [FlagParameter("f")]
    [Description("Perform a full site publish.")]
    [Optional]
    public bool FullPublish { get; set; }

    [FlagParameter("r")]
    [Description("Recursively publish tree.")]
    [Optional]
    public bool Recursive { get; set; }

    [FlagParameter("h")]
    [Description("Only output the handle of the publish job.")]
    [Optional]
    public bool OutputHandleOnly { get; set; }

    [FlagParameter("w")]
    [Description("Wait for the publish job to complete.")]
    [Optional]
    public bool WaitForCompletion { get; set; }

    [NamedParameter("t", "targets")]
    [Description("A pipe seperated list of names of the publishing targets to publish the item to.")]
    [Optional]
    public string Targets { get; set; }

    [NamedParameter("l", "languages")]
    [Description("A pipe separated list of languages to publish the item in.")]
    [Optional]
    public string Languages { get; set; }

    [NumberedParameter(0, "path")]
    [Description("The path of the item to publish.")]
    [Optional]
    public string Path { get; set; }

    public PublishItem()
    {
      IncrementalPublish = false;
      SmartPublish = false;
      Recursive = false;
      FullPublish = false;
      OutputHandleOnly = false;
      WaitForCompletion = false;
    }

    public override CommandResult Run()
    {
      var flagCount = 0;

      if (IncrementalPublish)
        flagCount++;

      if (SmartPublish)
        flagCount++;

      if (FullPublish)
        flagCount++;

      if (flagCount > 1)
        return new CommandResult(CommandStatus.Failure, "Only one of -i, -s or -f may be used");

      if (Recursive && (IncrementalPublish || SmartPublish || FullPublish))
        return new CommandResult(CommandStatus.Failure, "-r cannot be used with -i, -s or -f");

      var targetNames = (Targets ?? string.Empty).Split(new [] {'|'}, StringSplitOptions.RemoveEmptyEntries);
      var languageNames = (Languages ?? string.Empty).Split(new [] {'|'}, StringSplitOptions.RemoveEmptyEntries);

      using (var cs = new ContextSwitcher(Context, Path))
      {
        // Find targets for DB inside context switcher in case path changes the database

        var targets = (from ti in PublishManager.GetPublishingTargets(Context.CurrentDatabase)
                       let t = Sitecore.Configuration.Factory.GetDatabase(ti[Sitecore.FieldIDs.PublishingTargetDatabase])
                       where t != null && (targetNames.Length == 0 || targetNames.Contains(t.Name))
                       select t).ToArray();

        var languages = (from l in Context.CurrentItem.Languages
                         where languageNames.Length == 0 || languageNames.Contains(l.Name)
                         select l).ToArray();

        Handle publishHandle = null;

        if (IncrementalPublish)
          publishHandle = PublishManager.PublishIncremental(Context.CurrentDatabase, targets, languages);
        else if (SmartPublish)
          publishHandle = PublishManager.PublishSmart(Context.CurrentDatabase, targets, languages);
        else if (FullPublish)
          publishHandle = PublishManager.Republish(Context.CurrentDatabase, targets, languages);
        else
        {
          if (!Context.CurrentItem.Publishing.IsPublishable(DateTime.Now, true))
            return new CommandResult(CommandStatus.Failure, "Item or ancestor is not publishable");

          var provider = Context.CurrentDatabase.WorkflowProvider;
          if (provider != null)
          {
            var workflow = Context.CurrentDatabase.WorkflowProvider.GetWorkflow(Context.CurrentItem);
            if (workflow != null)
            {
              if (!workflow.IsApproved(Context.CurrentItem))
                return new CommandResult(CommandStatus.Failure, "Item is not approved in workflow");
            }
          }

          publishHandle = PublishManager.PublishItem(Context.CurrentItem, targets, languages, Recursive, true);
        }

        var message = publishHandle.ToString();

        if (WaitForCompletion)
        {
          PublishManager.WaitFor(publishHandle);

          if (!OutputHandleOnly)
            message = "Publish completed.";
        }
        else if(!OutputHandleOnly)
          message = "Publish started.";

        if(!OutputHandleOnly)
          message = message + " Publish job handle: '" + publishHandle.ToString() + "'";

        return new CommandResult(CommandStatus.Success, message);
      }
    }

    public override string Description()
    {
      return "Publish items or process the publish queue.";
    }

    public override void Help(HelpDetails details)
    {
      var comments = new StringBuilder();
      Formatter.PrintLine("If 'targets' or 'languages' is not supplied the item will be published to all.", comments);
      comments.Append("Only one of -i, -s, -f or -r may be used.");

      details.Comments = comments.ToString();

      details.AddExample("-t web");
      details.AddExample("-t web|live");
      details.AddExample("-l en|de");
      details.AddExample("-t (target 1|target 2) -l en");
      details.AddExample("-f -w");
    }
  }
}