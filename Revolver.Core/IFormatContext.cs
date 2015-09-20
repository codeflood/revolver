using Revolver.Core.Formatting;
using System;

namespace Revolver.Core
{
  [Obsolete("Use IOutputFormatter instead")]
  public interface IFormatContext : ICommandFormatter
  {
    /// <summary>
    /// Gets the characters which represents a newline for this context
    /// </summary>
    [Obsolete("Use PrintLine() instead")]
    string[] NewLines { get; }
  }
}
