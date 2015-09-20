using Sitecore.StringExtensions;
using System.Linq;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("ct")]
  public class ChangeTemplate : BaseCommand
  {
    [FlagParameter("f")]
    [Description("Force the change even if the target template is not compatible")]
    [Optional]
    public bool Force { get; set; }

    [NumberedParameter(0, "template")]
    [Description("The template to change to")]
    public string Template { get; set; }

    [NumberedParameter(1, "path")]
    [Description("The path of the item to change template on")]
    [Optional]
    public string Path { get; set; }

    public ChangeTemplate()
    {
      Force = false;
      Template = string.Empty;
      Path = string.Empty;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Template))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("template"));

      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        var templateItem = Context.CurrentDatabase.Templates[Template];
        if (templateItem == null)
          return new CommandResult(CommandStatus.Failure, "Failed to find the template");

        var itemFieldNames = from field in Context.CurrentItem.Fields
                             select field.Name;

        var templateFieldNames = from field in templateItem.Fields
                                 select field.Name;

        // Check if the new template contains all the currently used fields.
        var missingFields = itemFieldNames.Except(templateFieldNames);

        if (missingFields.Any() && !Force)
        {
          var output = new StringBuilder();
          Formatter.PrintLine("Incompatible template. Use -f to force the change. The following fields are missing on the target template: ", output);

          foreach (var fieldName in missingFields.OrderBy(x => x))
          {
            Formatter.PrintLine(fieldName, output);
          } 

          return new CommandResult(CommandStatus.Failure, output.ToString());
        }
        else
        {
          Context.CurrentItem.ChangeTemplate(templateItem);
          return new CommandResult(CommandStatus.Success, "Item's template has been changed");
        }
      }
    }

    public override string Description()
    {
      return "Change an item's template";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("Document");
      details.AddExample("Document /sitecore/content/home");
    }
  }
}
