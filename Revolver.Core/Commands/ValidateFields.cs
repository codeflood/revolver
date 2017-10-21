using Sitecore.Data.Items;
using System.Text;
using System.Text.RegularExpressions;

namespace Revolver.Core.Commands {
  /// <summary>
  /// Validate each field individually using the regex for each field.
  /// </summary>
  [Command("val")]
  public class ValidateFields : BaseCommand {

    [NumberedParameter(0, "path")]
    [Description("The path of the item to validate. If not specified the current item is used.")]
    public string Path { get; set; }

    public ValidateFields() {
      Path = string.Empty;
    }

    public override CommandResult Run() {
      
      using (ContextSwitcher cs = new ContextSwitcher(Context, Path)) {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        StringBuilder sb = new StringBuilder(500);
        Item item = Context.CurrentItem;
        // TODO: Review using item.Fields here. Check source item is of type document ('a' wasn't during testing)
        for (int i = 0; i < item.Template.Fields.Length; i++) {
          // Get regex from template field def
          string regexStr = item.Template.Fields[i].Validation;
          Sitecore.Data.ID id = item.Template.Fields[i].ID;
          if (regexStr != null && regexStr != string.Empty) {
            if (!Regex.Match(item.Fields[id].Value, item.Fields[id].Validation).Success) {
              if (sb.Length > 0)
                Formatter.PrintLine(string.Empty, sb);
                
              sb.Append("\"");
              sb.Append(item.Fields[id].ValidationText);
              sb.Append("\" in field ");
              sb.Append(item.Fields[id].Name);
            }
          }
        }

        if (sb.Length > 0)
        {
          var output = new StringBuilder();
          Formatter.PrintLine("FAILED: Validation failed for " + item.Paths.FullPath, output);
          output.Append(sb);

          return new CommandResult(CommandStatus.Success, output.ToString());
        } else
          return new CommandResult(CommandStatus.Success, "PASSED: Validation passed for " + item.Paths.FullPath);
      }
    }

    public override string Description() {
      return "Performs field validation for the item";
    }

    public override void Help(HelpDetails details) {
      details.AddExample("/sitecore/content/home/a");
    }
  }
}
