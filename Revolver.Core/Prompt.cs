using System;

namespace Revolver.Core
{
  public static class Prompt
  {
    /// <summary>
    /// Evaluates the tokens in a prompt string
    /// </summary>
    /// <param name="context">The context to use during evaluation</param>
    /// <param name="prompt">The prompt to evaluate</param>
    /// <returns>The prompt</returns>
    public static string EvaluatePrompt(Context context, string prompt)
    {
      string toRet = prompt;

      if (context.CurrentItem != null)
      {
        // Path / DB tokens
        toRet = toRet.Replace("%path%", context.CurrentItem.Paths.FullPath);
        toRet = toRet.Replace("%itemname%", context.CurrentItem.Name);
        toRet = toRet.Replace("%ver%", context.CurrentItem.Version.Number.ToString());
      }
      else
      {
        toRet = toRet.Replace("%path%", "<undefined>");
        toRet = toRet.Replace("%itemname%", "<undefined>");
        toRet = toRet.Replace("%ver%", "<undefined>");
      }

      if (context.CurrentDatabase != null)
        toRet = toRet.Replace("%db%", context.CurrentDatabase.Name);
      else
        toRet = toRet.Replace("%db%", "<undefined>");

      if (context.CurrentLanguage != null)
      {
        toRet = toRet.Replace("%lang%", context.CurrentLanguage.CultureInfo.DisplayName);
        toRet = toRet.Replace("%langcode%", context.CurrentLanguage.Name);
      }
      else
      {
        toRet = toRet.Replace("%lang%", "<undefined>");
        toRet = toRet.Replace("%langcode%", "<undefined>");
      }

      // DateTime tokens
      DateTime dt = DateTime.Now;
      toRet = toRet.Replace("%date%", dt.ToShortDateString());
      toRet = toRet.Replace("%time%", dt.ToShortTimeString());

      // Environment variables
      toRet = Parser.PerformSubstitution(context, toRet);

      return toRet;
    }
  }
}