using Sitecore;
using Sitecore.StringExtensions;
using System;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Revolver.Core.Commands
{
  [Command("pp")]
  public class PrettyPrint : BaseCommand
  {
    [FlagParameter("x")]
    [Description("Format input as XML")]
    [Optional]
    public bool FormatXml { get; set; }

    [FlagParameter("d")]
    [Description("Format input as a date")]
    [Optional]
    public bool FormatDate { get; set; }

    [FlagParameter("j")]
    [Description("Format input as JSON")]
    [Optional]
    public bool FormatJson { get; set; }

    [NumberedParameter(0, "input")]
    [Description("The input to format")]
    public string Input { get; set; }

    public override CommandResult Run()
    {
      if (!FormatXml && !FormatDate && !FormatJson)
        return new CommandResult(CommandStatus.Failure, "One of -x, -j or -d must be used");

      var flagCount = 0;

      if (FormatXml)
        flagCount++;

      if (FormatJson)
        flagCount++;

      if (FormatDate)
        flagCount++;

      if (flagCount > 1)
        return new CommandResult(CommandStatus.Failure, "Only one of -x, -j or -d can be used");

      if(string.IsNullOrEmpty(Input))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("input"));

      if (FormatXml)
        return RunFormatXml();

      if (FormatDate)
        return RunFormatDate();

      if (FormatJson)
        return RunFormatJson();

      return new CommandResult(CommandStatus.Failure, "Unknown format options");
    }

    protected virtual CommandResult RunFormatXml()
    {
      try
      {
        var doc = XDocument.Parse(Input);
        return new CommandResult(CommandStatus.Success, doc.ToString());
      }
      catch (XmlException ex)
      {
        return new CommandResult(CommandStatus.Failure, ex.Message);
      }
    }

    protected virtual CommandResult RunFormatDate()
    {
      // Try parsing as a Sitecore ISO date
      var parsedDate = DateUtil.IsoDateToDateTime(Input, DateTime.MinValue);

      if (parsedDate == DateTime.MinValue)
        DateTime.TryParse(Input, out parsedDate);

      if (parsedDate != DateTime.MinValue)
        return new CommandResult(CommandStatus.Success, parsedDate.ToString());

      return new CommandResult(CommandStatus.Failure, "Failed to parse input as date");
    }

    protected virtual CommandResult RunFormatJson()
    {
      try
      {
        var parsed = JsonConvert.DeserializeObject(Input);
        var formatted = JsonConvert.SerializeObject(parsed, Newtonsoft.Json.Formatting.Indented);

        return new CommandResult(CommandStatus.Success, formatted);
      }
      catch (JsonReaderException ex)
      {
        return new CommandResult(CommandStatus.Failure, ex.Message);
      }
    }

    public override string Description()
    {
      return "Format input to a user friendly format";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("-x <xml><el></el></xml>");
      details.AddExample("-d 20140613T142316");
    }
  }
}