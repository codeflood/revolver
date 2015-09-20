using System.Collections.Generic;
using System.Text;

namespace Revolver.Core.Formatting
{
  /// <summary>
  /// Defines a contract for formatting input and output
  /// </summary>
  public interface ICommandFormatter
  {
    /// <summary>
    /// Prints a line to the string builder
    /// </summary>
    /// <param name="input">The line to write</param>
    /// <param name="sb">The string builder to output to</param>
    void PrintLine(string input, StringBuilder sb);

    /// <summary>
    /// Split individual lines from input
    /// </summary>
    /// <param name="input">The input to split</param>
    /// <returns>The split lines</returns>
    string[] SplitLines(string input);

    /// <summary>
    /// Join individual lines
    /// </summary>
    /// <param name="lines">The lines to join</param>
    /// <returns>The joined lines</returns>
    string JoinLines(IEnumerable<string> lines);

    /// <summary>
    /// Prints a definition to the string builder
    /// </summary>
    /// <param name="name">The name of the definition</param>
    /// <param name="leadingPadding">Leading padding to apply before the name</param>
    /// <param name="definition">The definition of the name</param>
    /// <param name="padding">The padding to use for the name</param>
    /// <param name="sb">The string builder to print the defiintion to</param>
    void PrintDefinition(string name, int leadingPadding, string definition, int padding, StringBuilder sb);

    /// <summary>
    /// Prints a definition to the string builder
    /// </summary>
    /// <param name="name">The name of the definition</param>
    /// <param name="definition">The definition of the name</param>
    /// <param name="padding">The padding to use for the name</param>
    /// <param name="sb">The string builder to print the defiintion to</param>
    void PrintDefinition(string name, string definition, int padding, StringBuilder sb);

    /// <summary>
    /// Prints a definition to the string builder
    /// </summary>
    /// <param name="name">The name of the definition</param>
    /// <param name="leadingPadding">Leading padding to apply before the name</param>
    /// <param name="definition">The definition of the name</param>
    /// <param name="sb">The string builder to print the defiintion to</param>
    void PrintDefinition(string name, int leadingPadding, string definition, StringBuilder sb);

    /// <summary>
    /// Prints a definition to the string builder
    /// </summary>
    /// <param name="name">The name of the definition</param>
    /// <param name="definition">The definition of the name</param>
    /// <param name="sb">The string builder to print the defiintion to</param>
    void PrintDefinition(string name, string definition, StringBuilder sb);

    /// <summary>
    /// Prints a definition to the string
    /// </summary>
    /// <param name="name">The name of the definition</param>
    /// <param name="definition">The definition of the name</param>
    /// <returns>The printed defination</returns>
    string PrintDefinition(string name, string definition);

    /// <summary>
    /// Prints a table to the string builder
    /// </summary>
    /// <param name="cells">The cells to print</param>
    /// <param name="columWidths">The widths to use for each column</param>
    /// <param name="sb">The string builder to print the table to</param>
    void PrintTable(string[] cells, int[] columWidths, StringBuilder sb);

    /// <summary>
    /// Generate a string from the given help details object
    /// </summary>
    /// <param name="details">The help details to print</param>
    string PrintHelp(HelpDetails details);

    /// <summary>
    /// Generate a string from the given help details object
    /// </summary>
    /// <param name="details">The help details to print</param>
    /// <param name="binding">The name the command is bound to</param>
    string PrintHelp(HelpDetails details, string binding);
  }
}