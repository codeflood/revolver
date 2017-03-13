using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revolver.Core.Commands
{
    [Command("alias")]
    public class AliasCommand : BaseCommand
    {
        [NumberedParameter(0, "name")]
        [Description("The name for the alias.")]
        public string Name { get; set; }

        [NumberedParameter(1, "command")]
        [Description("The command to alias.")]
        [Optional]
        public string Command { get; set; }

        [ListParameter("parameters")]
        [Description("Additional parameters to pass to the command.")]
        [Optional]
        public IList<string> Parameters { get; set; }

        public override CommandResult Run()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return new CommandResult(CommandStatus.Success, PrintCurrentAliases());

            if (string.IsNullOrWhiteSpace(Command))
                return Context.CommandHandler.RemoveCommandAlias(Name);
            
            return Context.CommandHandler.AddCommandAlias(Name, Command, Parameters != null ? Parameters.ToArray() : null);
        }

        public override string Description()
        {
            return "Create and remove aliases for commands";
        }

        public override void Help(HelpDetails details)
        {
            details.Comments = "To remove the alias, exclude the 'command' parameter.";
            details.AddExample("dir ls");
            details.AddExample("dir ls -a -d");
            details.AddExample("dir");
        }

        private string PrintCurrentAliases()
        {
            var sb = new StringBuilder();
            var hasAliases = false;

            if (string.IsNullOrWhiteSpace(Name))
            {
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
            }

            if (!hasAliases)
                return "No aliases defined.";

            return sb.ToString();
        }
    }
}