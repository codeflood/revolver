using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Data;
using Sitecore.StringExtensions;

namespace Revolver.Core.Commands
{
  [Command("pageevents")]
  public class ItemPageEvents : BaseCommand
  {
    public const string TrackingFieldName = "__tracking";
    public const string RootXmlElementName = "tracking";
    public const string PageEventXmlElementName = "event";
    public const string IdXmlAttributeName = "id";
    public const string NameXmlAttributeName = "name";

    [FlagParameter("a")]
    [Description("Add the IDs to the tracking field.")]
    [Optional]
    public bool Add { get; set; }

    [FlagParameter("r")]
    [Description("Remove the IDs from the tracking field.")]
    [Optional]
    public bool Remove { get; set; }

    [ListParameter("ids")]
    [Description("The IDs to operate with.")]
    [Optional]
    public List<string> PageEventIds { get; set; }

    public ItemPageEvents()
    {
      Add = false;
      Remove = false;
    }

    public override CommandResult Run()
    {
      if(Add && Remove)
        return new CommandResult(CommandStatus.Failure, "Only one of -a or -r can be used.");

      // parse existing tracking field
      var xml = ParseTrackingField();
      if(xml == null)
        return new CommandResult(CommandStatus.Failure, "Unable to parse XML from field.");

      if (Add || Remove)
      {
        if (PageEventIds.Count == 0)
          return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("ids"));

        // process the XML
        var result = ProcessXml(xml);

        // save the field
        var item = Context.CurrentItem;
        item.Editing.BeginEdit();
        item[TrackingFieldName] = xml.ToString(SaveOptions.DisableFormatting);
        item.Editing.EndEdit();

        return result;
      }
      
      return ListEvents(xml);
    }

    private XDocument ParseTrackingField()
    {
      var rawField = Context.CurrentItem[TrackingFieldName];
      if (string.IsNullOrEmpty(rawField))
        rawField = "<" + RootXmlElementName + "/>";

      try
      {
        return XDocument.Parse(rawField);
      }
      catch (XmlException)
      {
        return null;
      }
    }

    private CommandResult ProcessXml(XDocument doc)
    {
      var trackingElement = doc.Element(RootXmlElementName);
      if (trackingElement == null)
        return new CommandResult(CommandStatus.Failure, "Unable to find the '" + RootXmlElementName + "' element in the tracking XML");

      var messages = new List<string>();
      var status = CommandStatus.Success;

      foreach (var idString in PageEventIds)
      {
        ID id = null;
        var success = ID.TryParse(idString, out id);
        if (!success)
        {
          status = CommandStatus.Failure;
          messages.Add("Cannot parse " + idString + " as an ID");
          continue;
        }

        if (Add && trackingElement.Elements(PageEventXmlElementName).All(x => ID.Parse(x.Attribute(IdXmlAttributeName)?.Value) != id))
        {
          var definitionItem = Context.CurrentDatabase.GetItem(id);
          if (definitionItem == null)
          {
            status = CommandStatus.Failure;
            messages.Add("Page event definition " + id + " not found");
          }
          else
          {
            trackingElement.Add(new XElement(PageEventXmlElementName, new XAttribute(IdXmlAttributeName, id), new XAttribute(NameXmlAttributeName, definitionItem.Name)));
            messages.Add("Added page event " + id);
          }
        }

        if (Remove)
        {
          var targetElement = trackingElement.Elements(PageEventXmlElementName).FirstOrDefault(x => ID.Parse(x.Attribute(IdXmlAttributeName)?.Value) == id);

          if (targetElement == null)
          {
            status = CommandStatus.Failure;
            messages.Add("Failed to find ID in field: " + id);
          }
          else
          {
            targetElement.Remove();
            messages.Add("Removed page event " + id);
          }
        }
      }

      return new CommandResult(status, Formatter.JoinLines(messages));
    }

    private CommandResult ListEvents(XDocument doc)
    {
      var buffer = new StringBuilder();

      var trackingElement = doc.Element(RootXmlElementName);
      if(trackingElement == null)
        return new CommandResult(CommandStatus.Success, "Tracking field is empty");

      var elements = trackingElement.Elements(PageEventXmlElementName);

      if (!elements.Any())
        buffer.Append("No events in field");

      foreach (var ev in elements)
      {
        buffer.Append(Formatter.PrintDefinition(ev.Attribute(IdXmlAttributeName)?.Value ?? "??", ev.Attribute(NameXmlAttributeName)?.Value ?? "??"));
      }

      return new CommandResult(CommandStatus.Success, buffer.ToString());
    }

    public override string Description()
    {
      return "List, add or remove page events from an item";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments = Formatter.JoinLines(new[]
      {
        "Without -a or -r the existing page events will be listed",
        "Only either -a or -r can be used at once.",
      });

      details.Examples.Add("");
      details.Examples.Add("-a {7275BDB8-EC28-48DE-90F9-ED732510F89B} {7DC516D4-BE55-45DE-80DB-E2FCEBD1DA3C}");
      details.Examples.Add("-a {7275BDB8-EC28-48DE-90F9-ED732510F89B}");
      details.Examples.Add("-r {7275BDB8-EC28-48DE-90F9-ED732510F89B}");
    }
  }
}