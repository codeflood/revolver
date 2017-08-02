using System.Collections.Generic;
using System.Linq;

namespace Revolver.Core.Commands
{
  [Command("exit")]
  public class Exit : BaseCommand
  {
    [ListParameter("message")]
    [Description("A message to display to the user")]
    [Optional]
    public IEnumerable<string> MessageWords { get; set; }

    public override CommandResult Run()
    {
      return new CommandResult(CommandStatus.Abort, string.Join(" ", MessageWords));
    }

    public override string Description()
    {
      return "Aborts the current execution context";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("");
      details.AddExample("(missing parameter)");
    }
  }
}