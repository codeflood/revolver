using System.Collections.Generic;

namespace Revolver.Core.Commands
{
  [Command("mod")]
  public class MathModulus : BaseCommand
  {
    [ListParameterAttribute("numbers")]
    [Description("Any number of numbers to modulus from the result.")]
    public IList<string> Numbers { get; set; }

    public MathModulus()
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
          result %= num;
      }

      return new CommandResult(CommandStatus.Success, result.ToString());
    }

    public override string Description()
    {
      return "Find the modulus of multiple numbers";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("10 3");
      details.AddExample("5 2");
    }
  }
}