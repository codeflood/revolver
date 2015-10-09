using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revolver.Core.Formatting
{
  /// <summary>
  /// Implements a basic output formatter for textual output
  /// </summary>
  [Serializable]
  public class TextOutputFormatter : ICommandFormatter
  {
    /// <summary>The default padding to use</summary>
    private const int DefaultPadding = 20;

    /// <summary>The newline character to use for separating lines of output</summary>
    private readonly string _newLine = Environment.NewLine;

    /// <summary>The characters to use for separating lines of input</summary>
    private readonly string[] _splittingChars = new[]
    {
      Environment.NewLine,
      "\n"
    };

    /// <summary>
    /// Prints a line to the string builder
    /// </summary>
    /// <param name="input">The line to write</param>
    /// <param name="sb">The string builder to output to</param>
    public void PrintLine(string input, StringBuilder sb)
    {
      if(input != null)
        sb.Append(input);

      sb.Append(_newLine);
    }

    /// <summary>
    /// Split individual lines from input
    /// </summary>
    /// <param name="input">The input to split</param>
    /// <returns>The split lines</returns>
    public string[] SplitLines(string input)
    {
      return input.Split(_splittingChars, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Join individual lines
    /// </summary>
    /// <param name="lines">The lines to join</param>
    /// <returns>The joined lines</returns>
    public string JoinLines(IEnumerable<string> lines)
    {
      return string.Join(_newLine, lines.ToArray());
    }

    /// <summary>
    /// Prints a definition to the string
    /// </summary>
    /// <param name="name">The name of the definition</param>
    /// <param name="definition">The definition of the name</param>
    /// <returns>The printed defination</returns>
    public string PrintDefinition(string name, string definition)
    {
      StringBuilder sb = new StringBuilder();

      PrintDefinition(name, 0, definition, DefaultPadding, sb);
      return sb.ToString();
    }

    /// <summary>
    /// Prints a definition to the string builder
    /// </summary>
    /// <param name="name">The name of the definition</param>
    /// <param name="definition">The definition of the name</param>
    /// <param name="sb">The string builder to print the defiintion to</param>
    public void PrintDefinition(string name, string definition, StringBuilder sb)
    {
      PrintDefinition(name, definition, DefaultPadding, sb);
    }

    /// <summary>
    /// Prints a definition to the string builder
    /// </summary>
    /// <param name="name">The name of the definition</param>
    /// <param name="leadingPadding">Leading padding to apply before the name</param>
    /// <param name="definition">The definition of the name</param>
    /// <param name="sb">The string builder to print the defiintion to</param>
    public void PrintDefinition(string name, int leadingPadding, string definition, StringBuilder sb)
    {
      PrintDefinition(name, leadingPadding, definition, DefaultPadding, sb);
    }

    /// <summary>
    /// Prints a definition to the string builder
    /// </summary>
    /// <param name="name">The name of the definition</param>
    /// <param name="definition">The definition of the name</param>
    /// <param name="padding">The padding to use for the name</param>
    /// <param name="sb">The string builder to print the defiintion to</param>
    public void PrintDefinition(string name, string definition, int padding, StringBuilder sb)
    {
      PrintDefinition(name, 0, definition, padding, sb);
    }

    /// <summary>
    /// Prints a definition to the string builder
    /// </summary>
    /// <param name="name">The name of the definition</param>
    /// <param name="leadingPadding">Leading padding to apply before the name</param>
    /// <param name="definition">The definition of the name</param>
    /// <param name="padding">The padding to use for the name</param>
    /// <param name="sb">The string builder to print the defiintion to</param>
    public void PrintDefinition(string name, int leadingPadding, string definition, int padding, StringBuilder sb)
    {
      if (leadingPadding > 0)
        sb.Append(new string(' ', leadingPadding));

      sb.Append(name);
      int pad = padding - name.Length;
      sb.Append(new string(' ', pad > 0 ? pad : 1));
      sb.Append(definition);
      sb.Append(_newLine);
    }

    /// <summary>
    /// Prints a table to the string builder
    /// </summary>
    /// <param name="cells">The cells to print</param>
    /// <param name="columWidths">The widths to use for each column</param>
    /// <param name="sb">The string builder to print the table to</param>
    public void PrintTable(string[] cells, int[] columWidths, StringBuilder sb)
    {
      if (cells.Length != columWidths.Length)
        throw new ArgumentException("Length of columWidths and length of cells doesn't match");

      for (int i = 0; i < cells.Length; i++)
      {
        sb.Append(cells[i]);
        int pad = columWidths[i] - cells[i].Length;
        sb.Append(new string(' ', pad > 0 ? pad : 0));
      }
      sb.Append(_newLine);
    }

    /// <summary>
    /// Generate a string from the given help details object
    /// </summary>
    /// <param name="details">The help details to print</param>
    public string PrintHelp(HelpDetails details)
    {
      return PrintHelp(details, string.Empty);
    }

    /// <summary>
    /// Generate a string from the given help details object
    /// </summary>
    /// <param name="details">The help details to print</param>
    /// <param name="binding">The name the command is bound to</param>
    public string PrintHelp(HelpDetails details, string binding)
    {
      StringBuilder sb = new StringBuilder(300);

      sb.Append(details.Description);
      sb.Append(_newLine);
      sb.Append(_newLine);
      sb.Append("Usage: ");
      sb.Append(_newLine);
      sb.Append("  ");
      sb.Append(details.Usage);
      sb.Append(_newLine);

      if (details.Parameters.Count > 0)
      {
        sb.Append(_newLine);
        sb.Append("Parameters:");
        sb.Append(_newLine);

        for (int i = 0; i < details.Parameters.Count; i++)
        {
          PrintDefinition(details.Parameters.Keys[i], 2, details.Parameters[i], sb);
        }
      }

      if (!string.IsNullOrEmpty(details.Comments))
      {
        sb.Append(_newLine);
        sb.Append("Comments:");
        sb.Append(_newLine);
        sb.Append("  ");
        sb.Append(details.Comments);
        sb.Append(_newLine);
      }

      if (details.Examples.Count > 0)
      {
        sb.Append(_newLine);
        sb.Append("Examples:");
        sb.Append(_newLine);

        for (int i = 0; i < details.Examples.Count; i++)
        {
          sb.Append("  ");
          sb.Append((string.IsNullOrEmpty(binding) ? string.Empty : binding + " ") + details.Examples[i]);
          sb.Append(_newLine);
        }
      }

      return sb.ToString();
    }
  }
}