using Sitecore.StringExtensions;
using System;

namespace Revolver.Core.Commands
{
  [Command("cdb")]
  public class ChangeDatabase : BaseCommand
  {
    [NumberedParameter(0, "name")]
    [Description("The name of the database to change to")]
    public string DatabaseName { get; set; }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(DatabaseName))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("name"));

      try
      {
        var db = Sitecore.Configuration.Factory.GetDatabase(DatabaseName);
        if (db != null)
          Context.CurrentDatabase = db;
        else
          return new CommandResult(CommandStatus.Failure, "Failed to find database '" + DatabaseName + "'");
      }
      catch (Exception ex)
      {
        return new CommandResult(CommandStatus.Failure, "Failed to find database '" + DatabaseName + "'");
      }

      return new CommandResult(CommandStatus.Success, "Database " + Context.CurrentDatabase.Name);
    }

    public override string Description()
    {
      return "Change the current database";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("master");
      details.AddExample("core");
      details.AddExample("web");
    }
  }
}
