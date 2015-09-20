namespace Revolver.Core
{
  /// <summary>
  /// Represents the arguments for a command
  /// </summary>
  public class CommandArgs
  {
    /// <summary>
    /// Gets or sets the command name.
    /// </summary>
    public string CommandName { get; set; }

    /// <summary>
    /// Gets or sets the parameters for the command.
    /// </summary>
    public string[] Parameters { get; set; }

    public CommandArgs(string commandName, string[] parameters)
    {
      CommandName = commandName;
      Parameters = parameters;
    }
  }
}