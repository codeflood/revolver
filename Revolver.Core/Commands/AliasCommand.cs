using System.Linq;
using System.Text;

namespace Revolver.Core.Commands
{
    [Command("alias")]
    public class AliasCommand : BaseCommand, IManualParseCommand
	{
		public CommandResult Run(string[] args)
		{
		  if(args.Length == 0)
			return new CommandResult(CommandStatus.Success, PrintCurrentAliases());

          if (args.Length == 1)
                return Context.CommandHandler.RemoveCommandAlias(args[0]);
            
          return Context.CommandHandler.AddCommandAlias(args[0], args[1], args.Skip(2).ToArray());
        }

        public override string Description()
        {
            return "Create, remove and list aliases for commands";
        }

        public override void Help(HelpDetails details)
        {
			details.AddParameter("name", "The name for the alias.");
			details.AddParameter("command", "The command to alias.");
			details.AddParameter("parameters", "Additional parameters to pass to the command.");

			details.Comments = Formatter.JoinLines(new[]
			{
			  "To list the current aliases, don't provide any parameters.",
			  "To remove the alias, exclude the 'command' parameter."
			});
			details.AddExample(string.Empty);
			details.AddExample("dir ls");
            details.AddExample("dir ls -a -d");
            details.AddExample("dir");
        }

        private string PrintCurrentAliases()
        {
            var sb = new StringBuilder();
            var hasAliases = false;

            var commands = Context.CommandHandler.CoreCommands.Union(Context.CommandHandler.CustomCommands);

            Formatter.PrintDefinition("Command", "Aliases", sb);
            Formatter.PrintLine(new string('-', 50), sb);

            foreach (var command in commands)
            {
                var aliases = Context.CommandHandler.GetCommandAliasesFor(command.Key);
                if (aliases.Any())
                {
                    hasAliases = true;
                    var value = string.Join(", ", aliases);
                    Formatter.PrintDefinition(command.Key, value, sb);
                }
            }

            if (!hasAliases)
                return "No aliases defined.";

            return sb.ToString();
        }
    }
}