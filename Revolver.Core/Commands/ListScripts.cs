using System.Text;

namespace Revolver.Core.Commands
{
  [Command("ls-scripts")]
  public class ListScripts : BaseCommand
  {
    public override CommandResult Run()
    {
      var scripts = Context.CommandHandler.ScriptLocator.GetScriptNames();
      var buffer = new StringBuilder();

      foreach (var script in scripts)
      {
        Formatter.PrintLine(script, buffer);
      }

      return new CommandResult(CommandStatus.Success, buffer.ToString());
    }

    public override string Description()
    {
      return "List all script names";
    }

    public override void Help(HelpDetails details)
    {
      var help = new HelpDetails
      {
        Description = Description()
      };
    }
  }
}