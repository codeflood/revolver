using Sitecore.Collections;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Revolver.Core.Commands
{
  [Command("ls")]
  public class List : BaseCommand
  {
    public List()
    {
      Regex = string.Empty;
      Path = string.Empty;
    }

    /// <summary>
    /// Gets or sets a regex each item must match to be included in the output
    /// </summary>
    [NamedParameter("r", "regex")]
    [Description("Use regular expression matching for item names.")]
    [Optional]
    public string Regex { get; set; }

    /// <summary>
    /// Gets or sets whether the regex should be executed as case sensitive
    /// </summary>
    [FlagParameter("c")]
    [Description("Perform case sensitive regular expression matching.")]
    [Optional]
    public bool CaseSensitiveRegex { get; set; }

    /// <summary>
    /// Gets or sets whether the output should be listed alphabetically
    /// </summary>
    [FlagParameter("a")]
    [Description("List items in alphabetical order.")]
    [Optional]
    public bool Alphabetical { get; set; }

    /// <summary>
    /// Gets or sets whether the output should be listed in reverse order
    /// </summary>
    [FlagParameter("d")]
    [Description("List items in reverse order.")]
    [Optional]
    public bool ReverseOrder { get; set; }

    /// <summary>
    /// Gets or sets the path to execute the command at
    /// </summary>
    [NumberedParameter(0, "path")]
    [Description("The path of the item to list. Path can either be relative or absolute. If no path is specified the current item is used.")]
    [Optional]
    public string Path { get; set; }

    public override CommandResult Run()
    {
      var output = new StringBuilder();

      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        var item = Context.CurrentItem;

        if (item.HasChildren)
        {
          var children = item.GetChildren(ChildListOptions.None);
          var includeItems = new List<Item>(children.Count);
          var options = RegexOptions.Compiled;
          if (!CaseSensitiveRegex)
            options |= RegexOptions.IgnoreCase;

          var regex = new Regex(Regex, options);

          for (int i = 0; i < children.Count; i++)
          {
            if (Regex != string.Empty)
              if (!regex.IsMatch(children[i].Name))
                continue;

            includeItems.Add(children[i]);
          }

          var items = includeItems.ToArray();

          if (Alphabetical)
            Array.Sort(items, delegate(Item x, Item y)
            {
              return string.Compare(x.Name, y.Name);
            });

          if (ReverseOrder)
            Array.Reverse(items);

          for (int i = 0; i < items.Length; i++)
          {
            if (items[i].HasChildren)
              output.Append("+ ");
            else
              output.Append("  ");

            output.Append(items[i].Name);

            if (i < (items.Length - 1))
              Formatter.PrintLine(string.Empty, output);
          }
        }
        else
        {
          output.Append("zero items found");
        }
      }

      return new CommandResult(CommandStatus.Success, output.ToString());
    }

    public override string Description()
    {
      return "List children of the current item";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample(string.Empty);
      details.AddExample("item1/item2");
      details.AddExample("../item1");
      details.AddExample("-r .+name.+");
      details.AddExample("-r .+name.+ -c");
      details.AddExample("-a -d");
    }
  }
}
