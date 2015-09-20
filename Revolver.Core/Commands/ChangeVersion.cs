namespace Revolver.Core.Commands
{
  [Command("cv")]
  public class ChangeVersion : BaseCommand
  {
    [FlagParameter("l")]
    [Description("Change to the latest version")]
    [Optional]
    public bool Latest { get; set; }

    [NumberedParameter(0, "version")]
    [Description("The version to change to. Negative numbers index from the end of the version list (-1 is version before latest)")]
    [Optional]
    public int Version { get; set; }

    public ChangeVersion()
    {
      Latest = false;
      Version = 0;
    }

    public override CommandResult Run()
    {
      if (Version == 0 && !Latest)
        return new CommandResult(CommandStatus.Failure, "Either '-l' or 'version' is required");

      if (Latest)
        Context.CurrentItem = Context.CurrentItem.Versions.GetLatestVersion();
      else
      {
        var result = Context.SetContext(Context.CurrentItem.ID.ToString(), null, null, Version);
        if (result.Status != CommandStatus.Success)
          return result;
      }

      return new CommandResult(CommandStatus.Success, "Version " + Context.CurrentItem.Version.Number.ToString());
    }

    public override string Description()
    {
      return "Change to a different version of the current item";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("3");
      details.AddExample("-2");
      details.AddExample("-l");
    }
  }
}