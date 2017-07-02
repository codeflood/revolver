using Sitecore.Data;
using Sitecore.Globalization;
using Sitecore.StringExtensions;
using System.Collections.Generic;
using System.Linq;

namespace Revolver.Core.Commands
{
  [Command("rmv")]
  public class DeleteVersions : BaseCommand
  {
    [FlagParameter("ov")]
    [Description("All other versions. Delete all versions except the context version.")]
    [Optional]
    public bool OtherVersions { get; set; }

    [FlagParameter("l")]
    [Description("All languages. Delete the verion(s) in all languages.")]
    [Optional]
    public bool AllLanguages { get; set; }

    [NumberedParameter(0, "path")]
    [Description("The path of the item to delete the version from. If not specified the current item is used.")]
    [Optional]
    public string Path { get; set; }

    public override CommandResult Run()
    {
      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        var languages = new List<Language>();
        languages.Add(Context.CurrentItem.Language);

        if (AllLanguages)
        {
          languages.Clear();
          languages.AddRange(Context.CurrentItem.Languages);
        }

        var versions = new List<int>();
        versions.Add(Context.CurrentItem.Version.Number);

        if (OtherVersions)
        {
          versions.Clear();
          versions.AddRange(Context.CurrentItem.Versions.GetVersionNumbers().Select(x => x.Number));
          versions.Remove(Context.CurrentItem.Version.Number);

          if (AllLanguages)
          {
            // Other languages may have different version numbers. Need to add those extras
            foreach (var language in Context.CurrentItem.Languages)
            {
              var langItem = Context.CurrentItem.Database.GetItem(Context.CurrentItem.ID, language);
              foreach (var version in langItem.Versions.GetVersionNumbers())
              {
                if (!versions.Contains(version.Number) && version.Number != Context.CurrentItem.Version.Number)
                  versions.Add(version.Number);
              }
            }
          }
        }

        var count = 0;

        foreach (var language in languages)
        {
          foreach (var version in versions)
          {
            var versionItem = Context.CurrentItem.Database.GetItem(Context.CurrentItem.ID, language, new Version(version));
            if (versionItem != null)
            {
              versionItem.Versions.RemoveVersion();
              count++;
            }
          }
        }

        return new CommandResult(CommandStatus.Success, "Deleted {0} version{1}".FormatWith(count, count == 1 ? string.Empty : "s"));
      }
    }

    public override string Description()
    {
      return "Delete versions from an item.";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments += "Extended paths can be used to specify a specific version.";
      details.AddExample("-ao");
      details.AddExample("item1/item2");
      details.AddExample("-al");
      details.AddExample("::4");
    }
  }
}
