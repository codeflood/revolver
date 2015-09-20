using System.Collections.Generic;

namespace Revolver.Core.Commands
{
  [Command("add")]
  public class MathAdd : BaseCommand
  {
    [ListParameterAttribute("numbers")]
    [Description("Any number of numbers to add to the result.")]
    public IList<string> Numbers { get; set; }

    public MathAdd()
    {
      Numbers = new List<string>();
    }

    public override CommandResult Run()
    {
      var result = 0.0d;
      foreach (var arg in Numbers)
      {
        var num = 0.0d;
        if (!double.TryParse(arg, out num))
          return new CommandResult(CommandStatus.Failure, "Failed to parse '" + arg + "' as a number");

        result += num;
      }

      return new CommandResult(CommandStatus.Success, result.ToString());
    }

    public override string Description()
    {
      return "Add multiple numbers together";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("2 3");
      details.AddExample("2 3 6");
    }
  }
}