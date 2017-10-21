using Revolver.Core;
using Revolver.Core.Commands;
using Revolver.Core.Formatting;

namespace Revolver.Test
{
  [Command("cc2")]
  public class CustomCommand2 : ICommand
  {
    public void Initialise(Core.Context context, ICommandFormatter formatContext)
    {
    }

    public CommandResult Run()
    {
      return new CommandResult(CommandStatus.Success, "lorem");
    }

    public string Description()
    {
      return "Another custom command";
    }

    public void Help(HelpDetails details)
    {
    }
  }
}