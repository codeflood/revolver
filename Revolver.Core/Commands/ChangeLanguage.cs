using Sitecore.Globalization;
using System.Linq;

namespace Revolver.Core.Commands
{
  [Command("cl")]
  public class ChangeLanguage : BaseCommand
  {
    [FlagParameter("d")]
    [Description("Change to the default language")]
    [Optional]
    public bool DefaultLanguage { get; set; }

    [FlagParameter("f")]
    [Description("Force the language change even if the language doesn't exist in the current database")]
    [Optional]
    public bool Force { get; set; }

    [NumberedParameter(0, "language")]
    [Description("The language code to change to")]
    [Optional]
    public string Language { get; set; }

    public ChangeLanguage()
    {
      DefaultLanguage = false;
      Force = false;
      Language = string.Empty;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Language) && !DefaultLanguage)
        return new CommandResult(CommandStatus.Failure, "Either 'language' or -d is required");

      var langString = DefaultLanguage ? Sitecore.Context.Site.Language : Language;

      Language language = null;

      if (!Sitecore.Globalization.Language.TryParse(langString, out language))
        return new CommandResult(CommandStatus.Failure, "Failed to parse language '" + langString + "'");

      // Ensure the selected language has been configured for this database
      var validLanguage = Force || Context.CurrentDatabase.Languages.Contains(language);
      if (!validLanguage)
        return new CommandResult(CommandStatus.Failure, "Language not found");

      Context.CurrentLanguage = language;
      return new CommandResult(CommandStatus.Success, "Language " + Context.CurrentLanguage.CultureInfo.DisplayName + " [" + Context.CurrentLanguage.CultureInfo.Name + "]");
    }

    public override string Description()
    {
      return "Change the context language";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments = "One of language or -d must be used";
      details.AddExample("en");
      details.AddExample("zg-CH");
      details.AddExample("-d");
      details.AddExample("-f kk");
    }
  }
}
