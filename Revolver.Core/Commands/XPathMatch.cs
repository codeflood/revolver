using HtmlAgilityPack;
using Sitecore.StringExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Revolver.Core.Commands
{
  [Command("xpath")]
  public class XPathMatch : BaseCommand
  {
    [FlagParameter("h")]
    [Description("Preprocess the input using HtmlAgilityPack")]
    public bool HAPRequired { get; set; }

    [FlagParameter("v")]
    [Description("Extract the value of the matches nodes")]
    public bool ValueOutput { get; set; }

    /// <summary>
    /// True indicates stats will not be visible, false indicates stats will be visible
    /// </summary>
    [FlagParameter("ns")]
    [Description("No statistics. Don't output the number of nodes matched")]
    public bool NoStats { get; set; }

    [NumberedParameter(0, "input")]
    [Description("The XML input to match within")]
    public string Input { get; set; }

    [NumberedParameter(1, "xpath")]
    [Description("The XPath used to match")]
    public string XPath { get; set; }

    public XPathMatch()
    {
      HAPRequired = false;
      ValueOutput = false;
      NoStats = false;
      Input = string.Empty;
      XPath = string.Empty;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(Input))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("input"));

      if (string.IsNullOrEmpty(XPath))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("xpath"));

      XPathNavigator nav = null;

      if (HAPRequired)
      {
        var doc = new HtmlDocument();
        try
        {
          doc.LoadHtml(Input);
        }
        catch (Exception ex)
        {
          return new CommandResult(CommandStatus.Success, "Failed to parse input: " + ex.Message);
        }

        nav = doc.CreateNavigator();
      }
      else
      {
        var doc = new XmlDocument();

        try
        {
          doc.LoadXml(Input);
        }
        catch (Exception ex)
        {
          return new CommandResult(CommandStatus.Success, "Failed to parse input: " + ex.Message);
        }

        nav = doc.CreateNavigator();
      }

      if (nav == null)
        return new CommandResult(CommandStatus.Failure, "Failed to create XPath navigator");

      // Process XML and extract any namespaces it contains. Allows using the namespaces in queries
      var namespaceManager = new XmlNamespaceManager(nav.NameTable);

      var allNodes = nav.Select("//*");
      while (allNodes.MoveNext())
      {
        var namespaces = allNodes.Current.GetNamespacesInScope(XmlNamespaceScope.Local);
        foreach (var ns in namespaces)
        {
          namespaceManager.AddNamespace(ns.Key, ns.Value);
        }
      }

      var nodes = nav.Select(XPath, namespaceManager);

      var lines = new List<string>();
      while (nodes.MoveNext())
      {
        if (ValueOutput)
          lines.Add(nodes.Current.Value);
        else
          lines.Add(nodes.Current.OuterXml);
      }

      var buffer = new StringBuilder(Formatter.JoinLines(lines));

      if (!NoStats)
      {
        Formatter.PrintLine(string.Empty, buffer);
        buffer.Append(string.Format("Matched {0} {1}", nodes.Count, (nodes.Count == 1 ? "node" : "nodes")));
      }

      return new CommandResult(CommandStatus.Success, buffer.ToString());
    }

    public override string Description()
    {
      return "Extract matches from XML using XPath";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("(<a id=\"myid\"></a>) (//@id)");
      details.AddExample("-h (<div><h1>title</h1><br><div class=top>the top</div></div>) //div[@class='top']");
      details.AddExample("-v (<a>val<b>subval</b></a>) ./a");
    }
  }
}