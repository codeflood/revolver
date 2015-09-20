using System;
using Revolver.Core.Exceptions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Revolver.Core.Formatting;

namespace Revolver.Core.ScriptLocator
{
  [Serializable]
  public class FileScriptLocator : IScriptLocator
  {
    public const string HelpCommentSymbol = "^";
    public const string HelpCommentDelimiter = ":";

    protected readonly string _path = string.Empty;
    protected readonly string _fileExtension = string.Empty;

    public FileScriptLocator(string path, string extension)
    {
      if (!path.Contains(Path.VolumeSeparatorChar) && HttpContext.Current != null)
        _path = HttpContext.Current.Server.MapPath(path);
      else
        _path = path;

      _fileExtension = extension;
    }

    public string GetScript(string name)
    {
      var files = FindScriptFiles(name);
      if (files == null)
        return null;

      if (files.Length == 1)
        return File.ReadAllText(files[0].FullName);

      if (files.Length > 1)
      {
        var filenames = from file in files
                        select file.FullName;

        throw new MultipleScriptsFoundException(filenames);
      }

      return null;
    }

    public HelpDetails GetScriptHelp(string name)
    {
      var scriptSource = GetScript(name);

      // Check if the script exists. If not, return null
      if (scriptSource == null)
        return null;

      var helpData = ExtractHelpData(scriptSource);

      var details = new HelpDetails();
      foreach (var entry in helpData)
      {
        switch (entry.Key.ToLower())
        {
          case "comments":
            details.Comments += entry.Value;
            break;

          case "description":
            details.Description += entry.Value;
            break;

          case "example":
            details.AddExample(entry.Value);
            break;

          case "parameter":
            var idx = entry.Value.IndexOf(HelpCommentDelimiter);
            if (entry.Value.Length > idx)
            {
              var key = entry.Value.Substring(0, idx);
              var desc = entry.Value.Substring(idx + 1);

              details.AddParameter(key, desc);
            }
            
            break;

          case "usage":
            details.Usage = entry.Value;
            break;
        }
      }

      return details;
    }

    public IEnumerable<string> GetScriptNames()
    {
      var scriptFiles = FindScriptFiles(null);
      return from file in scriptFiles
             select Path.GetFileNameWithoutExtension(file.Name);

      // todo: Show leading path
    }

    protected FileInfo[] FindScriptFiles(string name)
    {
      var safeName = name;

      if (string.IsNullOrEmpty(name))
      {
        safeName = "*";
      }
      else
      {
        // Make sure the script name doesn't contain leading slashes.
        if (safeName.StartsWith("/") || safeName.StartsWith("\\"))
          safeName = safeName.TrimStart('/', '\\');
      }

      var fullPath = Path.Combine(_path, safeName);
      fullPath = Path.ChangeExtension(fullPath, "." + _fileExtension);

      var dir = Path.GetDirectoryName(fullPath);

      if (Directory.Exists(dir))
      {
        var fileName = Path.GetFileName(fullPath);

        var dirInfo = new DirectoryInfo(dir);
        return dirInfo.GetFiles(fileName, SearchOption.AllDirectories);
      }

      return null;
    }

    protected IEnumerable<KeyValuePair<string, string>> ExtractHelpData(string scriptSource)
    {
      var lines = Parser.ParseScriptLines(scriptSource, new TextOutputFormatter());

      // Parse all help comments from script
      var scriptCommentLineIndicatorLength = Constants.ScriptCommentLineIndicator.Length;
      var helpCommentIndicatorLength = HelpCommentSymbol.Length;

      var commentLines = from line in lines
        where line.StartsWith(Constants.ScriptCommentLineIndicator) &&
              line.Substring(scriptCommentLineIndicatorLength).StartsWith(HelpCommentSymbol)
        select line.Substring(scriptCommentLineIndicatorLength + helpCommentIndicatorLength);

      // Break token into key and value
      foreach (var line in commentLines)
      {
        if (!line.Contains(HelpCommentDelimiter))
          continue;

        var idx = line.IndexOf(HelpCommentDelimiter);
        if(line.Length < idx + 1)
          continue;

        var key = line.Substring(0, idx);
        var value = line.Substring(idx + 1);

        yield return new KeyValuePair<string, string>(key.Trim(), value.Trim());
      }
    }
  }
}