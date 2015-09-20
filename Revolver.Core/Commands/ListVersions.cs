using Sitecore.Globalization;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("lsv")]
  public class ListVersions : BaseCommand
  {
    [NumberedParameter(0, "lang")]
    [Description("The language to list the versions for.")]
    [Optional]
    public string Lang { get; set; }

    public ListVersions()
    {
      Lang = string.Empty;
    }

    public override CommandResult Run()
    {
      var previousLanguage = Context.CurrentLanguage;
      var output = new StringBuilder();
      var id = Context.CurrentItem.ID;

      if (!string.IsNullOrEmpty(Lang))
      {
        // Language provided. List version numbers within language
        var language = Context.CurrentItem.Language;
        if (!Language.TryParse(Lang, out language))
          return new CommandResult(CommandStatus.Failure, "Failed to parse '{0}' as a language");

        var item = Context.CurrentDatabase.GetItem(id, language);
        foreach (var version in item.Versions.GetVersionNumbers())
        {
          output.Append(version.Number + " ");
        }
      }
      else
      {
        try
        {
          foreach (var language in Context.CurrentItem.Languages)
          {
            var item = Context.CurrentDatabase.GetItem(id, language);
            var count = item.Versions.Count;
            Formatter.PrintDefinition(language.CultureInfo.DisplayName + " [" + language.Name + "]",
                            count.ToString() + (count == 1 ? " version" : " versions"), output);
          }
        }
        finally
        {
          Context.CurrentLanguage = previousLanguage;
        }
      }

      return new CommandResult(CommandStatus.Success, output.ToString());
    }

    public override string Description()
    {
      return "List the versions of the context item";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("en");
    }
  }
}