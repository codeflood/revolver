using Revolver.Core.Exceptions;
using System;
using System.Threading;

namespace Revolver.Core
{
  // This class requires a review and a rewrite. Users should avoid direct interaction with this class.
  internal static class ExpressionParser
  {
    /// <summary>
    /// Evaluates an expression which may contain joining conditions (and, or)
    /// </summary>
    /// <param name="context">The current Revolver context</param>
    /// <param name="exp">The expression to evaluate</param>
    /// <returns>The outcome of th expression</returns>
    public static bool EvaluateExpression(Context context, string exp)
    {
      // Break the expression around the joining condition keywords			
      bool res = false;
      string[] expElms = exp.Trim().Replace("  ", " ").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
      int startInd = 0;
      string op = string.Empty;

      for (int i = 0; i < expElms.Length; i++)
      {
        if (expElms[i] == "and" || expElms[i] == "or" || i == expElms.Length - 1)
        {
          string[] currentPart;

          if (i == expElms.Length - 1)
          {
            currentPart = new string[(i + 1) - startInd];
            Array.Copy(expElms, startInd, currentPart, 0, (i + 1) - startInd);
          }
          else
          {
            currentPart = new string[i - startInd];
            Array.Copy(expElms, startInd, currentPart, 0, i - startInd);
          }

          bool currentRes = EvaluateSingleExpression(context, string.Join(" ", currentPart));
          if (op.Length > 0)
          {
            switch (op)
            {
              case "and":
                res &= currentRes;
                break;

              case "or":
                res |= currentRes;
                break;

              default:
                throw new ExpressionException("Invalid operator: " + op);
            }
          }
          else
            res = currentRes;

          if (expElms.Length > i + 1)
            op = expElms[i];

          if (expElms.Length > i + 2)
            startInd = i + 1;
        }
      }

      return res;
    }

    /// <summary>
    /// Evaluate a single expression
    /// </summary>
    /// <param name="item">The current item</param>
    /// <param name="exp">The expression to evaluate</param>
    /// <returns>The outcome of the expression</returns>
    public static bool EvaluateSingleExpression(Context context, string exp)
    {
      // Clean the expression
      exp = exp.Trim();

      // substitute tokens
      exp = Parser.PerformSubstitution(context, exp);

      // Parse elements
      string[] elms = Parser.ParseFirstLevelGroups(exp, Constants.SubcommandEnter, Constants.SubcommandExit);

      // Validate the expression
      if (elms.Length != 1 && elms.Length != 2 && elms.Length != 3 && elms.Length != 5 && elms.Length != 7)
        throw new ExpressionException("Malformed expression");

      // a single element in the expression should be parsable true or false
      if (elms.Length == 1)
      {
        var value = false;
        if (Parser.TryParseBoolean(elms[0], out value))
          return value;
        else
          throw new ExpressionException("Could not parse boolean value '" + elms[0] + "'");
      }

      // 2 elements means a function
      if (elms.Length == 2)
      {
        switch (elms[0])
        {
          case "isempty":
            return string.IsNullOrEmpty(elms[1]);

          case "not":
            return !EvaluateExpression(context, elms[1]);

          case "isbound":
            return context.CommandHandler.CoreCommands.ContainsKey(elms[1]) || context.CommandHandler.CustomCommands.ContainsKey(elms[1]);

          default:
            throw new ExpressionException("Unknown function " + elms[0]);
        }
      }

      string op = elms[1];
      if ((op != "<") && (op != ">") && (op != "=") && (op != "!=") && (op != "<=") && (op != ">=") && (op != "[") && (op != "]") && (op != "?") && (op != "!?"))
        throw new ExpressionException("Invalid operator " + op);

      // substitute fields and attributes and pull out casting operator and flags
      string type = "string";
      bool ignoreCase = false;
      bool ignoreDecimal = false;
      bool round = false;
      bool ceil = false;
      bool floor = false;

      for (int i = 0; i < elms.Length; i++)
      {
        if (elms[i].StartsWith("@@"))
        {
          var inspector = new ItemInspector(context.CurrentItem);
          var attr = inspector.GetItemAttribute(elms[i]);
          if (attr == null)
            throw new ExpressionException("Unknown attribute " + elms[i]);

          elms[i] = attr;
        }
        else if (elms[i].StartsWith("@"))
        {
          elms[i] = context.CurrentItem[elms[i].Substring(1)];
        }
        else
        {
          if (elms[i] == "as")
          {
            if (elms.Length >= i + 2)
              type = elms[i + 1];
            else
              throw new ExpressionException("Missing the cast type");
          }
          else if (elms[i] == "with")
          {
            if (elms.Length < i + 2)
              throw new ExpressionException("Missing flags");

            switch (elms[i + 1])
            {
              case "ignorecase": ignoreCase = true; break;
              case "ignoredecimal": ignoreDecimal = true; break;
              case "round": round = true; break;
              case "ceiling": ceil = true; break;
              case "floor": floor = true; break;
              default:
                throw new ExpressionException("Unknown flag " + elms[i + 1]);
            }
          }
        }
      }

      string val1 = elms[0];
      string val2 = elms[2];

      // Do we need to convert the strings?
      switch (type)
      {
        case "string":
          if (ignoreCase)
          {
            val1 = val1.ToLower();
            val2 = val2.ToLower();
          }

          // Now do the comparison
          switch (op)
          {
            case "=":
              return val1 == val2;

            case "<":
              return string.Compare(val1, val2) < 0;

            case ">":
              return string.Compare(val1, val2) > 0;

            case "!=":
              return val1 != val2;

            case "<=":
              return string.Compare(val1, val2) <= 0;

            case ">=":
              return string.Compare(val1, val2) >= 0;

            case "[":
              return val1.StartsWith(val2);

            case "]":
              return val1.EndsWith(val2);

            case "?":
              return val1.Contains(val2);

            case "!?":
              return !val1.Contains(val2);
          }
          break;

        case "number":
          if (val1.Length == 0 || val2.Length == 0)
            return false;

          double a = 0;
          double b = 0;
          if (!double.TryParse(val1, out a))
            throw new ExpressionException(string.Format("{0} is not a number", val1));
          if (!double.TryParse(val2, out b))
            throw new ExpressionException(string.Format("{0} is not a number", val2));

          if (ignoreDecimal)
          {
            a = Math.Truncate(a);
            b = Math.Truncate(b);
          }

          if (ceil)
          {
            a = Math.Ceiling(a);
            b = Math.Ceiling(b);
          }

          if (floor)
          {
            a = Math.Floor(a);
            b = Math.Floor(b);
          }

          if (round)
          {
            a = Math.Round(a);
            b = Math.Round(b);
          }

          // Now do the comparison
          switch (op)
          {
            case "=":
              return a == b;

            case "<":
              return a < b;

            case ">":
              return a > b;

            case "!=":
              return a != b;

            case "<=":
              return a <= b;

            case ">=":
              return a >= b;

            case "[":
              return a.ToString().StartsWith(b.ToString());

            case "]":
              return a.ToString().EndsWith(b.ToString());

            case "?":
              return a.ToString().Contains(b.ToString());

            case "!?":
              return !a.ToString().Contains(b.ToString());
          }
          break;

        case "date":
          if (val1.Length == 0 || val2.Length == 0)
            return false;

          DateTime aa;
          DateTime bb;
          DateTime defaultPassthrough = DateTime.MinValue.AddSeconds(7);

          var culture = Sitecore.Context.Culture;
          if (culture.IsNeutralCulture)
            culture = Thread.CurrentThread.CurrentCulture;

          aa = Sitecore.DateUtil.ParseDateTime(val1, defaultPassthrough, culture);
          if (aa == defaultPassthrough)
            throw new ExpressionException(string.Format("{0} is not a date", val1));

          bb = Sitecore.DateUtil.ParseDateTime(val2, defaultPassthrough, culture);
          if (bb == defaultPassthrough)
            throw new ExpressionException(string.Format("{0} is not a date", val2));

          // Now do the comparison
          switch (op)
          {
            case "=":
              return aa == bb;

            case "<":
              return aa < bb;

            case ">":
              return aa > bb;

            case "!=":
              return aa != bb;

            case "<=":
              return aa <= bb;

            case ">=":
              return aa >= bb;
          }
          break;
      }

      // Something went wrong if we get to here
      return false;
    }
  }
}