using Sitecore.Data;
using System.Collections.Specialized;

namespace Revolver.Core
{
  public static class PathParser
  {
    /// <summary>
    /// Evaulate the path string in relation to the current item
    /// </summary>
    /// <param name="context">The Revolver context to evaluate the path against</param>
    /// <param name="path">The path to evaulate. Can either be absolute or relative</param>
    /// <returns>The full sitecore path to the target item</returns>
    public static string EvaluatePath(Context context, string path)
    {
      if (ID.IsID(path))
        return path;

      string workingPath = string.Empty;
      if (!path.StartsWith("/"))
        workingPath = context.CurrentItem.Paths.FullPath + "/" + path;
      else
        workingPath = path;

      // Strip any language and version tags
      if (workingPath.IndexOf(':') >= 0)
        workingPath = workingPath.Substring(0, workingPath.IndexOf(':'));

      // Make relative paths absolute
      string[] parts = workingPath.Split('/');
      StringCollection targetParts = new StringCollection();
      targetParts.AddRange(parts);

      while (targetParts.Contains(".."))
      {
        int ind = targetParts.IndexOf("..");
        targetParts.RemoveAt(ind);
        if (ind > 0)
        {
          targetParts.RemoveAt(ind - 1);
        }
      }

      if (targetParts[targetParts.Count - 1] == ".")
        targetParts.RemoveAt(targetParts.Count - 1);

      // Remove empty elements
      while (targetParts.Contains(""))
      {
        targetParts.RemoveAt(targetParts.IndexOf(""));
      }

      string[] toRet = new string[targetParts.Count];
      targetParts.CopyTo(toRet, 0);
      return "/" + string.Join("/", toRet);
    }

    /// <summary>
    /// Parse the language out of a path
    /// </summary>
    /// <param name="path">The path to parse</param>
    /// <returns>The language if found, otherwise string.Empty</returns>
    public static string ParseLanguageFromPath(string path)
    {
      string toRet = string.Empty;
      if (path.IndexOf(':') >= 0)
      {
        string[] parts = path.Split(':');
        toRet = parts[1];
      }
      return toRet;
    }

    /// <summary>
    /// Parse the version out of a path
    /// </summary>
    /// <param name="path">The path to parse</param>
    /// <returns>The version if found, otherwise string.Empty</returns>
    public static string ParseVersionFromPath(string path)
    {
      string toRet = string.Empty;
      if (path.IndexOf(':') >= 0)
      {
        string[] parts = path.Split(':');
        if (parts.Length >= 3)
          toRet = parts[2];
        else
          toRet = string.Empty;
      }
      return toRet;
    }
  }
}