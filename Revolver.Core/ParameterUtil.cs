using System;
using System.Collections.Specialized;

namespace Revolver.Core
{
  public static class ParameterUtil
  {
    /// <summary>
    /// Find the parameter value with the given parameter name
    /// </summary>
    /// <param name="args">The arguments array to search through</param>
    /// <param name="name">The name of the parameter to search for</param>
    /// <param name="output">Parameter to put result in</param>
    /// <returns>The index of the parameter</returns>
    public static int GetParameter(string[] args, string name, ref string output)
    {
      int ind = Array.IndexOf(args, name);
      if (ind >= 0)
      {
        if (args.Length >= ind + 2)
          output = args[ind + 1];
        else
          throw new ArgumentException("Missing parameter value for " + name);

        return ind + 1;
      }

      return ind;
    }

    /// <summary>
    /// Find the element offset from the given parameter name
    /// </summary>
    /// <param name="args">The arguments array to search through</param>
    /// <param name="name">The name of the parameter to search for</param>
    /// <param name="offset">The offset after the parameter to return</param>
    /// <param name="output">Parameter to put the result in</param>
    /// <returns>The index of the parameter</returns>
    public static int GetParameter(string[] args, string name, int offset, ref string output)
    {
      int ind = Array.IndexOf(args, name);
      if (ind >= 0)
      {
        if (args.Length >= ind + 2 + offset)
          output = args[ind + 1 + offset];
        else
          throw new ArgumentException("Missing offset value for " + name);

        return ind + 1 + offset;
      }

      return ind;
    }

    /// <summary>
    /// Extract flags, named and numbered parameters from a string array of arguments
    /// </summary>
    /// <param name="named">The found named parameters</param>
    /// <param name="numbered">The found numbered parameters</param>
    /// <param name="input">The arguments</param>
    /// <param name="flags">Allowed flags to find</param>
    public static void ExtractParameters(out StringDictionary named, out string[] numbered, string[] input, string[] flags)
    {
      named = new StringDictionary();
      StringCollection numberedColl = new StringCollection();
      StringCollection args = new StringCollection();
      args.AddRange(input);

      // Pull out flags first
      if (flags != null)
      {
        for (int i = 0; i < flags.Length; i++)
        {
          int ind = -1;
          if ((ind = args.IndexOf("-" + flags[i])) >= 0)
          {
            named.Add(flags[i], string.Empty);
            args[ind] = null;
          }
        }
      }

      // pull out named parameters
      StringEnumerator e = args.GetEnumerator();
      string name = string.Empty;
      while (e.MoveNext())
      {
        if (e.Current != null)
        {
          if (name != string.Empty)
          {
            string nextname = string.Empty;
            string value = e.Current;
            if (value == null)
              value = string.Empty;

            if (value.StartsWith("-") && value.Length > 1)
            {
              nextname = value.Substring(1);
              value = string.Empty;
            }

            if (value.StartsWith("\\-"))
              value = "-" + value.Substring(2);

            named.Add(name, value);

            if (nextname != string.Empty)
              name = nextname;
            else
              name = string.Empty;
          }
          else if (e.Current.StartsWith("-") && e.Current.Length > 1)
            name = e.Current.Substring(1);
          else
          {
            string value = e.Current;
            if (value.StartsWith("\\-"))
              value = "-" + value.Substring(2);

            numberedColl.Add(value);
          }
        }
        else
        {
          if (name != string.Empty)
          {
            named.Add(name, string.Empty);
            name = string.Empty;
          }
        }
      }

      if (name != string.Empty)
        named.Add(name, string.Empty);

      // Pull out numbered parameters
      numbered = new string[numberedColl.Count];
      numberedColl.CopyTo(numbered, 0);
    }

    /// <summary>
    /// Extract flags, named and numbered parameters from a string array of arguments
    /// </summary>
    /// <param name="named">The found named parameters</param>
    /// <param name="numbered">The found numbered parameters</param>
    /// <param name="input">The arguments</param>
    public static void ExtractParameters(out StringDictionary named, out string[] numbered, string[] input)
    {
      StringDictionary innerNamed = null;
      string[] innerNumbered = null;
      ExtractParameters(out innerNamed, out innerNumbered, input, null);
      named = innerNamed;
      numbered = innerNumbered;
    }

    /// <summary>
    /// Remove an element from an array by value and optionally a number of elements after it.
    /// </summary>
    /// <param name="array">The array to remove the value from</param>
    /// <param name="value">The value to remove</param>
    /// <param name="count">The number of elements after the value to remove aswell</param>
    /// <returns>An array without the value</returns>
    public static string[] RemoveParameter(string[] array, string value, int count)
    {
      if (Array.IndexOf(array, value) < 0)
        return array;
      else
      {
        StringCollection coll = new StringCollection();
        coll.AddRange(array);
        int ind = coll.IndexOf(value);
        for (int i = ind + count; i >= ind; i--)
          coll.RemoveAt(ind);

        string[] output = new string[coll.Count];
        coll.CopyTo(output, 0);
        return output;
      }
    }
  }
}