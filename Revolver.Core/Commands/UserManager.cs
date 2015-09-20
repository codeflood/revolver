using Sitecore.Web.Authentication;
using System.Collections.Generic;
using System.Text;

namespace Revolver.Core.Commands
{
  [Command("users")]
  public class UserManager : BaseCommand
  {
    [NamedParameter("k", "id")]
    [Description("ID of session to kick.")]
    public string KickID { get; set; }

    public UserManager()
    {
      KickID = string.Empty;
    }

    public override CommandResult Run()
    {
      if (string.IsNullOrEmpty(KickID))
        return ListUsers();
      else
        return KickUser(KickID);
    }

    public override string Description()
    {
      return "List and kick logged on users";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments = "If no parameters are provided the current list of logged on users will be displayed";
      details.AddExample("-k wezgix55fvzkoq45ualyvj45");
    }

    private CommandResult ListUsers()
    {
      var output = new StringBuilder();
      List<DomainAccessGuard.Session> sessions = DomainAccessGuard.Sessions;
      if (sessions != null)
      {
        PrintTable(output, "SessionID", "UserName", "LastRequest");
        PrintTable(output, "---------", "--------", "-----------");

        foreach (var session in sessions)
        {
          PrintTable(output, session.SessionID, session.UserName, session.LastRequest.ToString());
        }

        return new CommandResult(CommandStatus.Success, output.ToString());
      }

      return new CommandResult(CommandStatus.Failure, "Failed to retrieve user sessions");
    }

    private CommandResult KickUser(string id)
    {
      DomainAccessGuard.Kick(id);
      return new CommandResult(CommandStatus.Success, "Session kicked");
    }

    private void PrintTable(StringBuilder output, string sessionId, string username, string lastRequest)
    {
      Formatter.PrintTable(new[] { sessionId, username, lastRequest }, new[] { 27, 30, 30 }, output);
    }
  }
}
