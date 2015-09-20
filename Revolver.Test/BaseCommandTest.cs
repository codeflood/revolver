using NUnit.Framework;
using Revolver.Core.Commands;
using Revolver.Core.Formatting;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;

namespace Revolver.Test
{
  public class BaseCommandTest
  {
    private ICommand _command = null;
    private Database _database = Sitecore.Configuration.Factory.GetDatabase("web");
    protected Revolver.Core.Context _context = new Revolver.Core.Context();
    protected Item _testRoot = null;

    protected virtual void InitCommand(ICommand command)
    {
      _command = command;
      _context.CurrentDatabase = _database;
      _command.Initialise(_context, new TextOutputFormatter());
    }

    protected virtual void InitContent(Database database = null)
    {
      if (database != null)
        _database = database;

      var testRootName = "test root-" + DateUtil.IsoNow;
      _context.CurrentDatabase = _database;
      var contentNode = _context.CurrentDatabase.GetItem(Sitecore.Constants.ContentPath);

      _testRoot = contentNode.Add(testRootName, _context.CurrentDatabase.Templates[Constants.Paths.FolderTemplate]);
    }

    [TestFixtureTearDown]
    protected virtual void CleanUp()
    {
      if (_testRoot != null)
      {
        using (new SecurityDisabler())
        {
          _testRoot.Delete();
        }
      }
    }
  }
}
