using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.StringExtensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Revolver.Core.Formatting;

namespace Revolver.Core
{
  [Serializable]
  public class Context
  {
    private ItemUri _currentItemUri = null;
    private string _lastGoodPath = string.Empty;
    private StringDictionary _envVars = null;
    private Dictionary<string, string> _customCommands = null;
    
    [NonSerialized]
    private CommandHandler _commandHandler = null;

    private Stack<ItemUri> _pathStack = new Stack<ItemUri>();

    public Item CurrentItem
    {
      get
      {
        Item item = null;

        if (_currentItemUri != null)
        {
          // todo: check performance of below with lots of versions.

          item = Database.GetItem(_currentItemUri);
          if (item == null || !item.Versions.GetVersionNumbers().Select(x => x.Number).Contains(item.Version.Number))
            // The version may have been deleted, try to get the latest version
            item = CurrentDatabase.GetItem(_currentItemUri.ItemID, _currentItemUri.Language);

          if (item == null)
            // The item may have been deleted, try to get the parent
            item = CurrentDatabase.GetItem(_lastGoodPath.Substring(0, _lastGoodPath.LastIndexOf("/")));
        }

        return item;
      }
      set
      {
        if (value != null)
        {
          _lastGoodPath = value.Paths.FullPath;
          _currentItemUri = value.Uri;
        }
      }
    }

    public Database CurrentDatabase
    {
      get
      {
        if (_currentItemUri != null)
          return Factory.GetDatabase(_currentItemUri.DatabaseName);

        return null;
      }
      set
      {
        if (value == null)
          return;

        var language = Sitecore.Context.Language;
        if (_currentItemUri != null)
          language = _currentItemUri.Language;

        _currentItemUri = value.GetRootItem(language).Uri;
      }
    }

    public Language CurrentLanguage
    {
      get
      {
        if (_currentItemUri != null)
          return _currentItemUri.Language;

        return null;
      }
      set
      {
        _currentItemUri = new ItemUri(_currentItemUri.ItemID, value, Sitecore.Data.Version.Latest, _currentItemUri.DatabaseName);
      }
    }

    /// <summary>
    /// Gets the last path that was valid that the context was on
    /// </summary>
    public string LastGoodPath
    {
      get { return _lastGoodPath; }
    }

    public CommandHandler CommandHandler
    {
      get { return _commandHandler; }
      set { _commandHandler = value; }
    }

    public StringDictionary EnvironmentVariables
    {
      get { return _envVars; }
    }

    public Dictionary<string, string> CustomCommands
    {
      get { return _customCommands; }
      set { _customCommands = value; }
    }

    public Context()
    {
      if (Sitecore.Context.Site == null)
        Sitecore.Context.SetActiveSite(Sitecore.Constants.ShellSiteName);

      var db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
      _currentItemUri = db.GetRootItem(Sitecore.Context.Language).Uri;

      // Initialise environment variables dictionary
      _envVars = new StringDictionary();
      _envVars.Add("prompt", "sc >");
      _envVars.Add("outputbuffer", "1000000");

      _customCommands = new Dictionary<string, string>();
    }

    /// <summary>
    /// Create a copy of this context
    /// </summary>
    /// <returns></returns>
    public Context Clone()
    {
      return new Context()
      {
        _commandHandler = _commandHandler,
        _currentItemUri = _currentItemUri,
        _envVars = _envVars,
        _lastGoodPath = _lastGoodPath,
        _pathStack = _pathStack
      };
    }

    /// <summary>
    /// Save the current context (database and item) onto the stack.
    /// </summary>
    public void PushContext()
    {
      _pathStack.Push(_currentItemUri);
    }

    /// <summary>
    /// Revert the context back to the previous database and item
    /// </summary>
    public void Revert()
    {
      if (_pathStack.Count > 0)
      {
        _currentItemUri = _pathStack.Pop();
      }
    }

    public CommandResult ExecuteCommand(string commandLine, ICommandFormatter formatter)
    {
      if (_commandHandler == null)
        _commandHandler = new CommandHandler(this, formatter);

      return _commandHandler.Execute(commandLine);
    }

    /// <summary>
    /// Set the given context to the given path
    /// </summary>
    /// <param name="path">The path to set the context from</param>
    /// <param name="dbName">The databse to change to</param>
    /// <param name="language">The language to change context to</param>
    /// <param name="versionNumber">The version number to change context to</param>
    /// <returns>A CommandResult indicating the status of the command</returns>
    public CommandResult SetContext(string path, string dbName = null, Language language = null, int? versionNumber = null)
    {
      PushContext();
      string workingPath = path;
      // Determine if we're changing dbs.
      if (string.IsNullOrEmpty(dbName))
        dbName = CurrentDatabase.Name;
      if (path.StartsWith("/") && path.Length > 1)
      {
        int ind = path.IndexOf('/', 2);
        string pathDBName = string.Empty;

        if (ind > 0)
          pathDBName = path.Substring(1, ind - 1);
        else
          pathDBName = path.Substring(1);

        // Loop through available dbs and see if we have a name match
        string[] avNames = Factory.GetDatabaseNames();
        for (int i = 0; i < avNames.Length; i++)
        {
          if (string.Compare(avNames[i], pathDBName, true) == 0)
          {
            dbName = avNames[i];
          }
        }
      }

      Database db = null;
      //bool requireRevert = false;
      if (CurrentDatabase.Name != dbName)
      {
        try
        {
          db = Factory.GetDatabase(dbName, true);
        }
        catch(InvalidOperationException)
        {
          // Couldn't find DB
          db = null;
        }

        if (db == null)
        {
          Revert();
          return new CommandResult(CommandStatus.Failure, "Failed to find database '{0}'".FormatWith(dbName));
        }
        else
          CurrentDatabase = db;

        //requireRevert = true;
      }

      if (workingPath.StartsWith("/" + dbName))
      {
        int ind = path.IndexOf('/', 2);
        if (ind >= 0)
          workingPath = path.Substring(ind);
        else
          workingPath = "/";
      }

      string itemPath = string.Empty;
      int index = -1;

      if (workingPath.StartsWith("{"))
        itemPath = workingPath;
      else
      {
        itemPath = PathParser.EvaluatePath(this, workingPath);

        // Extract indexer if present
        if (itemPath.Contains("["))
        {
          int startIndex = itemPath.IndexOf("[");
          int endIndex = itemPath.LastIndexOf("]");

          if ((endIndex > startIndex) && (endIndex < itemPath.Length))
          {
            string indexerString = itemPath.Substring(startIndex + 1, itemPath.Length - endIndex);
            if (int.TryParse(indexerString, out index))
            {
              itemPath = itemPath.Substring(0, startIndex);
            }

            if (index < 0)
            {
              Revert();
              return new CommandResult(CommandStatus.Failure, "Index must be non-negative");
            }
          }
        }
      }

      string parsedLanguage = PathParser.ParseLanguageFromPath(workingPath);
      if (string.IsNullOrEmpty(parsedLanguage))
      {
        if (language != null)
          parsedLanguage = language.Name;
      }

      string version = PathParser.ParseVersionFromPath(workingPath);
      if (string.IsNullOrEmpty(version))
      {
        if (versionNumber != null)
          version = versionNumber.ToString();
      }

      Sitecore.Globalization.Language targetLanguage = null;
      Sitecore.Data.Version targetVersion = null;

      if (parsedLanguage != string.Empty)
      {
        if (!Sitecore.Globalization.Language.TryParse(parsedLanguage, out targetLanguage))
        {
          Revert();
          return new CommandResult(CommandStatus.Failure, "Failed to parse language '{0}'".FormatWith(language));
        }
      }
      else
        targetLanguage = CurrentLanguage;

      if (!string.IsNullOrEmpty(version))
      {
        // if version contains a negative number, take it as latest version minus that number
        var parsedVersion = Sitecore.Data.Version.Latest.Number;

        //if (!Sitecore.Data.Version.TryParse(version, out targetVersion))
        if (int.TryParse(version, out parsedVersion))
        {
          if (parsedVersion < 0)
          {
            var versionedItem = CurrentDatabase.GetItem(itemPath, targetLanguage);
            if (versionedItem != null)
            {
              var versions = versionedItem.Versions.GetVersionNumbers();

              var targetVersionIndex = versions.Length + parsedVersion - 1;

              if (targetVersionIndex < 0 || targetVersionIndex > versions.Length - 1)
              {
                Revert();
                return new CommandResult(CommandStatus.Failure, "Version index greater than number of versions.");
              }

              targetVersion = versions[targetVersionIndex];
            }
          }
          else
            targetVersion = Sitecore.Data.Version.Parse(parsedVersion);
        }
        else
        {
          Revert();
          return new CommandResult(CommandStatus.Failure, "Failed to parse version '{0}'".FormatWith(version));
        }
      }
      else
        targetVersion = Sitecore.Data.Version.Latest;

      var item = CurrentDatabase.GetItem(itemPath, targetLanguage, targetVersion);

      if (item == null)
      {
        Revert();
        return new CommandResult(CommandStatus.Failure, "Failed to find item '{0}'".FormatWith(itemPath));
      }
      else
      {
        // If indexer was specified, swap to indexed item
        if (index >= 0)
        {
          if (itemPath.EndsWith("/"))
          {
            Sitecore.Collections.ChildList children = item.GetChildren();

            if (index < children.Count)
              item = children[index];
            else
            {
              Revert();
              return new CommandResult(CommandStatus.Failure, "Index greater than child count");
            }
          }
          else
          {
            Sitecore.Collections.ChildList children = item.Parent.GetChildren();

            int count = 0;
            foreach (Item child in children)
            {
              if (child.Name == item.Name)
                count++;

              if (count == (index + 1))
              {
                item = child;
                break;
              }
            }

            if (count <= index)
            {
              Revert();
              return new CommandResult(CommandStatus.Failure, "Index greater than named child count");
            }
          }
        }

        CurrentItem = item;
      }

      return new CommandResult(CommandStatus.Success, string.Empty);
    }
  }
}
