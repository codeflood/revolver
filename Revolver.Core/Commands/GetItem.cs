namespace Revolver.Core.Commands
{
  [Command("gi")]
  public class GetItem : BaseCommand
  {
    [FlagParameter("r")]
    [Description("Recursive. Include this item's subitems.")]
    [Optional]
    public bool Recursive { get; set; }

    [NumberedParameter(0, "path")]
    [Description("The path to the item.")]
    [Optional]
    public string Path { get; set; }

    public GetItem()
    {
      Recursive = false;
      Path = string.Empty;
    }

    public override CommandResult Run()
    {
      var xml = string.Empty;

      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        xml = Context.CurrentItem.GetOuterXml(Recursive);
      }

      return new CommandResult(CommandStatus.Success, xml);
    }

    public override string Description()
    {
      return "Get the XML which constitutes this item";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("-r");
      details.AddExample("../a/bb");
    }
  }
}
