namespace Revolver.Core
{
  public class ExecutionDirective
  {
    /// <summary>
    /// Instructs the script to not show output
    /// </summary>
    public bool? EchoOff
    {
      get;
      set;
    }

    /// <summary>
    /// Determines if the script should stop if an error is detected during execution
    /// </summary>
    public bool? StopOnError
    {
      get;
      set;
    }

    /// <summary>
    /// If true, don't error when commands and scripts are unknown
    /// </summary>
    public bool? IgnoreUnknownCommands
    {
      get;
      set;
    }

    public ExecutionDirective()
    {
      EchoOff = null;
      StopOnError = null;
      IgnoreUnknownCommands = null;
    }

    /// <summary>
    /// Parse a directive from a string
    /// </summary>
    /// <param name="directive">The string to parse</param>
    /// <returns>The parsed directive if recognised</returns>
    public static ExecutionDirective Parse(string directive)
    {
      ExecutionDirective output = new ExecutionDirective();
      string loweredDirective = directive.ToLower();

      switch (loweredDirective)
      {
        case "echooff":
          output.EchoOff = true;
          break;

        case "echoon":
          output.EchoOff = false;
          break;

        case "stoponerror":
          output.StopOnError = true;
          break;

		case "continueonerror":
		  output.StopOnError = false;
		  break;

		case "ignoreunknowncommands":
          output.IgnoreUnknownCommands = true;
          break;
      }

      return output;
    }

    /// <summary>
    /// Determine if the provided directive if empty (no properties true)
    /// </summary>
    /// <param name="directive">The directive to check</param>
    /// <returns>True if all properties are their default value</returns>
    public bool IsEmpty()
    {
      return (EchoOff == null) && (StopOnError == null) && (IgnoreUnknownCommands == null);
    }

    /// <summary>
    /// Patch an ExecutionDirective with another
    /// </summary>
    /// <param name="original">The directive to patch</param>
    /// <param name="patch">The patch to apply</param>
    /// <returns>The patched directive</returns>
    public void Patch(ExecutionDirective patch)
    {
      if (patch.EchoOff != null)
        EchoOff = patch.EchoOff;

      if (patch.StopOnError != null)
        StopOnError = patch.StopOnError;

      if (patch.IgnoreUnknownCommands != null)
        IgnoreUnknownCommands = patch.IgnoreUnknownCommands;
    }

    /// <summary>
    /// Gets the default execution directive
    /// </summary>
    /// <returns>The default execution directive</returns>
    public static ExecutionDirective GetDefault()
    {
      return new ExecutionDirective()
      {
        StopOnError = true,
        EchoOff = false,
        IgnoreUnknownCommands = false
      };
    }
  }
}
