using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Revolver.Core.Formatting;

namespace Revolver.Core
{
  // This class is only temporary and will undergo many updates soon. Users should avoid direct interaction with this class.
  internal class Parser
  {
    /// <summary>
    /// Break the input into separate elements
    /// </summary>
    /// <param name="line">The input line</param>
    /// <returns>The command elements</returns>
    public static string[] ParseInputLine(string line)
    {
      var buffer = new StringBuilder();

      // Clean the line of excess whitespace
      buffer.Append(line.Trim());

      // Convert \n to \r\n for consistency. \n may be used by some browsers or if line endings in a script are not Windows style
      buffer.Replace("\n", "\r\n");

      // Replace escaped quotes with a symbol
      buffer.Replace(Constants.EscapeCharacter + Constants.SubcommandEnter, Constants.SubcommandEnterSymbol);
      buffer.Replace(Constants.EscapeCharacter + Constants.SubcommandExit, Constants.SubcommandExitSymbol);

      // replace escaped command output pipes
      buffer.Replace(Constants.EscapeCharacter + Constants.SubcommandSymbol, Constants.EscapedSubcommandSymbol);
      buffer.Replace(Constants.EscapeCharacter + Constants.CommandChainSymbol, Constants.EscapedCommandChainSymbol);

      string[] groups = ParseFirstLevelGroups(buffer.ToString(), Constants.SubcommandEnter, Constants.SubcommandExit);

      // Substitute escaped characters back in
      for (int i = 0; i < groups.Length; i++)
      {
        if (!groups[i].Contains(Constants.SubcommandEnter.ToString()))
          groups[i] = groups[i].Replace(Constants.SubcommandEnterSymbol, Constants.SubcommandEnter.ToString());

        if (!groups[i].Contains(Constants.SubcommandExit.ToString()))
          groups[i] = groups[i].Replace(Constants.SubcommandExitSymbol, Constants.SubcommandExit.ToString());
      }

      return groups;
    }

    /// <summary>
    /// Parse groups out of an input string based on the opening and closing characters
    /// </summary>
    /// <param name="input">The input to parse</param>
    /// <param name="openingChar">The opening character of a grouping</param>
    /// <param name="closingChar">The closing character of a grouping</param>
    /// <returns>The complete first level groups found</returns>
    public static string[] ParseFirstLevelGroups(string input, char openingChar, char closingChar)
    {
      StringCollection groups = new StringCollection();
      char[] delimiters = new char[] { openingChar, closingChar };
      int ind = input.IndexOfAny(delimiters);
      if (ind >= 0)
        groups.AddRange(input.Substring(0, ind).Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

      int count = 0;
      int start = ind;
      while (ind >= 0)
      {
        if (input[ind] == openingChar)
          count++;
        else if (input[ind] == closingChar)
          count--;

        if (count == 0)
        {
          string chunk = input.Substring(start + 1, ind - start - 1).Trim();
          //chunk = chunk.Replace(Constants.SubcommandEnterSymbol, Constants.EscapeCharacter + Constants.SubcommandEnter);
          //chunk = chunk.Replace(Constants.SubcommandExitSymbol, Constants.EscapeCharacter + Constants.SubcommandExit);
          groups.Add(chunk);
          start = ind + 1;
        }

        if (start + 1 < input.Length)
        {
          ind = input.IndexOfAny(delimiters, ind + 1);
          if (ind >= 0 && count == 0)
          {
            groups.AddRange(input.Substring(start + 1, ind - start - 1).Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            start = ind;
          }
        }
        else
          break;
      }

      if (start + 1 < input.Length)
        groups.AddRange(input.Substring(start + 1, input.Length - start - 1).Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

      string[] output = new string[groups.Count];
      groups.CopyTo(output, 0);
      return output;
    }

    /// <summary>
    /// Substitute tokens in the input string such as environment variables
    /// </summary>
    /// <param name="context">The context to use during substitution</param>
    /// <param name="input">The input to perform substitution on</param>
    /// <returns>A string with tokens replaced</returns>
    public static string PerformSubstitution(Context context, string input)
    {
      // Substitute environment variables
      foreach (string key in context.EnvironmentVariables.Keys)
      {
        if (input.Contains(Constants.TokenIndicator + key + Constants.TokenIndicator))
          input = input.Replace(Constants.TokenIndicator + key + Constants.TokenIndicator, context.EnvironmentVariables[key]);
      }

      // Replace escaped tokens
      input = input.Replace(Constants.EscapeCharacter + Constants.TokenIndicator, Constants.TokenIndicator);

      return input;
    }

    public static bool TryParseBoolean(string value, out bool boolean)
    {
      var lvalue = value.ToLower();
      boolean = lvalue == "true" || lvalue == "yes" || lvalue == "y" || lvalue == "1";

      if (boolean)
        return true;

      if (lvalue == "false" || lvalue == "no" || lvalue == "n" || lvalue == "0" || string.IsNullOrEmpty(lvalue))
        return true;

      return false;
    }

    public static string[] ParseScriptLines(string scriptSource, ICommandFormatter formatter)
    {
      var rawLines = new List<string>(formatter.SplitLines(scriptSource));

        for (var i = rawLines.Count - 1; i >= 0; i--)
        {
            var line = rawLines[i];
            if (line.EndsWith(Constants.LineContinuationIndicator))
            {
                if (line.EndsWith(Constants.EscapeCharacter + Constants.LineContinuationIndicator))
                {
                    var length = line.Length - (Constants.EscapeCharacter + Constants.LineContinuationIndicator).Length;
                    rawLines[i] = line.Substring(0, length) + Constants.LineContinuationIndicator;
                }
                else if (i < rawLines.Count - 1)
                {
                    rawLines[i] = line.Substring(0, line.Length - Constants.LineContinuationIndicator.Length) + rawLines[i + 1];
                    rawLines.RemoveAt(i + 1);
                }
            }
        }

        return rawLines.ToArray();
    }
  }
}