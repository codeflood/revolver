using Sitecore.StringExtensions;
using System;

namespace Revolver.Core.Commands
{
    [Command("random")]
    public class Random : BaseCommand
    {
        private readonly static System.Random RandomProvider = new System.Random();
        private readonly static object RandomLock = new object();

        [NumberedParameter(0, "max")]
        [Description("The maximum allowed value.")]
        [Optional]
        public string Maximum { get; set; }

        [NumberedParameter(1, "min")]
        [Description("The minimum allowed value.")]
        [Optional]
        public string Minimum { get; set; }

        [NamedParameter("f", "fractions")]
        [Description("The number of fractal digits.")]
        [Optional]
        public int FractalDigits { get; set; }

        [FlagParameter("d")]
        [Description("Generate dates")]
        [Optional]
        public bool GenerateDates { get; set; }

        [FlagParameter("t")]
        [Description("Generate times")]
        [Optional]
        public bool GenerateTimes { get; set; }

        public Random()
        {
            Maximum = string.Empty;
            Minimum = string.Empty;
            FractalDigits = 0;
            GenerateDates = false;
            GenerateTimes = false;
        }

        public override CommandResult Run()
        {
            if (FractalDigits != 0 && (GenerateDates || GenerateTimes))
                return new CommandResult(CommandStatus.Failure, "Cannot use -d or -t with -f");

            if (GenerateDates || GenerateTimes)
                return RunDateTime();

            return RunNumber();
        }

        protected virtual CommandResult RunNumber()
        {
            var parsedMinimum = 0;
            var parsedMaximum = 10;

            if (!string.IsNullOrEmpty(Minimum) && !int.TryParse(Minimum, out parsedMinimum))
                return new CommandResult(CommandStatus.Failure, "Cannot parse '{0}' as integer for parameter 'min'".FormatWith(Minimum));

            if (!string.IsNullOrEmpty(Maximum) && !int.TryParse(Maximum, out parsedMaximum))
                return new CommandResult(CommandStatus.Failure, "Cannot parse '{0}' as integer for parameter 'max'".FormatWith(Minimum));

            if (FractalDigits == 0)
            {
                // Random will be an integer as interval is integer
                int value = GetRandomInteger(parsedMinimum, parsedMaximum);
                return new CommandResult(CommandStatus.Success, value.ToString());
            }
            
            // Random will be a double as interval is double
            var rnum = GetRandomDouble();
            var num = Math.Round(parsedMinimum + ((parsedMaximum - parsedMinimum) * rnum), FractalDigits);

            string formatString = "F" + FractalDigits;
            //F# = Force fractional digits, no commas. See http://msdn.microsoft.com/en-us/library/kfsatb94(v=vs.110).aspx
            return new CommandResult(CommandStatus.Success, num.ToString(formatString));
        }

        protected virtual CommandResult RunDateTime()
        {
            var parsedMinimum = DateTime.UtcNow;
            var parsedMaximum = parsedMinimum.AddMonths(6);

            if (!string.IsNullOrEmpty(Minimum) && !DateTime.TryParse(Minimum, out parsedMinimum))
                return new CommandResult(CommandStatus.Failure, "Cannot parse '{0}' as datetime for parameter 'min'".FormatWith(Minimum));

            if (!string.IsNullOrEmpty(Maximum) && !DateTime.TryParse(Maximum, out parsedMaximum))
                return new CommandResult(CommandStatus.Failure, "Cannot parse '{0}' as datetime for parameter 'max'".FormatWith(Minimum));

            var value = parsedMinimum;

            if (GenerateDates)
            {
                var addDays = GetRandomInteger(0, (int)Math.Ceiling((parsedMaximum - parsedMinimum).TotalDays));
                value = value.AddDays(GetRandomInteger(0, addDays));
            }

            if (GenerateTimes)
            {
                var minuteSpan = (int)Math.Ceiling(parsedMaximum.TimeOfDay.TotalMinutes);
                if (minuteSpan == 0)
                    minuteSpan = (int) Math.Ceiling(new TimeSpan(24, 0, 0).TotalMinutes);

                value = value.AddMinutes(GetRandomInteger(0, minuteSpan));
            }

            return new CommandResult(CommandStatus.Success, value.ToString());
        }

        protected int GetRandomInteger(int min, int max)
        {
            lock (RandomLock)
            {
                return RandomProvider.Next(min, max);
            }
        }

        protected double GetRandomDouble()
        {
            lock (RandomLock)
            {
                return RandomProvider.NextDouble();
            }
        }

        public override string Description()
        {
            return "Generate random numbers, dates and times";
        }

        public override void Help(HelpDetails details)
        {
            details.AddExample("10");
            details.AddExample("5 50");
            details.AddExample("10 -f 2");
            details.AddExample("-d 2020-12-31");
            details.AddExample("-d -t 2020-12-31");
            details.AddExample("-t");
            details.AddExample("-d -t 2010-01-01 2020-12-31");
        }
    }
}