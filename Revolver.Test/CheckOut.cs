using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data.Items;
using Sitecore.Security.AccessControl;
using Sitecore.Security.Accounts;
using Sitecore.Security.Authentication;
using Sitecore.SecurityModel;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("CheckOut")]
  public class CheckOut : BaseCommandTest
  {
    Cmd.CheckOut _checkOut = null;
    Item _notLockedItem = null;
    Item _lockedItem = null;
    Item _lockedByOtherUserItem = null;
    User _currentUser = null;
    User _otherUser = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = false;

      // setup content
      using (new SecurityDisabler())
      {
        InitContent();
        _notLockedItem = _testRoot.Add("not locked item", _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate]);
        _lockedItem = _testRoot.Add("locked item", _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate]);
        _lockedByOtherUserItem = _testRoot.Add("locked item by other user", _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate]);
      }

      // setup users      
      _currentUser = User.Create("sitecore\\currentuser", "abcd");
      _otherUser = User.Create("sitecore\\otheruser", "abcd");

      // set permissions
      var accessRules = _notLockedItem.Security.GetAccessRules();
      accessRules.Add(AccessRule.Create(_currentUser, AccessRight.ItemWrite, PropagationType.Any, AccessPermission.Allow));
      accessRules.Add(AccessRule.Create(_otherUser, AccessRight.ItemWrite, PropagationType.Any, AccessPermission.Allow));
      _notLockedItem.Security.SetAccessRules(accessRules);

      accessRules = _lockedItem.Security.GetAccessRules();
      accessRules.Add(AccessRule.Create(_currentUser, AccessRight.ItemWrite, PropagationType.Any, AccessPermission.Allow));
      accessRules.Add(AccessRule.Create(_otherUser, AccessRight.ItemWrite, PropagationType.Any, AccessPermission.Allow));
      _lockedItem.Security.SetAccessRules(accessRules);

      accessRules = _lockedByOtherUserItem.Security.GetAccessRules();
      accessRules.Add(AccessRule.Create(_currentUser, AccessRight.ItemWrite, PropagationType.Any, AccessPermission.Allow));
      accessRules.Add(AccessRule.Create(_otherUser, AccessRight.ItemWrite, PropagationType.Any, AccessPermission.Allow));
      _lockedByOtherUserItem.Security.SetAccessRules(accessRules);
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      CleanUp();
      System.Web.Security.Membership.DeleteUser(_currentUser.Name);
      System.Web.Security.Membership.DeleteUser(_otherUser.Name);
    }

    [SetUp]
    public void SetUp()
    {
      _checkOut = new Cmd.CheckOut();
      base.InitCommand(_checkOut);

      using (new SecurityDisabler())
      {
        _notLockedItem.Editing.BeginEdit();
        _notLockedItem.Locking.Unlock();
        _notLockedItem.Editing.EndEdit();

        AuthenticationManager.Login(_otherUser);
        _lockedByOtherUserItem.Editing.BeginEdit();
        _lockedByOtherUserItem.Locking.Lock();
        _lockedByOtherUserItem.Editing.EndEdit();
        AuthenticationManager.Logout();

        AuthenticationManager.Login(_currentUser);
        _lockedItem.Editing.BeginEdit();
        _lockedItem.Locking.Lock();
        _lockedItem.Editing.EndEdit();
        // don't log this user out, tests should be run as this user
      }
    }

    [TearDown]
    public void TearDown()
    {
      AuthenticationManager.Logout();
    }

    [Test]
    public void Normal()
    {
      _context.CurrentItem = _notLockedItem;
      var result = _checkOut.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      _notLockedItem.Reload();
      Assert.IsTrue(_notLockedItem.Locking.IsLocked());
    }

    [Test]
    public void AlreadyLocked()
    {
      _context.CurrentItem = _lockedItem;
      var result = _checkOut.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      _lockedItem.Reload();
      Assert.IsTrue(_lockedItem.Locking.IsLocked());
    }

    [Test]
    public void Normal_RelativePath()
    {
      _context.CurrentItem = _testRoot;
      _checkOut.Path = _notLockedItem.Name;
      var result = _checkOut.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      _notLockedItem.Reload();
      Assert.IsTrue(_notLockedItem.Locking.IsLocked());
    }

    [Test]
    public void Normal_AbsolutePath()
    {
      _context.CurrentItem = _context.CurrentDatabase.GetRootItem();
      _checkOut.Path = _notLockedItem.Paths.FullPath;
      var result = _checkOut.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      _notLockedItem.Reload();
      Assert.IsTrue(_notLockedItem.Locking.IsLocked());
    }

    [Test]
    public void Normal_ByID()
    {
      _context.CurrentItem = _context.CurrentDatabase.GetRootItem();
      _checkOut.Path = _notLockedItem.ID.ToString();
      var result = _checkOut.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      _notLockedItem.Reload();
      Assert.IsTrue(_notLockedItem.Locking.IsLocked());
    }

    [Test]
    public void LockedByOtherUser()
    {
      _context.CurrentItem = _lockedByOtherUserItem;
      var result = _checkOut.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
      _lockedByOtherUserItem.Reload();
      Assert.IsTrue(_lockedByOtherUserItem.Locking.IsLocked());
    }

    [Test]
    public void SecurityNotAllowed()
    {
      AuthenticationManager.Logout();
      _context.CurrentItem = _notLockedItem;
      var result = _checkOut.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
      _notLockedItem.Reload();
      Assert.IsFalse(_notLockedItem.Locking.IsLocked());
    }
  }
}
