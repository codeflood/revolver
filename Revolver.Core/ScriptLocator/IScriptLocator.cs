using System.Collections.Generic;

namespace Revolver.Core.ScriptLocator
{
  public interface IScriptLocator
  {
    string GetScript(string name);
    HelpDetails GetScriptHelp(string name);
    IEnumerable<string> GetScriptNames();
  }
}
