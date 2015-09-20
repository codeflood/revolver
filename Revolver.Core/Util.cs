using Sitecore.Data.Items;
using Sitecore.Globalization;
using System;
using System.Collections.Specialized;

namespace Revolver.Core
{
  public static class Util
  {
    [Obsolete("Use Revolver.Core.PathParser.EvaluatePath() instead")]
    public static string EvaluatePath(Context context, string path)
    {
      return PathParser.EvaluatePath(context, path);
    }

    [Obsolete("Use Revolver.Core.PathParser.ParseLanguageFromPath() instead")]
    public static string ParseLanguageFromPath(string path)
    {
      return PathParser.ParseLanguageFromPath(path);
    }

    [Obsolete("Use Revolver.Core.PathParser.ParseVersionFromPath() instead")]
    public static string ParseVersionFromPath(string path)
    {
      return PathParser.ParseVersionFromPath(path);
    }


    [Obsolete("Use Revolver.Core.Context.SetContext() instead")]
    public static CommandResult SetContext(Context context, string path, string dbName = null, Language language = null, int? versionNumber = null)
    {
      return context.SetContext(path, dbName, language, versionNumber);
    }

    [Obsolete("Use Revolver.Core.ItemInspector.CountChildren() instead")]
    public static int CountChildren(Item item)
    {
      var inspector = new ItemInspector(item);
      return inspector.CountDescendants();
    }

    [Obsolete("Use Revolver.Core.ExpressionParser.EvaluateExpression() instead")]
    public static bool EvaluateExpression(Context context, string exp)
    {
      return ExpressionParser.EvaluateExpression(context, exp);
    }

    [Obsolete("Use Revolver.Core.ExpressionParser.EvaluateSingleExpression() instead")]
    public static bool EvaluateSingleExpression(Context context, string exp)
    {
      return ExpressionParser.EvaluateSingleExpression(context, exp);
    }

    [Obsolete("Use Revolver.Core.ItemInspector.GetItemAttribute() instead")]
    public static string GetItemAttribute(Item item, string name)
    {
      var inspector = new ItemInspector(item);
      return inspector.GetItemAttribute(name);
    }

    [Obsolete("Use Revolver.Core.Prompt.EvaluatePrompt() instead")]
    public static string EvaluatePrompt(Context context, string prompt)
    {
      return Prompt.EvaluatePrompt(context, prompt);
    }

    [Obsolete("Use Revolver.Core.ParameterUtil.GetParameter() instead")]
    public static int GetParameter(string[] args, string name, ref string output)
    {
      return ParameterUtil.GetParameter(args, name, ref output);
    }

    [Obsolete("Use Revolver.Core.ParameterUtil.GetParameter() instead")]
    public static int GetParameter(string[] args, string name, int offset, ref string output)
    {
      return ParameterUtil.GetParameter(args, name, offset, ref output);
    }

    [Obsolete("Use Revolver.Core.ParameterUtil.ExtractParameters() instead")]
    public static void ExtractParameters(out StringDictionary named, out string[] numbered, string[] input, string[] flags)
    {
      ParameterUtil.ExtractParameters(out named, out numbered, input, flags);
    }

    [Obsolete("Use Revolver.Core.ParameterUtil.ExtractParameters() instead")]
    public static void ExtractParameters(out StringDictionary named, out string[] numbered, string[] input)
    {
      ParameterUtil.ExtractParameters(out named, out numbered, input);
    }

    [Obsolete("Use Revolver.Core.ParameterUtil.RemoveParameter() instead")]
    public static string[] RemoveParameter(string[] array, string value, int count)
    {
      return ParameterUtil.RemoveParameter(array, value, count);
    }

    [Obsolete("Use Revolver.Core.Parser.PerformSubstitution() instead")]
    public static string PerformSubstitution(Context context, string input, bool autowrap)
    {
      return Parser.PerformSubstitution(context, input);
    }


    [Obsolete("Use Revolver.Core.ProductInfo.GetProductName() instead")]
    public static string GetProductName()
    {
      return ProductInfo.GetProductName();
    }

    [Obsolete("Use Revolver.Core.ProductInfo.GetProductVersion() instead")]
    public static string GetProductVersion()
    {
      return ProductInfo.GetProductVersion();
    }

    [Obsolete("No longer supported")]
    public static HelpDetails CreateHelpForScript(Item script)
    {
      return new HelpDetails
      {
        Description = "Not supported. Use the 'Help' command."
      };
    }

    [Obsolete("Use Revolver.Core.Parser.TryParseBoolean() instead")]
    public static bool TryParseBoolean(string value, out bool boolean)
    {
      return Parser.TryParseBoolean(value, out boolean);
    }

    [Obsolete("Use Revolver.Core.Parser.ParseScriptLines() instead")]
    public static string[] ParseScriptLines(string scriptSource, IFormatContext formatContext)
    {
      return Parser.ParseScriptLines(scriptSource, formatContext);
    }
  }
}
