#if SC70
using Sitecore.Buckets.Util;
#endif
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data;
using Sitecore.StringExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("csearch")]
  public class ContentSearch : BaseCommand
  {
    [FlagParameter("ns")]
    [Description("No Statistics. Don't show how many items were found.")]
    [Optional]
    public bool NoStats { get; set; }
    
    [FlagParameter("so")]
    [Description("Statistics only. Only show the count of items that were found. No need to supply a command when using this flag.")]
    [Optional]
    public bool StatsOnly { get; set; }
    
    [FlagParameter("l")]
    [Description("Include all languages and not just the context language.")]
    [Optional]
    public bool AllLanguages { get; set; }
    
    [NamedParameter("i", "indexName")]
    [Description("The name of the index to search within. If ommitted, use an appropriate index.")]
    [Optional]
    public string IndexName { get; set; }

    [NumberedParameter(0, "query")]
    [Description("The content search query to execute.")]
    public string Query { get; set; }
    
    [NumberedParameter(1, "command")]
    [Description("The command to execute against each found item. Required when -so flag is not used.")]
    [NoSubstitutionAttribute]
    public string Command { get; set; }

    public ContentSearch()
    {
      NoStats = false;
      StatsOnly = false;
      AllLanguages = false;
      IndexName = string.Empty;
      Query = string.Empty;
      Command = string.Empty;
    }

    public override CommandResult Run()
    {
      if(string.IsNullOrEmpty(Query))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("query"));

      if(string.IsNullOrEmpty(Command) && !StatsOnly)
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("command") + " or -so flag");

      if (!string.IsNullOrEmpty(Command) && StatsOnly)
        return new CommandResult(CommandStatus.Failure, "Cannot specify a command when using the -so flag");

      ISearchIndex index = null;

      if (string.IsNullOrEmpty(IndexName))
        index = ContentSearchManager.GetIndex(new SitecoreIndexableItem(Context.CurrentItem));
      else
      {
        try
        {
          index = ContentSearchManager.GetIndex(IndexName);
        }
        catch (Exception)
        {
          index = null;
        }
      }

      if (index == null)
        return new CommandResult(CommandStatus.Failure, "Index not found");

      var buffer = new StringBuilder();
      var foundCount = 0;

      using (var searchContext = index.CreateSearchContext())
      {
        List<SearchStringModel> searchModel;

        try
        {
#if SC70
          searchModel = UIFilterHelpers.ExtractSearchQuery(Query);
#else
          searchModel = SearchStringModel.ExtractSearchQuery(Query);
#endif
        }
        catch (Exception ex)
        {
          return new CommandResult(CommandStatus.Failure, "Failed to parse search query: " + ex.GetType().Name + ": " + ex.Message);
        }

        var queryable = LinqHelper.CreateQuery(searchContext, searchModel);

        var results = queryable.GetResults<SitecoreUISearchResultItem>();
        var uris = from item in results.Hits
                   let itemUri = item.Document.Uri ?? new ItemUri(item.Document["_uniqueid"])
                   where (AllLanguages || itemUri.Language == Context.CurrentLanguage)
                   select itemUri;

        foreach (var uri in uris)
        {
          if (uri != null)
          {
            CommandResult contextres = Context.SetContext(uri.ItemID.ToString(), uri.DatabaseName, uri.Language, uri.Version.Number);
            if (contextres.Status != CommandStatus.Success)
              return contextres;

            if (!StatsOnly)
              buffer.Append(Context.ExecuteCommand(Command, Formatter));

            foundCount++;

            Context.Revert();
          }
        }
      }

      if (StatsOnly)
        buffer.Append(foundCount);

      if (!NoStats && !StatsOnly)
      {
        Formatter.PrintLine(string.Empty, buffer);
        buffer.Append(string.Format("Found {0} {1}", foundCount, (foundCount == 1 ? "item" : "items")));
      }

      return new CommandResult(CommandStatus.Success, buffer.ToString());
    }

    public override string Description()
    {
      return "Perform a search using content search and execute the given command on each item";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("_name:home pwd");
      details.AddExample("_path:{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9};-_name:home pwd");
      details.AddExample("sitecore_web_index (+_name:content;+_templatename:main section) (ga -a id)");
      details.AddExample("(\\-_name:content;+_templatename:main section) (ga -a id)");
      details.AddExample("(__smallcreateddate:[20120903 TO 20130917]) pwd");
      details.AddExample("-l text:someterm pwd");
      details.AddExample("-so sitecore_core_index _name:sitecore");
    }
  }
}