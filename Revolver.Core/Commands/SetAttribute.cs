using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.StringExtensions;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("sa")]
  public class SetAttribute : BaseCommand
  {
    [NumberedParameter(0, "attribute")]
    [Description("The name of the attribute to set. Must be one of name, template.")]
    public string Attribute { get; set; }

    [NumberedParameter(1, "value")]
    [Description("The value to set the attribute to. Can contain the '$prev' token to use the previous attribute value.")]
    public string Value { get; set; }

    [NumberedParameter(2, "path")]
    [Description("Optional. The path of the item to set the field on. If not specified the current item is used.")]
    [Optional]
    public string Path { get; set; }

    public SetAttribute()
    {
      Attribute = string.Empty;
      Value = string.Empty;
      Path = string.Empty;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Attribute))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("attribute"));

      if (string.IsNullOrEmpty(Value))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("value"));

      // resolve template
      TemplateItem template = null;

      if (Attribute == "template")
      {
        template = Context.CurrentDatabase.GetItem(Value);
        if (template == null)
        {
          if (ID.IsID(Attribute))
            return new CommandResult(CommandStatus.Failure, "Failed to find template with ID '" + Attribute + "'");
          else
            return new CommandResult(CommandStatus.Failure, "Failed to find template '" + Attribute + "'");
        }
      }

      string toRet = string.Empty;

      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        string query = Attribute;

        Item item = Context.CurrentItem;
        item.Editing.BeginEdit();

        switch (Attribute)
        {
          case "name":
            Value = Value.Replace("$prev", item.Name);
            item.Name = Value;
            break;

          case "template":
            item.TemplateID = template.ID;
            if (Value.StartsWith("{"))
              query += "id";
            break;

          default:
            return new CommandResult(CommandStatus.Failure, "Unknown attribute " + Attribute);
        }

        item.Editing.EndEdit();

        GetAttributes ga = new GetAttributes();
        ga.Initialise(Context, Formatter);
        ga.Attribute = query;
        CommandResult result = ga.Run();
        StringBuilder sb = new StringBuilder(200);
        Formatter.PrintDefinition(query, result.ToString(), sb);
        toRet = sb.ToString();
      }

      return new CommandResult(CommandStatus.Success, toRet);
    }

    public override string Description()
    {
      return "Set attributes for a given item";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("name (new name)");
      details.AddExample("name (new $prev) ../../a");
      details.AddExample("template {76036F5E-CBCE-46D1-AF0A-4143F9B557AA}");
    }
  }
}
