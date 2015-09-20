using System;
using Revolver.Core.Exceptions;
using Sitecore.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Revolver.Core.ScriptLocator
{
  [Serializable]
  public class ScriptLocator : IScriptLocator
  {
    protected const string CONFIG_PATH = "/sitecore/revolver/script/locator/*";

    protected IList<IScriptLocator> _locators = null;

    public ScriptLocator()
    {
      _locators = LoadLocators().ToList();
    }

    public string GetScript(string name)
    {
      var scripts = (from locator in _locators
                     let s = locator.GetScript(name)
                     where !string.IsNullOrEmpty(s)
                     select s).ToArray();

      var count = scripts.Count();

      if (count == 0)
        return null;

      if (count == 1)
        // Convert \n to \r\n for consistency. \n may be used by some browsers or if line endings in a script are not Windows style
        return scripts.ElementAt(0).Replace("(?<!\r)\n", "\r\n");

      // todo: return script names
      throw new MultipleScriptsFoundException(new string[0]);
    }

    public HelpDetails GetScriptHelp(string name)
    {
      var details = from locator in _locators
                    let d = locator.GetScriptHelp(name)
                    where d != null
                    select d;

      var count = details.Count();

      if (count == 0)
        return null;

      if (count == 1)
        return details.ElementAt(0);

      // todo: return script names
      throw new MultipleScriptsFoundException(new string[0]);
    }

    public IEnumerable<string> GetScriptNames()
    {
      var locatorScriptNames = from locator in _locators
        select locator.GetScriptNames();

      return locatorScriptNames.SelectMany(x => x).OrderBy(x => x);
    }

    protected IEnumerable<IScriptLocator> LoadLocators()
    {
      var nodes = Factory.GetConfigNodes(CONFIG_PATH);
      return from XmlNode node in nodes
             select Factory.CreateObject<IScriptLocator>(node);
    }
  }
}