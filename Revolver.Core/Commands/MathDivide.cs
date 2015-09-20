using System.Collections.Generic;

namespace Revolver.Core.Commands
{
  [Command("divide")]
  public class MathDivide : BaseCommand
  {
    [ListParameterAttribute("numbers")]
    [Description("Any number of numbers to divide from the result.")]
    public IList<string> Numbers { get; set; }

    public MathDivide()
    {
      Numbers = new List<string>();
    }

    public override CommandResult Run()
    {
      var result = 0.0d;
      for (var i = 0; i < Numbers.Count; i++)
      {
        var num = 0.0d;
        if (!double.TryParse(Numbers[i], out num))
          return new CommandResult(CommandStatus.Failure, "Failed to parse '" + Numbers + "' as a number");

        if (i == 0)
          result = num;
        else
          result /= num;
      }

      return new CommandResult(CommandStatus.Success, result.ToString());
    }

    public override string Description()
    {
      return "Divide multiple numbers from each other";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("10 2");
      details.AddExample("36 6");
    }
  }
}