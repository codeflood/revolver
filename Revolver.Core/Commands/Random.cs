using Sitecore.StringExtensions;
using System;

namespace Revolver.Core.Commands
{
  [Command("random")]
  public class Random : BaseCommand
  {
    private static System.Random randomProvider = new System.Random();
    private static object theLock = new object();

    // todo: work out how to support positional parameters that can change depending on the number of parameters. Prefer min then max rather than max then optional min.
    [NumberedParameter(0, "max")]
    [Description("The maximum allowed value.")]
    public int Maximum { get; set; }

    [NumberedParameter(1, "min")]
    [Description("The minimum allowed value.")]
    [Optional]
    public int Minimum { get; set; }

    [NamedParameter("f", "fractions")]
    [Description("The number of fractal digits,")]
    public int FractalDigits { get; set; }

    public Random()
    {
      Maximum = int.MinValue;
      Minimum = 0;
      FractalDigits = 0;
    }
    
    public override CommandResult Run()
    {
      if (Maximum == int.MinValue)
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("max"));

   
      if (FractalDigits == 0)
      {
        // Random will be an integer as interval is integer
        int value = GetRandomInteger(Minimum, Maximum);
        return new CommandResult(CommandStatus.Success, value.ToString());
      }
      else
      {
        // Random will be a double as interval is integer

        var rnum = GetRandomDouble();
        var num = Math.Round(Minimum + ((Maximum - Minimum) * rnum), FractalDigits);
        
        string formatString = "F" + FractalDigits; 
        //F# = Force fractional digits, no commas. See http://msdn.microsoft.com/en-us/library/kfsatb94(v=vs.110).aspx
        return new CommandResult(CommandStatus.Success, num.ToString(formatString));
      }
    }

    private int GetRandomInteger(int min, int max)
    {
      lock(theLock)
      {
        return randomProvider.Next(min, max);
      }
    }

    private double GetRandomDouble()
    {
      lock (theLock)
      {
        return randomProvider.NextDouble();
      }
    }

    public override string Description()
    {
      return "Generate random numbers";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("10");
      details.AddExample("5 50");
      details.AddExample("10 -f 2");
    }
  }
}