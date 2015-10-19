using System.Linq;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("wfc")]
  public class WorkflowCommand : BaseCommand
  {
    [NumberedParameter(0, "command")]
    [Description("The command to execute.")]
    public string Command { get; set; }

    [NumberedParameter(0, "comment")]
    [Description("The workflow change state comment.")]
    [Optional]
    public string Comment { get; set; }

    public WorkflowCommand()
    {
      Command = string.Empty;
      Comment = string.Empty;
    }

    public override CommandResult Run()
    {
      if (Context.CurrentDatabase.WorkflowProvider == null)
        return new CommandResult(CommandStatus.Undetermined, "Command cannot execute. No workflow provider configured for current database");

      if (!string.IsNullOrEmpty(Command))
        return ExecuteCommand(Command, Comment);
      else
        return ListCommands();
    }

    public override string Description()
    {
      return "List and execute workflow commands";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments = "If 'command' is not provided the available commands will be listed";
      details.AddExample("submit");
    }

    public CommandResult ExecuteCommand(string name, string comment = "")
    {
      var provider = Context.CurrentDatabase.WorkflowProvider;
      if (provider == null)
        return new CommandResult(CommandStatus.Failure, "Command cannot execute. No workflow provider configured for current database");

      var item = Context.CurrentItem;

      var workflow = provider.GetWorkflow(item);
      if (workflow == null)
        return new CommandResult(CommandStatus.Failure, "The item is not in workflow");

      var command = (from c in workflow.GetCommands(item) 
                     let ci = Context.CurrentDatabase.GetItem(c.CommandID)
                     where string.Compare(ci.Name, name, true) == 0 select c).FirstOrDefault();

      if (command == null)
        return new CommandResult(CommandStatus.Failure, "Workflow command not found");

      var commandExecuteResult = workflow.Execute(command.CommandID, item, comment, false);
      var status = commandExecuteResult.Succeeded ? CommandStatus.Success : CommandStatus.Failure;
      var message = "Command executed";
      if (!string.IsNullOrEmpty(commandExecuteResult.Message))
        message += ": " + commandExecuteResult.Message;

      return new CommandResult(status, message);
    }

    public CommandResult ListCommands()
    {
      var provider = Context.CurrentDatabase.WorkflowProvider;
      if (provider == null)
        return new CommandResult(CommandStatus.Failure, "Command cannot execute. No workflow provider configured for current database");

      var item = Context.CurrentItem;

      var workflow = provider.GetWorkflow(item);
      if (workflow == null)
        return new CommandResult(CommandStatus.Success, "No commands available; no workflow for item");

      var commands = workflow.GetCommands(item);

      var buffer = new StringBuilder();

      foreach (var command in commands)
      {
        var commandItem = Context.CurrentDatabase.GetItem(command.CommandID);

        Formatter.PrintDefinition(commandItem.Name, "[" + command.DisplayName + "]", buffer);
      }

      if (commands.Length == 0)
        buffer.AppendLine("No commands available");

      return new CommandResult(CommandStatus.Success, buffer.ToString());
    }
  }
}