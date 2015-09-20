using System;

namespace Revolver.Core.Commands
{
  [AttributeUsage(AttributeTargets.Property)]
  public class FlagParameterAttribute : Attribute
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
    /// Construct a new instance of FlagParameterAttribute
    /// </summary>
    /// <param name="name">The name of the parameter</param>
    public FlagParameterAttribute(string name)
    {
      Name = name;
    }
  }
}