using Revolver.Core;
using Revolver.Core.Commands;
using Sitecore.StringExtensions;

namespace Revolver.SheerExtensions
{
  public class ApplicationLauncher : BaseCommand
  {
    [NumberedParameter(0, "application")]
    [Description("The name of the application to launch")]
    public string Application { get; set; }

    [NumberedParameter(1, "parameters")]
    [Optional]
    [Description("A query string like list of parameters to pass to the application")]
    public string AppParameters { get; set; }

    public override CommandResult Run()
    {
      if(string.IsNullOrEmpty(Application))
        return new CommandResult(CommandStatus.Failure, Constants.Messages.MissingRequiredParameter.FormatWith("application"));

      var coreDb = Sitecore.Configuration.Factory.GetDatabase("core");
      if (coreDb != null)
      {
        var appItem = coreDb.GetItem("/sitecore/content/Applications/" + Application);
        if (appItem != null)
        {
          if (string.IsNullOrEmpty(AppParameters))
            Sitecore.Shell.Framework.Windows.RunApplication(appItem);
          else
            Sitecore.Shell.Framework.Windows.RunApplication(appItem, AppParameters);

          return new CommandResult(CommandStatus.Success, "Launched application '" + Application + "'");
        }
        else
          return new CommandResult(CommandStatus.Failure, "Failed to locate application '" + Application + "'");
      }
      else
        return new CommandResult(CommandStatus.Failure, "Failed to locate core database");
    }

    public override string Description()
    {
      return "Launch an application inside a sheer context";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments = "Parameters are passed in the form key1=val1&key2=val2";
      details.AddExample("(content editor)");
      details.AddExample("(content editor) id={GUID}");
    }
  }
}
