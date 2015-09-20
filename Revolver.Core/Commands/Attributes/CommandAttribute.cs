using System;

namespace Revolver.Core.Commands
{
  [AttributeUsage(AttributeTargets.Class)]
  public class CommandAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the command name to bind to
    /// </summary>
    public string Binding
    {
      get;
      protected set;
    }

    /// <summary>
    /// Construct a new instance of CommandAttribute
    /// </summary>
    /// <param name="binding">The command name to bind to</param>
    public CommandAttribute(string binding)
    {
      Binding = binding;
    }
  }
}