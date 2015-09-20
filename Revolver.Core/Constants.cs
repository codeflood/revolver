using System;

namespace Revolver.Core
{
  public static class Constants
  {
    public const string SubcommandEnterSymbol = "-!--rev_openbracket-!--";
    public const string SubcommandExitSymbol = "-!--rev_closebracket-!--";
    public const string EscapedSubcommandSymbol = "-!--sub_command-!--";
    public const string EscapedCommandChainSymbol = "-!--sub_command_chain-!--";
    public const string EscapedTokenIndicator = "-!--token-!--";
    public const string EscapeCharacter = "\\";
    public const string SubcommandSymbol = "<";
    public const string CommandChainSymbol = ">";
    public const char SubcommandEnter = '(';
    public const char SubcommandExit = ')';
    public const string FastQueryQualifier = "fast:";
    public const string NotDefinedLiteral = "<not defined>";
    public const string ScriptCommentLineIndicator = "#";
    public const string ScriptDirectiveIndicator = "@";
    public const string LineContinuationIndicator = "-";
    public const string TokenIndicator = "$";
    public const string CommandChainedValue = "~";
    public const string Empty = "_empty_";
    public const string CookieName = "revolver-clientid";

    public static readonly string[] ReservedVariables = new[]
    {
      CommandChainedValue
    };

    [Obsolete("Use SubcommandSymbol instead")]
    public const string CommandOutputPipeSymbol = "<";

    [Obsolete("Use EscapedSubcommandSymbol instead")]
    public const string EscapedCommandOutputPipeSymbol = "---cmdoutputpipe---";

    public static class Fields
    {
      public const string Description = "description";
      public const string Usage = "usage";
      public const string Comments = "comments";
      public const string Name = "name";
      public const string Example = "example";
    }

    public static class Messages
    {
      public const string MissingRequiredParameter = "Required parameter '{0}' is missing";
    }
  }
}
