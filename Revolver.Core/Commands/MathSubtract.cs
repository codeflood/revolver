using System.Collections.Generic;

namespace Revolver.Core.Commands
{
  [Command("subtract")]
  public class MathSubtract : BaseCommand
  {
    [ListParameterAttribute("numbers")]
    [Description("Any number of numbers to subtract from the result.")]
    public IList<string> Numbers { get; set; }

    public MathSubtract()
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
          return new CommandResult(CommandStatus.Failure, "Failed to parse '" + Numbers[i] + "' as a number");

        if (i == 0)
          result = num;
        else
          result -= num;
      }

      return new CommandResult(CommandStatus.Success, result.ToString());
    }

    public override string Description()
    {
      return "Subtract multiple numbers from each other";
    }

    public override void Help(HelpDetails details)
    {

      details.AddExample("<cmd> 12 4");
      details.AddExample("<cmd> 12 3 4");
    }
  }
}