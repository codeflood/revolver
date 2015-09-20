using Revolver.Core.Commands.Parameters;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.StringExtensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace Revolver.Core.Commands
{
  [Command("find")]
  public class FindItems : BaseCommand
  {
    protected const string WILDCARD_FIELDNAME = "*";
    protected Regex _fieldRegex = null;
    protected Regex _attributeRegex = null;

    [FlagParameter("r")]
    [Description("Recursive. Search all descendants as well.")]
    [Optional]
    public bool Recursive { get; set; }

    [FlagParameter("ns")]
    [Description("No statistics. Don't show how many items were found.")]
    [Optional]
    public bool NoStatistics { get; set; }

    [FlagParameter("so")]
    [Description("Statistics only. Only show the count of items that were found. No need to supply a command when using this flag.")]
    [Optional]
    public bool StatisticsOnly { get; set; }

    [FlagParameter("nsv")]
    [Description("Retrieve the field value directly from the item, ignoring standard values.")]
    [Optional]
    public bool NoStandardValues { get; set; }

    [NamedParameter("i", "idlist")]
    [Description("Pipe seperated list of item IDs to find.")]
    [Optional]
    public string Ids { get; set; }

    [NamedParameter("t", "template")]
    [Description("The template to match items on. Can either be an ID or a path.")]
    [Optional]
    public string Template { get; set; }

    [NamedParameter("b", "branch")]
    [Description("The branch to match items on. Can either be an ID or a name.")]
    [Optional]
    public string Branch { get; set; }

    [NamedParameter("f", "field value", 2)]
    [Description("The field to match items on. Use " + WILDCARD_FIELDNAME + " to search in all fields")]
    [Optional]
    [TypeConverter(typeof(KeyValueConverter))]
    public KeyValuePair<string, string> FindByField { get; set; }

    [FlagParameter("fc")]
    [Description("Perform case sensitive field regular expression match.")]
    [Optional]
    public bool FindByFieldCaseSensitive { get; set; }

    [NamedParameter("e", "expression")]
    [Description("A comparative expression to evaluate.")]
    [Optional]
    public string Expression { get; set; }

    [NamedParameter("a", "attribute value", 2)]
    [Description("The attribute to match items on.")]
    [Optional]
    [TypeConverter(typeof(KeyValueConverter))]
    public KeyValuePair<string, string> FindByAttribute { get; set; }

    [FlagParameter("ac")]
    [Description("Perform case sensitive attribute regular expression match.")]
    [Optional]
    public bool FindByAttributeCaseSensitive { get; set; }

    [NumberedParameter(0, "command")]
    [Description("Required when -so flag is not used. The command to execute against each matching item.")]
    [Optional]
    [NoSubstitutionAttribute]
    public string Command { get; set; }

    [NumberedParameter(1, "path")]
    [Description("The path of the item to execute this command from. If not specified the current item is used.")]
    [Optional]
    public string Path { get; set; }
    
    public FindItems()
    {
      Recursive = false;
      NoStatistics = false;
      StatisticsOnly = false;
      NoStandardValues = false;
      Ids = string.Empty;
      Template = string.Empty;
      Branch = string.Empty;
      FindByFieldCaseSensitive = false;
      Expression = string.Empty;
      FindByAttributeCaseSensitive = false;
      Command = string.Empty;
      Path = string.Empty;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Command) && !StatisticsOnly)
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("command") + " or -so flag.");

      if (!string.IsNullOrEmpty(Command) && StatisticsOnly)
        return new CommandResult(CommandStatus.Failure, "Cannot specify a command when returning statistics only.");

      // Make sure we have the right parameters filled in
      if (string.IsNullOrEmpty(FindByField.Key) ^ string.IsNullOrEmpty(FindByField.Value))
        return new CommandResult(CommandStatus.Failure, "Field name or value is missing.");

      // Reset regexs so they get rebuilt if required, based on current parameters
      _fieldRegex = null;
      _attributeRegex = null;

      // Convert template and branch arguments to IDs
      var templateId = ID.Null;
      if(!string.IsNullOrEmpty(Template))
      {
        var templateItem = Context.CurrentDatabase.Templates[Template];
        if (templateItem == null)
          return new CommandResult(CommandStatus.Failure, "Faild to find template '{0}'".FormatWith(Template));

        templateId = templateItem.ID;
      }

      var branchId = ID.Null;
      if (!string.IsNullOrEmpty(Branch))
      {
        var branchItem = Context.CurrentDatabase.Branches[Branch];
        if (branchItem == null)
          return new CommandResult(CommandStatus.Failure, "Faild to find branch '{0}'".FormatWith(Branch));

        branchId = branchItem.ID;
      }

      var output = new StringBuilder();

      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        var items = new ItemCollection();
        if (!string.IsNullOrEmpty(Ids))
        {
          foreach (var id in ID.ParseArray(Ids))
          {
            var item = Context.CurrentDatabase.GetItem(id);
            if (item != null)
            {
              if (IncludeItem(item, templateId, branchId))
                items.Add(item);
            }
            else
              return new CommandResult(CommandStatus.Failure, "Failed to find item with id '{0}'".FormatWith(id));
          }
        }
        else
          ProcessItem(items, Context.CurrentItem, templateId, branchId, true);

        // Return number of matched items if only returning stats
        if (StatisticsOnly)
          output.Append(items.Count);
        else
        {
          // Execute command against each found item
          var limit = items.Count;
          for (var i = 0; i < limit; i++)
          {
            var item = items[i];
            using (new ContextSwitcher(Context, item.ID.ToString()))
            {
              var result = Context.ExecuteCommand(Command, Formatter);

              // Should we check the status? Maybe abort if stoponerror is set?

              output.Append(result);
            }

            if(i < limit - 1)
              Formatter.PrintLine(string.Empty, output);
          }

          if (!NoStatistics)
          {
            Formatter.PrintLine(string.Empty, output);
            Formatter.PrintLine(string.Empty, output);
            output.Append(string.Format("Found {0} {1}", items.Count, (items.Count == 1 ? "item" : "items")));
          }
        }
      }

      return new CommandResult(CommandStatus.Success, output.ToString());
    }

    /// <summary>
    /// Adds items to the collection based on the search criteria
    /// </summary>
    /// <param name="items">The collection to add items to</param>
    /// <param name="current">The current item</param>
    /// <param name="templateId">Template filter</param>
    /// <param name="branchId">Branch filter</param>
    /// <param name="start">Indicates if the item is the start item</param>
    protected virtual void ProcessItem(ItemCollection items, Item current, ID templateId, ID branchId, bool start)
    {
      if (!start)
      {
        var include = IncludeItem(current, templateId, branchId);

        if (include)
          items.Add(current);
      }

      if (Recursive || start)
      {
        // process each child
        ChildList children = current.GetChildren();
        for (int i = 0; i < children.Count; i++)
        {
          ProcessItem(items, children[i], templateId, branchId, false);
        }
      }
    }

    /// <summary>
    /// Checks to see if an item passes the filters
    /// </summary>
    /// <param name="item">The item to check</param>
    /// <param name="templateId">Template filter</param>
    /// <param name="branchId">Branch filter</param>
    /// <returns></returns>
    protected virtual bool IncludeItem(Item item, ID templateId, ID branchId)
    {
      var include = FilterByTemplate(item, templateId);

      if (include)
        include = FilterByBranch(item, branchId);

      if (include)
        include = FilterByField(item);

      if (include)
        include = FilterByAttribute(item);

      if (include)
        include = FilterByExpression(item);

      return include;
    }

    protected virtual bool FilterByTemplate(Item item, ID templateId)
    {
      if (templateId == ID.Null)
        return true;

      return item.TemplateID == templateId;
    }

    protected virtual bool FilterByBranch(Item item, ID branchId)
    {
      if (branchId == ID.Null)
        return true;

      return item.BranchId == branchId;
    }

    protected virtual bool FilterByField(Item item)
    {
      if (string.IsNullOrEmpty(FindByField.Key) || string.IsNullOrEmpty(FindByField.Value))
        return true;

      if (item.Fields[FindByField.Key] == null && FindByField.Key != WILDCARD_FIELDNAME)
        return false;

      if (_fieldRegex == null)
      {
        var options = RegexOptions.Compiled;
        if (!FindByFieldCaseSensitive)
          options |= RegexOptions.IgnoreCase;

        _fieldRegex = new Regex(FindByField.Value, options);
      }

      if (FindByField.Key == WILDCARD_FIELDNAME)
      {
        foreach (Field singleField in item.Fields)
        {
          if (_fieldRegex.IsMatch(singleField.GetValue(!NoStandardValues)))
          {
            return true;
          }
        }

        return false;
      }

      return _fieldRegex.IsMatch(item.Fields[FindByField.Key].GetValue(!NoStandardValues));
    }

    protected virtual bool FilterByAttribute(Item item)
    {
      if (string.IsNullOrEmpty(FindByAttribute.Key) || string.IsNullOrEmpty(FindByAttribute.Value))
        return true;

      if (_attributeRegex == null)
      {
        var options = RegexOptions.Compiled;
        if (!FindByAttributeCaseSensitive)
          options |= RegexOptions.IgnoreCase;

        _attributeRegex = new Regex(FindByAttribute.Value, options);
      }

      var attrcmd = new GetAttributes();
      attrcmd.Initialise(Context, Formatter);
      attrcmd.Attribute = FindByAttribute.Key;
      attrcmd.Path = item.ID.ToString();

      var result = attrcmd.Run();
      if (result.Status == CommandStatus.Success)
      {
        var output = result.Message;
        return _attributeRegex.IsMatch(output);
      }

      return false;
    }

    protected virtual bool FilterByExpression(Item item)
    {
      if (string.IsNullOrEmpty(Expression))
        return true;

      var context = Context.Clone();
      context.CurrentItem = item;
      return ExpressionParser.EvaluateExpression(context, Expression);
    }

    public override string Description()
    {
      return "Locate a set of items through content tree traversal and execute the given command against each item.";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("-i {493B3A83-0FA7-4484-8FC9-4680991CF743} pwd");
      details.AddExample("-i {493B3A83-0FA7-4484-8FC9-4680991CF743}|{543B3A83-0FA7-4484-8FC9-4680991CF797} pwd");
      details.AddExample("-t {493B3A83-0FA7-4484-8FC9-4680991CF743} (sf title hi)");
      details.AddExample("-t Document (sf title hi)");
      details.AddExample("-b MyBranch (sf title hi)");
      details.AddExample("-f title (a page for .*) (sf title hi)");
      details.AddExample("-f * sitecore pwd");
      details.AddExample("-f title (a page for .*) -fc (sf title hi)");
      details.AddExample("-r -f title (a page for .*) (sf title hi) /sitecore/content/home");
      details.AddExample("-t {493B3A83-0FA7-4484-8FC9-4680991CF743} -f title hi (sf title welcome)");
      details.AddExample("-ns -e (@__created > 3/1/2007 as date) pwd");
      details.AddExample("-so -e (@__created > 3/1/2007 as date)");
    }
  }
}
