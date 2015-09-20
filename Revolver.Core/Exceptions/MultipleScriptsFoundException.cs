using System.Collections.Generic;

namespace Revolver.Core.Exceptions
{
  public class MultipleScriptsFoundException : RevolverException
  {
    public IEnumerable<string> Names { get; private set; }

    public MultipleScriptsFoundException(IEnumerable<string> names)
    {
      Names = names;
    }
  }
}