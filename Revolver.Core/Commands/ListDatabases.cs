using System.Text;

namespace Revolver.Core.Commands
{
  [Command("lsdb")]
  public class ListDatabases : BaseCommand
  {
    public override CommandResult Run()
    {
      var names = Sitecore.Configuration.Factory.GetDatabaseNames();

      var output = new StringBuilder();
      foreach (var name in names)
      {
        Formatter.PrintLine(name, output);
      }
      return new CommandResult(CommandStatus.Success, output.ToString());
    }

    public override string Description()
    {
      return "List the databases of the system";
    }

    public override void Help(HelpDetails details)
    {
    }
  }
}
