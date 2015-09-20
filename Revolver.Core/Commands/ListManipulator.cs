using Sitecore.StringExtensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Revolver.Core.Commands
{
  [Command("lm")]
  public class ListManipulator : BaseCommand
  {
    private int? _randomSeed = null;

    [FlagParameter("a")]
    [Description("Add the unique element to the input.")]
    [Optional]
    public bool Add { get; set; }

    [FlagParameter("r")]
    [Description("Remove the element from the input.")]
    [Optional]
    public bool Remove { get; set; }

    [FlagParameter("s")]
    [Description("Shuffle the elements of the list.")]
    [Optional]
    public bool Shuffle { get; set; }

    [FlagParameter("o")]
    [Description("Order the elements in the list alphabetically.")]
    [Optional]
    public bool OrderList { get; set; }

    [FlagParameter("d")]
    [Description("List items in reverse order.")]
    [Optional]
    public bool ReverseOrder { get; set; }

    [NumberedParameter(0, "input")]
    [Description("The input to manipulate.")]
    public string Input { get; set; }

    [NumberedParameter(1, "delimiter")]
    [Description("The list delimiter seperating the list elements.")]
    public string Delimiter { get; set; }

    [NumberedParameter(2, "element")]
    [Description("The element to add or remove.")]
    public string Element { get; set; }

    public ListManipulator() : this(null)
    {
    }

    public ListManipulator(int? randomSeed)
    {
      Add = false;
      Remove = false;
      Input = string.Empty;
      Delimiter = string.Empty;
      Element = string.Empty;
      Shuffle = false;
      OrderList = false;
      _randomSeed = randomSeed;
    }

    public override CommandResult Run()
    {

      if (!Add && !Remove && !Shuffle && !OrderList)
        return new CommandResult(CommandStatus.Failure, "Missing one of -a -r -s -o required flag");

      if (Input == null)
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("input"));

      if (string.IsNullOrEmpty(Delimiter))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("delimiter"));

      if (string.IsNullOrEmpty(Element) && !Shuffle && !OrderList)
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("element"));

      if (Shuffle && OrderList)
        return new CommandResult(CommandStatus.Failure, "Cannot use -s and -o together");

      var elements = new List<string>();
      elements.AddRange(Input.Split(new[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries));

      if (Add)
      {
        if (!elements.Contains(Element))
          elements.Add(Element);
      }
      else if (Remove)
      {
        if (elements.Contains(Element))
          elements.Remove(Element);
      }
      else if (Shuffle)
      {
        System.Random random;
        
        if(_randomSeed != null)
          random = new System.Random(_randomSeed.Value);
        else
          random = new System.Random();

        elements = (from element in elements
                    orderby random.Next()
                    select element).ToList();
      }
      else if (OrderList)
      {
        elements = (from element in elements
                    orderby element
                    select element).ToList();
      }
      else
        return new CommandResult(CommandStatus.Failure, "Unknown operation");

      if (ReverseOrder)
        elements.Reverse();

      return new CommandResult(CommandStatus.Success, string.Join(Delimiter, elements.ToArray()));
    }

    public override string Description()
    {
      return "Manipulate delimited lists in strings";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments = "One of -a or -r must be specified";

      details.AddExample("-a a-b-c - d");
      details.AddExample("-r {945F96B9-5A7D-459C-8240-3A61362A0D32}|{F77216B8-5740-4680-A93A-227D0F897455} | {945F96B9-5A7D-459C-8240-3A61362A0D32}");
    }
  }
}
