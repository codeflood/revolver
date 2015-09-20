using Revolver.Core.Formatting;

namespace Revolver.Core.Commands
{
  public interface ICommand
  {
    void Initialise(Context context, ICommandFormatter formatContext);
    CommandResult Run();
    string Description();
    void Help(HelpDetails details);
  }
}
