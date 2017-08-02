using Sitecore.Data;
using Sitecore.Search;
using Sitecore.StringExtensions;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("search")]
  public class IndexSearch : BaseCommand
  {
    private const int MAX_SEARCH_HITS = 10000000;

    [FlagParameter("ns")]
    [Description("No Statistics. Don't show how many items were found.")]
    [Optional]
    public bool NoStats { get; set; }

    [FlagParameter("so")]
    [Description("Statistics only. Only show the count of items that were found. No need to supply a command when using this flag.")]
    [Optional]
    public bool StatsOnly { get; set; }

    [FlagParameter("v")]
    [Description("Include all versions and not just the latest.")]
    [Optional]
    public bool AllVersions { get; set; }

    [FlagParameter("l")]
    [Description("Include all languages and not just the context language.")]
    [Optional]
    public bool AllLanguages { get; set; }

    [NamedParameter("i", "indexName")]
    [Description("The name of the index to search within. If ommitted, the system index is searched.")]
    [Optional]
    public string IndexName { get; set; }

    [NumberedParameter(0, "query")]
    [Description("The lucene query to execute.")]
    [Optional]
    public string Query { get; set; }

    [NumberedParameter(1, "command")]
    [Description("The command to execute against each found item. Required when -so flag is not used.")]
    [Optional]
    [NoSubstitutionAttribute]
    public string Command { get; set; }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Query))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("query"));

      if (string.IsNullOrEmpty(Command) && !StatsOnly)
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("command") + " or -so flag");

      if (StatsOnly && !string.IsNullOrEmpty(Command))
        return new CommandResult(CommandStatus.Failure, "Cannot specify a command when using the -so flag");

      Index index = null;

      if (string.IsNullOrEmpty(IndexName))
        index = SearchManager.SystemIndex;
      else
        index = SearchManager.GetIndex(IndexName);

      if (index == null)
        return new CommandResult(CommandStatus.Failure, "Index not found");

      var output = new StringBuilder();
      var foundCount = 0;
      var searchCount = 0;

      using (var searchContext = index.CreateSearchContext())
      {
        var hits = searchContext.Search(Query, MAX_SEARCH_HITS);
        searchCount = hits.Length;

        foreach (var result in hits.FetchResults(0, MAX_SEARCH_HITS))
        {
          var itemUri = new ItemUri(result.Url);
          if (itemUri != null)
          {
            if (AllLanguages || itemUri.Language == Context.CurrentLanguage)
            {
              var contextres = Context.SetContext(itemUri.ItemID.ToString(), null, null, itemUri.Version.Number);
              if (contextres.Status != CommandStatus.Success)
                return contextres;

              if (AllVersions || Context.CurrentItem.Versions.GetLatestVersion().Version.Number == itemUri.Version.Number)
              {
                if (!StatsOnly)
                  output.Append(Context.ExecuteCommand(Command, Formatter));

                foundCount++;
              }

              Context.Revert();
            }
          }
        }
      }

      if (StatsOnly)
        output.Append(foundCount);

      if (!NoStats && !StatsOnly)
      {
        Formatter.PrintLine(string.Empty, output);
        output.Append(string.Format("Found {0} {1}", foundCount, (foundCount == 1 ? "item" : "items")));

        if (searchCount == MAX_SEARCH_HITS)
        {
          Formatter.PrintLine(string.Empty, output);
          output.Append("WARNING: Number of results equals the maximum search hit count. You may not have all the results.");
        }
      }

      return new CommandResult(CommandStatus.Success, output.ToString());
    }

    public override string Description()
    {
      return "Perform a search within a Lucene index and execute the given command on each item";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("(title:home) pwd");
      details.AddExample("web (title:home AND text:lorem) (ga -a id)");
      details.AddExample("(_created:[20120903 TO 20120917]) pwd");
      details.AddExample("-v -l text:someterm pwd");
      details.AddExample("-so core name:sitecore");
    }
  }
}