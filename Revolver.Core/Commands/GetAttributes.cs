using System;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("ga")]
  public class GetAttributes : BaseCommand
  {
    [NamedParameter("a", "attribute")]
    [Description("The attribute name to retrieve. If not specified all attributes are retrieved. Must be one of id, name, key, template, templateid, branch, branchid, language, version.")]
    [Optional]
    public string Attribute { get; set; }

    [NumberedParameter(0, "path")]
    [Description("The path of the item to retrieve the attribute from. If not specified the current item is used.")]
    [Optional]
    public string Path { get; set; }

    public GetAttributes()
    {
      Attribute = string.Empty;
      Path = string.Empty;
    }

    public override CommandResult Run()
    {
      var output = new StringBuilder();

      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        var inspector = new ItemInspector(Context.CurrentItem);

        if (Attribute.Length > 0)
        {
          var value = inspector.GetItemAttribute(Attribute);
          if (value == null)
            return new CommandResult(CommandStatus.Failure, "Failed to find attribute " + Attribute);

          output.Append(value);
        }
        else
        {
          Formatter.PrintDefinition("id", inspector.GetItemAttribute("id") ?? Constants.NotDefinedLiteral, output);
          Formatter.PrintDefinition("name", inspector.GetItemAttribute("name") ?? Constants.NotDefinedLiteral, output);
          Formatter.PrintDefinition("key", inspector.GetItemAttribute("key") ?? Constants.NotDefinedLiteral, output);
          Formatter.PrintDefinition("template", inspector.GetItemAttribute("template") ?? Constants.NotDefinedLiteral, output);
          Formatter.PrintDefinition("templateid", inspector.GetItemAttribute("templateid") ?? Constants.NotDefinedLiteral, output);
          Formatter.PrintDefinition("branch", inspector.GetItemAttribute("branch") ?? Constants.NotDefinedLiteral, output);
          Formatter.PrintDefinition("branchid", inspector.GetItemAttribute("branchid") ?? Constants.NotDefinedLiteral, output);
          Formatter.PrintDefinition("language", inspector.GetItemAttribute("language") ?? Constants.NotDefinedLiteral, output);
          Formatter.PrintDefinition("version", inspector.GetItemAttribute("version") ?? Constants.NotDefinedLiteral, output);
        }
      }

      return new CommandResult(CommandStatus.Success, output.ToString());
    }

    public override string Description()
    {
      return "Display attributes for a given item";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("-a name item1/item2");
      details.AddExample("-a templateid ../item1");
    }
  }
}
