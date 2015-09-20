using Revolver.Core.Formatting;

namespace Revolver.Core.Commands
{
  public abstract class BaseCommand : ICommand
  {
    protected Context Context
    {
      get;
      private set;
    }

    protected ICommandFormatter Formatter
    {
      get;
      private set;
    }

    public virtual void Initialise(Context context, ICommandFormatter formatter)
    {
      Context = context;
      Formatter = formatter;
    }

    public virtual CommandResult Run()
    {
      return new CommandResult(CommandStatus.Undetermined, "Not Implemented");
    }

    public abstract string Description();
    public abstract void Help(HelpDetails details);
  }
}
