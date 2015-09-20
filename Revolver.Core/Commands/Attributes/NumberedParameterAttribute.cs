using System;

namespace Revolver.Core.Commands
{
  [AttributeUsage(AttributeTargets.Property)]
  public class NumberedParameterAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the index of the parameter
    /// </summary>
    public int Number
    {
      get;
      protected set;
    }

    /// <summary>
    /// Gets or sets the value placeholder to use in help messages
    /// </summary>
    public string HelpValuePlaceholder
    {
      get;
      protected set;
    }

    /// <summary>
    /// Construct a new instance of NumberedParameterAttribute
    /// </summary>
    /// <param name="number">The index of the parameter</param>
    /// <param name="helpValuePlaceholder">The value placeholder to use in help messages</param>
    public NumberedParameterAttribute(int number, string helpValuePlaceholder = "")
    {
      Number = number;
      HelpValuePlaceholder = helpValuePlaceholder;
    }
  }
}