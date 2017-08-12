using System;
using Sitecore.Data;

namespace Revolver.Core.Commands
{
  [Command("newid")]
  public class NewID : BaseCommand
  {
    public override CommandResult Run()
    {
      return new CommandResult(CommandStatus.Success, ID.NewID.ToString());
    }

    public override string Description()
    {
      return "Generate a new ID";
    }

    public override void Help(HelpDetails details)
    {
    }
  }
}