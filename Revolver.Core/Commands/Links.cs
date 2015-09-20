using Sitecore.Data.Items;
using Sitecore.Links;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("links")]
  public class Links : BaseCommand
  {
    private readonly int[] DISPLAY_COLUMN_WIDTHS = new int[] { 11, 8, 20, 10 };
    private const string VALID_LABEL = "yes";
    private const string INVALID_LABEL = "no";
    private const string INCOMING_LABEL = "in";
    private const string OUTGOING_LABEL = "out";
    private const string UNKNOWN_LABEL = "<unknown>";

    [FlagParameter("i")]
    [Description("Show incoming links (referrers).")]
    [Optional]
    public bool ShowIncomingLinks { get; set; }

    [FlagParameter("o")]
    [Description("Show outgoing links (references).")]
    [Optional]
    public bool ShowOutgoingLinks { get; set; }

    [FlagParameter("b")]
    [Description("Show invalid links.")]
    [Optional]
    public bool ShowBadLinks { get; set; }

    [FlagParameter("id")]
    [Description("Output links as an id list.")]
    [Optional]
    public bool OutputIds { get; set; }

    [NamedParameter("f", "field")]
    [Description("The source field the link must originate from.")]
    [Optional]
    public string OriginFieldName { get; set; }

    [NumberedParameter(0, "path")]
    [Description("The path of the item to list the links for.")]
    [Optional]
    public string Path { get; set; }

    public override CommandResult Run()
    {
      var showAll = !ShowBadLinks && !ShowIncomingLinks && !ShowOutgoingLinks;

      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        var linkDb = Sitecore.Configuration.Factory.GetLinkDatabase();
        if (linkDb == null)
          return new CommandResult(CommandStatus.Failure, "Failed to find the link database");

        var output = new StringBuilder();

        if (!OutputIds)
        {
          Formatter.PrintTable(new[] { "Direction", "Valid", "Field", "Link" }, DISPLAY_COLUMN_WIDTHS, output);
          Formatter.PrintTable(new[] { "---------", "-----", "-----", "----" }, DISPLAY_COLUMN_WIDTHS, output);
          Formatter.PrintLine(string.Empty, output);
        }

        if (ShowIncomingLinks || showAll)
          ProcessIncomingLinks(linkDb.GetReferrers(Context.CurrentItem), output);

        if (ShowOutgoingLinks || showAll)
          ProcessOutgoingLinks(linkDb.GetReferences(Context.CurrentItem), output);

        if (ShowIncomingLinks || showAll)
          ProcessBadLinks(linkDb.GetBrokenLinks(Context.CurrentDatabase), output);

        return new CommandResult(CommandStatus.Success, output.ToString());
      }
    }

    private void ProcessIncomingLinks(ItemLink[] links, StringBuilder output)
    {
      var currentItem = Context.CurrentItem;
      for (int i = 0; i < links.Length; i++)
      {
        if (links[i].TargetItemID == currentItem.ID)
        {
          var fieldItem = Context.CurrentDatabase.GetItem(links[i].SourceFieldID);
          var sourceItem = links[i].GetSourceItem();
          if (sourceItem != null)
          {
            if (string.IsNullOrEmpty(OriginFieldName) || (fieldItem != null && fieldItem.Key == OriginFieldName.ToLower()))
            {
              if (OutputIds)
              {
                if (output.Length > 0)
                  output.Append("|");

                output.Append(sourceItem.ID.ToString());
              }
              else
              {
                if (fieldItem != null)
                  Formatter.PrintTable(new[] { INCOMING_LABEL, VALID_LABEL, fieldItem.Name, sourceItem.Paths.FullPath }, DISPLAY_COLUMN_WIDTHS, output);
                else
                  Formatter.PrintTable(new[] { INCOMING_LABEL, VALID_LABEL, UNKNOWN_LABEL, sourceItem.Paths.FullPath }, DISPLAY_COLUMN_WIDTHS, output);
              }
            }
          }
          else if (!OutputIds)
            Formatter.PrintTable(new string[] { INCOMING_LABEL, INVALID_LABEL, UNKNOWN_LABEL, UNKNOWN_LABEL }, DISPLAY_COLUMN_WIDTHS, output);
        }
      }
    }

    private void ProcessOutgoingLinks(ItemLink[] links, StringBuilder output)
    {
      var currentItem = Context.CurrentItem;
      for (int i = 0; i < links.Length; i++)
      {
        if (links[i].SourceItemID == currentItem.ID)
        {
          var fieldItem = Context.CurrentDatabase.GetItem(links[i].SourceFieldID);
          var targetItem = links[i].GetTargetItem();
          if (targetItem != null)
          {
            if (string.IsNullOrEmpty(OriginFieldName) || (fieldItem != null && fieldItem.Key == OriginFieldName.ToLower()))
            {
              if (OutputIds)
              {
                if (output.Length > 0)
                  output.Append("|");

                output.Append(targetItem.ID.ToString());
              }
              else
              {
                if (fieldItem != null)
                  Formatter.PrintTable(new string[] { OUTGOING_LABEL, VALID_LABEL, fieldItem.Name, targetItem.Paths.FullPath }, DISPLAY_COLUMN_WIDTHS, output);
                else
                  Formatter.PrintTable(new string[] { OUTGOING_LABEL, VALID_LABEL, UNKNOWN_LABEL, targetItem.Paths.FullPath }, DISPLAY_COLUMN_WIDTHS, output);
              }
            }
          }
          else if (!OutputIds)
            Formatter.PrintTable(new string[] { OUTGOING_LABEL, INVALID_LABEL, UNKNOWN_LABEL, UNKNOWN_LABEL }, DISPLAY_COLUMN_WIDTHS, output);
        }
      }
    }

    private void ProcessBadLinks(ItemLink[] links, StringBuilder output)
    {
      var currentItem = Context.CurrentItem;
      for (int i = 0; i < links.Length; i++)
      {
        if (links[i].SourceItemID == currentItem.ID)
        {
          Item sourceFieldItem = Context.CurrentDatabase.GetItem(links[i].SourceFieldID);
          if (string.IsNullOrEmpty(OriginFieldName) || (sourceFieldItem != null && sourceFieldItem.Key == OriginFieldName.ToLower()))
          {
            if (OutputIds)
            {
              if (output.Length > 0)
                output.Append("|");

              output.Append(links[i].TargetItemID.ToString());
            }
            else
            {
              var fieldItem = Context.CurrentDatabase.GetItem(links[i].SourceFieldID);
              if (fieldItem != null)
                Formatter.PrintTable(new string[] { OUTGOING_LABEL, INVALID_LABEL, fieldItem.Name, links[i].TargetPath }, DISPLAY_COLUMN_WIDTHS, output);
              else
                Formatter.PrintTable(new string[] { OUTGOING_LABEL, INVALID_LABEL, UNKNOWN_LABEL, links[i].TargetPath }, DISPLAY_COLUMN_WIDTHS, output);
            }
          }
        }
      }
    }

    public override string Description()
    {
      return "List referrers and references from the link database";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("-o ..\a");
      details.AddExample("-i -o ..\a");
      details.AddExample("-b");
      details.AddExample("-o -id");
      details.AddExample("-i -f __source");
    }
  }
}
