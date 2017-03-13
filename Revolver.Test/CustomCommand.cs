using Revolver.Core;
using Revolver.Core.Commands;
using Revolver.Core.Formatting;

namespace Revolver.Test
{
    [Command("cc")]
    internal class CustomCommand
    {
        public string Description()
        {
            return "A custom command";
        }

        public void Help(Core.HelpDetails details)
        {
            details.Description = Description();
        }

        public void Initialise(Core.Context context, ICommandFormatter formatter)
        {
        }

        public Core.CommandResult Run()
        {
            return new CommandResult(CommandStatus.Success, "boo");
        }
    }
}