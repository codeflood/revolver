using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.StringExtensions;
using System.Linq;

namespace Revolver.Core.Commands
{
  [Command("cpl")]
  public class CopyItemToLanguage : BaseCommand
  {
    [FlagParameter("o")]
    [Description("Overwrite existing fields of the target item in the target language")]
    [Optional]
    public bool Overwrite { get; set; }

    [NamedParameter("f", "field")]
    [Description("The name of a single field to copy")]
    [Optional]
    public string FieldName { get; set; }

    [NumberedParameter(0, "language")]
    [Description("The language to copy to")]
    public string LanguageName { get; set; }

    [NumberedParameter(1, "path")]
    [Description("The path of the source item to copy fields from. If not specified the current item is used.")]
    [Optional]
    public string Path { get; set; }

    public CopyItemToLanguage()
    {
      Overwrite = false;
      FieldName = string.Empty;
      LanguageName = string.Empty;
      Path = string.Empty;
    }

    public override CommandResult Run()
    {
      if(string.IsNullOrEmpty(LanguageName))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("language"));

      Language targetLanguage = null;

      if (!Language.TryParse(LanguageName, out targetLanguage))
        return new CommandResult(CommandStatus.Failure, "Failed to parse language '" + LanguageName + "'");

      // Ensure the selected language has been configured for this server
      if(!(from l in Context.CurrentDatabase.Languages where l == targetLanguage select l).Any())
        return new CommandResult(CommandStatus.Failure, "Language not found in database '" + Context.CurrentDatabase.Name + "'");

      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        // Get the target item
        var target = Context.CurrentDatabase.GetItem(Context.CurrentItem.ID, targetLanguage);
        if (target != null)
        {
          if (!string.IsNullOrEmpty(FieldName))
          {
            var itemField = Context.CurrentItem.Fields[FieldName];
            if (itemField != null)
            {
              if (target[itemField.Name] == string.Empty || Overwrite)
              {
                using (new EditContext(target))
                {
                  target[itemField.Name] = itemField.Value;
                }
              }
              else
                return new CommandResult(CommandStatus.Failure, "Target field not empty. Use -o to overwrite the target field value.");
            }
            else
            {
              return new CommandResult(CommandStatus.Failure, "Failed to find field '" + FieldName + "' on source item.");
            }
          }
          else
          {
            for (int i = 0; i < Context.CurrentItem.Fields.Count; i++)
            {
              var itemFieldname = Context.CurrentItem.Fields[i].Name;

              if (string.IsNullOrEmpty(itemFieldname))
              {
                var fieldItem = Context.CurrentDatabase.GetItem(Context.CurrentItem.Fields[i].ID);
                if (fieldItem != null)
                  itemFieldname = fieldItem.Name;
              }

              if (!string.IsNullOrEmpty(itemFieldname) && (target[itemFieldname] == string.Empty || Overwrite))
              {
                using (new EditContext(target))
                {
                  target[itemFieldname] = Context.CurrentItem[itemFieldname];
                }
              }
            }
          }
        }
        else
          return new CommandResult(CommandStatus.Failure, "Failed to find the item");
      }

      return new CommandResult(CommandStatus.Success, string.Format("Copied {0} field{2} to {1} field{2}", Context.CurrentLanguage.GetDisplayName() ?? Context.CurrentLanguage.Name, targetLanguage.GetDisplayName() ?? targetLanguage.Name, FieldName == string.Empty ? "s" : string.Empty));
    }

    public override string Description()
    {
      return "Copies item fields to a specific language version of the same item";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("jp");
      details.AddExample("-o da");
      details.AddExample("en-US ../path1");
      details.AddExample("-f title da");
    }
  }
}
