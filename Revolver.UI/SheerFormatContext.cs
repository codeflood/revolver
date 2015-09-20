using Revolver.Core;
using Revolver.Core.Formatting;
using System;

namespace Revolver.UI
{
  [Serializable]
  [Obsolete("Use Revolver.Core.Formatting.TextOutputFormatter instead")]
  public class SheerFormatContext : TextOutputFormatter, IFormatContext
  {
    public string[] NewLines
    {
      get { return new[] {"\r\n", "\r"}; }
    }
  }
}
