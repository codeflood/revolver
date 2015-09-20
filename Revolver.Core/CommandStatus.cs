
namespace Revolver.Core
{
    /// <summary>
    /// Allowable values for the result of a command execution
    /// </summary>
    public enum CommandStatus
    {
        /// <summary>
        /// Indicates the command was successful
        /// </summary>
        Success,

        /// <summary>
        /// Indicates the command failed
        /// </summary>
        Failure,

        /// <summary>
        /// Indicates the status has not been set
        /// </summary>
        Undetermined,

        /// <summary>
        /// Instructs the contxet to abort
        /// </summary>
        Abort
    }
}
