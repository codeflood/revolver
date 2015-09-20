using System;

namespace Revolver.Core.Commands
{
  public class DescriptionAttribute : Attribute
  {
    public string Description
    {
      get;
      protected set;
    }

    public DescriptionAttribute(string description)
    {
      Description = description;
    }
  }
}