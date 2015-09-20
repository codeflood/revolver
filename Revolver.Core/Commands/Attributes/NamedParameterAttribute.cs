using System;

namespace Revolver.Core.Commands
{
  [AttributeUsage(AttributeTargets.Property)]
  public class NamedParameterAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the name of the parameter
    /// </summary>
    public string Name
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
    /// Gets or sets the number of words from the input this parameter requires
    /// </summary>
    public int WordCount
    {
      get;
      protected set;
    }

    /// <summary>
    /// Construct a new instance of NamedParameterAttribute
    /// </summary>
    /// <param name="name">The name of the parameter</param>
    /// <param name="helpValuePlaceholder">The value placeholder to use in help messages</param>
    /// <param name="wordCount">The number of words the parameter will take from the input</param>
    public NamedParameterAttribute(string name, string helpValuePlaceholder = "", int wordCount = 1)
    {
      Name = name;
      HelpValuePlaceholder = helpValuePlaceholder;
      WordCount = wordCount;
    }
  }
}