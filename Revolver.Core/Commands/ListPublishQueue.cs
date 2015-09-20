using Sitecore.StringExtensions;
using System;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("lsq")]
  public class ListPublishQueue : BaseCommand
  {
    [NumberedParameter(0, "from")]
    [Description("The start date to report from")]
    [Optional]
    public DateTime FromDate { get; set; }

    [NumberedParameter(1, "to")]
    [Description("The end date to report until")]
    [Optional]
    public DateTime ToDate { get; set; }

    [NamedParameter("d", "database")]
    [Description("The name of the database to get the last publish date for")]
    [Optional]
    public string DatabaseName { get; set; }

    [FlagParameter("id")]
    [Description("Only outputs the IDs of the items")]
    [Optional]
    public bool IdOnly { get; set; }

    [FlagParameter("ns")]
    [Description("No Statistics. Do not display the number of items shown")]
    [Optional]
    public bool NoStats { get; set; }

    public ListPublishQueue()
    {
      FromDate = DateTime.MinValue;
      ToDate = DateTime.Now;
    }

    public override CommandResult Run()
    {
      var startDate = FromDate;

      if (!string.IsNullOrEmpty(DatabaseName))
      {
        try
        {
          var targetDatabase = Sitecore.Configuration.Factory.GetDatabase(DatabaseName);
          if (targetDatabase == null)
            return new CommandResult(CommandStatus.Failure, "Failed to locate database '{0}'".FormatWith(DatabaseName));

          startDate = Context.CurrentDatabase.Properties.GetLastPublishDate(targetDatabase, Context.CurrentLanguage);
        }
        catch (Exception ex)
        {
          return new CommandResult(CommandStatus.Failure, "Failed to locate database '{0}'".FormatWith(DatabaseName));
        }
      }

      var queue = Sitecore.Publishing.PublishManager.GetPublishQueue(startDate, ToDate, Context.CurrentDatabase);
      var buffer = new StringBuilder();

      foreach (var id in queue)
      {
        var path = "not found";
        var item = Context.CurrentDatabase.GetItem(id);
        if (item != null)
          path = item.Paths.FullPath;

        if (IdOnly)
          buffer.AppendLine(id.ToString());
        else
          Formatter.PrintDefinition(id.ToString(), path, 40, buffer);
      }

      if (!NoStats)
      {
        Formatter.PrintLine(string.Empty, buffer);
        buffer.Append("{0} item{1} found".FormatWith(queue.Count, queue.Count == 1 ? string.Empty : "s"));
      }

      return new CommandResult(CommandStatus.Success, buffer.ToString());
    }

    public override string Description()
    {
      return "Lists items in the publish queue";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments = "If database is specified the date range will be from the latest publish time to the database.";

      details.AddExample("2013-11-18");
      details.AddExample("2013-11-18 (2014-01-12 11:00am)");
      details.AddExample("-d web");
      details.AddExample("-d web -id -ns");
    }
  }
}