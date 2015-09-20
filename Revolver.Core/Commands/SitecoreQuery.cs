using Sitecore.Data.Items;
using Sitecore.StringExtensions;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("query")]
  public class SitecoreQuery : BaseCommand
  {
    [FlagParameter("ns")]
    [Description("No Statistics. Don't show how many items were found.")]
    [Optional]
    public bool NoStatistics { get; set; }

    [FlagParameter("so")]
    [Description("Statistics only. Only show the count of items that were found. No need to supply a command when using this flag.")]
    [Optional]
    public bool StatisticsOnly { get; set; }

    [NumberedParameter(0, "query")]
    [Description("The sitecore query to execute.")]
    public string Query { get; set; }

    [NumberedParameter(1, "command")]
    [Description("Required when -so flag is not used. The command to run against each item.")]
    [Optional]
    [NoSubstitutionAttribute]
    public string Command { get; set; }

    [NumberedParameter(2, "path")]
    [Description("The path of the item to execute the command from. If not specified the current item is used.")]
    [Optional]
    public string Path { get; set; }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Query))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("query"));

      if (string.IsNullOrEmpty(Command) && !StatisticsOnly)
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("command") + " or -so flag");

      if (!string.IsNullOrEmpty(Command) && StatisticsOnly)
        return new CommandResult(CommandStatus.Failure, "Cannot specify a command when returning statistics only");

      var isFastQuery = Query.ToLower().StartsWith(Constants.FastQueryQualifier);

      if (isFastQuery && !string.IsNullOrEmpty(Path))
        return new CommandResult(CommandStatus.Failure, "Cannot specify a path for fast query");

      var output = new StringBuilder();

      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        Item[] items = null;

        if (isFastQuery)
          items = Context.CurrentDatabase.SelectItems(Query);
        else
          items = Context.CurrentItem.Axes.SelectItems(Query);

        if (StatisticsOnly)
          output.Append(items != null ? items.Length : 0);
        else
        {
          if (items != null)
          {
            for(var i = 0; i  < items.Length; i++)
            {
              var item = items[i];
              using (new ContextSwitcher(Context, item.ID.ToString()))
              {
                output.Append(Context.ExecuteCommand(Command, Formatter));
              }

              if (i < items.Length - 1)
                Formatter.PrintLine(string.Empty, output);
            }

            if (!NoStatistics)
            {
              Formatter.PrintLine(string.Empty, output);
              Formatter.PrintLine(string.Empty, output);
              output.Append(string.Format("Found {0} {1}", items.Length, (items.Length == 1 ? "item" : "items")));

              if (items.Length == Sitecore.Configuration.Settings.Query.MaxItems)
              {
                Formatter.PrintLine(string.Empty, output);
                output.Append("WARNING: Number of results equals the maximum query items length. You may not have all the results. Try increasing the value of the Query.MaxItems setting in web.config");
              }
            }
          }
          else if (!NoStatistics)
            output.Append("Found 0 items");
        }
      }

      return new CommandResult(CommandStatus.Success, output.ToString());
    }

    public override string Description()
    {
      return "Execute a sitecore query";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("/sitecore/content/home/* pwd");
      details.AddExample("/sitecore/content/home//news pwd");
      details.AddExample("//news pwd ..");
      details.AddExample("/sitecore/content/home/*[@nav = 'left'] (sf nav main)");
      details.AddExample("-so //news");
    }
  }
}
