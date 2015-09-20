using Sitecore.Data.Items;
using Sitecore.StringExtensions;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("sf")]
  public class SetField : BaseCommand
  {
    [FlagParameter("nv")]
    [Description("No versioning. Doesn't create a new version of the item before updating it.")]
    [Optional]
    public bool NoVersion { get; set; }

    [FlagParameter("ns")]
    [Description("No statistics. Don't update statistic fields automatically such as __updated.")]
    [Optional]
    public bool NoStats { get; set; }

    [FlagParameter("s")]
    [Description("Silent. Don't fire events.")]
    [Optional]
    public bool Silent { get; set; }

    [FlagParameter("r")]
    [Description("Reset the field so template standard values can be used.")]
    [Optional]
    public bool Reset { get; set; }

    [NumberedParameter(0, "field")]
    [Description("The name of the field set.")]
    public string Field { get; set; }

    [NumberedParameter(1, "value")]
    [Description("The value to set the field to. Can contain the '$prev' token to use the previous field value.")]
    public string Value { get; set; }

    [NumberedParameter(2, "path")]
    [Description("The path of the item to set the field on. If not specified the current item is used.")]
    [Optional]
    public string Path { get; set; }

    public SetField()
    {
      NoVersion = false;
      NoStats = false;
      Silent = false;
      Reset = false;
      Field = string.Empty;
      Value = string.Empty;
      Path = string.Empty;
    }

    public override CommandResult Run()
    {
      var output = new StringBuilder();

      // Get the field name
      if (string.IsNullOrEmpty(Field))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("field"));

      // Get the field value
      if (Value == null && !Reset)
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("value"));

      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        Item item = base.Context.CurrentItem;

        if (item.Fields[Field] != null)
        {
          // Check if we're using a replacement
          if (Value != null && Value.Contains("$prev"))
          {
            string previousValue = item.Fields[Field].Value;
            Value = Value.Replace("$prev", previousValue);
          }

          // Add a new version and move item to it
          if (!NoVersion)
          {
            item = item.Versions.AddVersion();
            base.Context.CurrentItem = item;
          }

          item.Editing.BeginEdit();
          if (Reset)
            item.Fields[Field].Reset();
          else
            item.Fields[Field].Value = Value;
          item.Editing.EndEdit(!NoStats, Silent);

          // Show new version
          Formatter.PrintLine("Version " + item.Version, output);

          // Show new field value
          var gf = new GetFields();
          gf.Initialise(base.Context, Formatter);
          gf.FieldName = Field;
          gf.Path = item.ID.ToString();
          
          var result = gf.Run();

          Formatter.PrintDefinition(Field, result.ToString(), output);
        }
        else
        {
          return new CommandResult(CommandStatus.Failure, "Failed to find field '" + Field + "'");
        }
      }

      return new CommandResult(CommandStatus.Success, output.ToString());
    }

    public override string Description()
    {
      return "Set a field on a given item.";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments = "Value should only be provided if -r is not used";

      details.AddExample("title hello");
      details.AddExample("title hello item1/item2");
      details.AddExample("body (some content) ../item1");
      details.AddExample("-ns -q category (some content)");
      details.AddExample("-r __renderings");
    }
  }
}
