using Sitecore;
using Sitecore.StringExtensions;
using System;
using System.Xml;
using System.Xml.Linq;

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

    [NumberedParameter(0, "input")]
    [Description("The input to format")]
    public string Input { get; set; }

    public override CommandResult Run()
    {
      if (!FormatXml && !FormatDate)
        return new CommandResult(CommandStatus.Failure, "One of -x or -d must be used");

      if (FormatXml && FormatDate)
        return new CommandResult(CommandStatus.Failure, "Only one of -x or -d can be used");

      if(string.IsNullOrEmpty(Input))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("input"));

      if(FormatXml)
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

      if(FormatDate)
      {
        // Try parsing as a Sitecore ISO date
        var parsedDate = DateUtil.IsoDateToDateTime(Input, DateTime.MinValue);

        if (parsedDate == DateTime.MinValue)
          DateTime.TryParse(Input, out parsedDate);

        if (parsedDate != DateTime.MinValue)
          return new CommandResult(CommandStatus.Success, parsedDate.ToString());

        return new CommandResult(CommandStatus.Failure, "Failed to parse input as date");
      }

      return new CommandResult(CommandStatus.Failure, "Unknown format options");
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