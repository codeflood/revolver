using Revolver.Core.Exceptions;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Revolver.Core.ScriptLocator
{
  [Serializable]
  public class DatabaseScriptLocator : IScriptLocator
  {
    public const string ScriptRootPath = "/sitecore/system/modules/revolver/scripts";

    protected readonly string _databaseName = string.Empty;
    protected readonly string _path = string.Empty;

    public DatabaseScriptLocator(string database, string path)
    {
      _databaseName = database;
      _path = path;
    }

    public string GetScript(string name)
    {
      var scriptsItems = FindScriptItems(name);

      if (scriptsItems == null)
        return null;

      if (scriptsItems.Length == 1)
        return scriptsItems[0]["Script"];

      if (scriptsItems.Length > 1)
      {
        var scriptPaths = from script in scriptsItems
                          select script.Paths.FullPath;

        throw new MultipleScriptsFoundException(scriptPaths);
      }

      return null;
    }

    public HelpDetails GetScriptHelp(string name)
    {
      var scriptsItems = FindScriptItems(name);

      if (scriptsItems == null)
        return null;

      if (scriptsItems.Length == 1)
      {
        var scriptItem = scriptsItems[0];
        HelpDetails details = new HelpDetails();

        if (scriptItem[Constants.Fields.Description] != string.Empty)
          details.Description = scriptItem[Constants.Fields.Description];

        if (scriptItem[Constants.Fields.Usage] != string.Empty)
          details.Usage = scriptItem[Constants.Fields.Usage];

        if (scriptItem[Constants.Fields.Comments] != string.Empty)
          details.Comments = scriptItem[Constants.Fields.Comments];

        Item[] parameters = scriptItem.Axes.SelectItems("*[@@templatekey='script help parameter']");
        if (parameters != null)
        {
          for (int i = 0; i < parameters.Length; i++)
          {
            details.AddParameter(parameters[i][Constants.Fields.Name], parameters[i][Constants.Fields.Description]);
          }
        }

        Item[] examples = scriptItem.Axes.SelectItems("*[@@templatekey='script help example']");
        if (examples != null)
        {
          for (int i = 0; i < examples.Length; i++)
          {
            details.AddExample(examples[i][Constants.Fields.Example]);
          }
        }

        return details;
      }

      if (scriptsItems.Length > 1)
      {
        var scriptPaths = from script in scriptsItems
                          select script.Paths.FullPath;

        throw new MultipleScriptsFoundException(scriptPaths);
      }

      return null;
    }

    public IEnumerable<string> GetScriptNames()
    {
      var scriptsItems = FindScriptItems(null);
      return from item in scriptsItems
        select item.Key;
    }

    protected Item[] FindScriptItems(string name)
    {
      // Craft a sitecore query to find the script in the scripts folder
      var query = string.Empty;

      if (string.IsNullOrEmpty(name))
      {
        query = ScriptRootPath + "//*[@@templatekey='script']";
      }
      else if (name.StartsWith("/"))
      {
        var safeName = name;

        // ensure query is escaped properly
        if (name.Contains("-"))
        {
          string[] parts = name.Split('/');
          for (int i = 0; i < parts.Length; i++)
          {
            if (parts[i].Contains("-"))
              parts[i] = "#" + parts[i] + "#";
          }

          safeName = string.Join("/", parts);
        }

        query = safeName + "[@@templatekey='script']";
      }
      else
        query = string.Format(ScriptRootPath + "//*[@@key='{0}' and @@templatekey='script']", name.ToLower());

      var database = Sitecore.Configuration.Factory.GetDatabase(_databaseName);

      return database.SelectItems(query);
    }
  }
}