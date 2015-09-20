using System.Collections.Generic;

namespace Revolver.Core.Commands
{
  [Command("multiply")]
  public class MathMultiply : BaseCommand
  {
    [ListParameterAttribute("numbers")]
    [Description("Any number of numbers to multiply with the result.")]
    public IList<string> Numbers { get; set; }

    public MathMultiply()
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
          result *= num;
      }

      return new CommandResult(CommandStatus.Success, result.ToString());
    }

    public override string Description()
    {
      return "Multiply multiple numbers together";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("<cmd> 3 6");
      details.AddExample("<cmd> 4 0.5");
    }
  }
}