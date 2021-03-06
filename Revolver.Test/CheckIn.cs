using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data.Items;
using Sitecore.Security.Accounts;
using Sitecore.Security.Authentication;
using Sitecore.SecurityModel;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("CheckIn")]
  public class CheckIn : BaseCommandTest
  {
    Cmd.CheckIn _checkIn = null;
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
      _checkIn = new Cmd.CheckIn();
      base.InitCommand(_checkIn);

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
    public void NoLock()
    {
      _context.CurrentItem = _notLockedItem;
      var result = _checkIn.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
      _notLockedItem.Reload();
      Assert.IsFalse(_notLockedItem.Locking.IsLocked());
    }

    [Test]
    public void Normal()
    {
      _context.CurrentItem = _lockedItem;
      var result = _checkIn.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      _lockedItem.Reload();
      Assert.IsFalse(_lockedItem.Locking.IsLocked());
    }

    [Test]
    public void Normal_RelativePath()
    {
      _context.CurrentItem = _testRoot;
      _checkIn.Path = _lockedItem.Name;
      var result = _checkIn.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      _lockedItem.Reload();
      Assert.IsFalse(_lockedItem.Locking.IsLocked());
    }

    [Test]
    public void Normal_AbsolutePath()
    {
      _context.CurrentItem = _context.CurrentDatabase.GetRootItem();
      _checkIn.Path = _lockedItem.Paths.FullPath;
      var result = _checkIn.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      _lockedItem.Reload();
      Assert.IsFalse(_lockedItem.Locking.IsLocked());
    }

    [Test]
    public void Normal_ByID()
    {
      _context.CurrentItem = _context.CurrentDatabase.GetRootItem();
      _checkIn.Path = _lockedItem.ID.ToString();
      var result = _checkIn.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      _lockedItem.Reload();
      Assert.IsFalse(_lockedItem.Locking.IsLocked());
    }

    [Test]
    public void LockedByOtherUser()
    {
      _context.CurrentItem = _lockedByOtherUserItem;
      var result = _checkIn.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
      _lockedByOtherUserItem.Reload();
      Assert.IsTrue(_lockedByOtherUserItem.Locking.IsLocked());
    }

    [Test]
    public void SecurityNotAllowed()
    {
      AuthenticationManager.Logout();
      _context.CurrentItem = _lockedItem;
      var result = _checkIn.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
      _lockedItem.Reload();
      Assert.IsTrue(_lockedItem.Locking.IsLocked());
    }
  }
}
