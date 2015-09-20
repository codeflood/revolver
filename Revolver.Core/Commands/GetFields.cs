using Sitecore.Data.Fields;
using Sitecore.StringExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("gf")]
  public class GetFields : BaseCommand
  {
    [FlagParameter("nsv")]
    [Description("Retrieve the value directly from the item, ignoring standard values.")]
    [Optional]
    public bool NoStandardValues { get; set; }

    [NamedParameter("f", "field")]
    [Description("The name of the field to retrieve. If not specified all fields are retrieved.")]
    [Optional]
    public string FieldName { get; set; }

    [NumberedParameter(0, "path")]
    [Description("The path of the item to retrieve the fields from. If not specified the current item is used.")]
    [Optional]
    public string Path { get; set; }

    public GetFields()
    {
      NoStandardValues = false;
      FieldName = string.Empty;
      Path = string.Empty;
    }

    public override CommandResult Run()
    {
      var output = new StringBuilder();

      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        var item = Context.CurrentItem;

        if (!string.IsNullOrEmpty(FieldName))
        {
          var field = item.Fields[FieldName];

          if (field == null)
            return new CommandResult(CommandStatus.Failure, "Failed to find field '{0}'".FormatWith(FieldName));

          output.Append(field.GetValue(!NoStandardValues));
        }
        else
        {
          var fields = item.Fields.OrderBy(x => x.Name);

          foreach (Field field in fields)
          {
            string name = field.Name;

            if (string.IsNullOrEmpty(name))
            {
              var fieldItem = field.Database.GetItem(field.ID);
              if (fieldItem != null)
                name = fieldItem.Name;
            }

            Formatter.PrintDefinition(name, field.GetValue(!NoStandardValues), output);
          }
        }
      }

      return new CommandResult(CommandStatus.Success, output.ToString());
    }

    public override string Description()
    {
      return "Display fields for a given item";
    }

    public override void Help(HelpDetails details)
    {     
      details.AddExample("-f title item1/item2");
      details.AddExample("-f body ../item1");
      details.AddExample("-nsv -f title");
    }
  }
}
