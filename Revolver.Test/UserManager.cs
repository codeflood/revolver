using NUnit.Framework;
using Revolver.Core;
using Sitecore.Security.Accounts;
using Sitecore.Web.Authentication;
using System;
using System.Linq;
using System.Web.Security;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("UserManager")]
  public class UserManager : BaseCommandTest
  {
    private const string UserName = "sitecore\\testuser";

    private string _sessionId = string.Empty;

    private bool _userExists = false;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      _userExists = User.Exists(UserName);

      var user = User.Create(UserName, "password");
      _sessionId = Guid.NewGuid().ToString();
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      if (!_userExists)
        Membership.DeleteUser(UserName);
    }

    [TearDown]
    public void TearDown()
    {
      DomainAccessGuard.Kick(_sessionId);
    }

    [Test]
    public void List_UserNotLoggedIn()
    {
      var cmd = new Cmd.UserManager();
      InitCommand(cmd);

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.Not.StringContaining(UserName));
    }

    [Test]
    public void List_UserLoggedIn()
    {
      var cmd = new Cmd.UserManager();
      InitCommand(cmd);

      DomainAccessGuard.Login(_sessionId, UserName);

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success), result.Message);
      Assert.That(result.Message, Is.StringContaining(UserName), result.Message);
    }

    [Test]
    public void Kick_UserNotLoggedIn()
    {
      var cmd = new Cmd.UserManager();
      InitCommand(cmd);

      cmd.KickID = "blahblahblah";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
    }

    [Test]
    public void Kick_UserLoggedIn()
    {
      var cmd = new Cmd.UserManager();
      InitCommand(cmd);

      DomainAccessGuard.Login(_sessionId, UserName);

      cmd.KickID = _sessionId;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      var postSession = (from s in DomainAccessGuard.Sessions
                     where s.UserName == UserName
                     select s).FirstOrDefault();

      Assert.That(postSession, Is.Null);
    }
  }
}