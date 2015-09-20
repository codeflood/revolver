using System.Text;

namespace Revolver.Core
{
  /// <summary>
  /// Contains information about the result of a command execution
  /// </summary>
  public class CommandResult
  {
    #region Properties
    /// <summary>
    /// Gets the message associated with the command execution
    /// </summary>
    public string Message
    {
      get;
      protected set;
    }

    /// <summary>
    /// Gets the status of the command execution
    /// </summary>
    public CommandStatus Status
    {
      get;
      protected set;
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of the CommandResult class
    /// </summary>
    /// <param name="status">The status of the command execution</param>
    /// <param name="message">Any message associated with the command</param>
    public CommandResult(CommandStatus status, string message)
    {
      Status = status;
      Message = message;
    }
    #endregion

    #region Implicit Casts
    public static implicit operator string(CommandResult result)
    {
      return result.ToString();
    }
    #endregion

    public override string ToString()
    {
      var buffer = new StringBuilder(Message);

      switch (Status)
      {
        case CommandStatus.Failure:
          return string.Format("FAIL: {0}", buffer.ToString());
        case CommandStatus.Undetermined:
          return string.Format("WARNING: {0}", buffer.ToString());
      }

      return buffer.ToString();
    }
  }
}
