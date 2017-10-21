
namespace Revolver.Core
{
  public static class ExtendedHelp
  {
    public static HelpDetails Expressions()
    {
      HelpDetails details = new HelpDetails();
      details.Description = "Allows logical testing against fields and attributes";
      details.Usage = "[@field | @@attribute] operator [@field | @@attribute] [as cast] [with flag] [and | or expression]";
      details.AddParameter("operator", "How to compare the 2 arguments. Must be one of = (equals), < (less than), > (greater than), != (not equal), <= (less or equal), >= (greater or equal), [ (starts with), ] (ends with), ? (contains), !? (doesn't contain).");
      details.AddParameter("cast", "Treat the argument as a specific data type. Must be one of string, number, date.");
      details.AddParameter("flag", "Treat the argument in a specific way. Must be one of ignorecase, ignoredecimal, round, ceiling, floor.");
      details.AddParameter("expression", "Another expression. The operator (and, or) is used to determine the overall result");
      details.Comments = "Expressions are used as arguments to other commands such as find.";
      details.AddExample("@title != hello");
      details.AddExample("(@__created by) = admin with ignorecase");
      details.AddExample("@price >= 70 as number with round and @title = bananas with ignorecase");
      details.AddExample("@@key = a or @@key = b or @@key = c");
      details.AddExample("@__created = 12/01/2007 as date");
      details.AddExample("@@name [ a");
      return details;
    }

    public static HelpDetails Prompt()
    {
      HelpDetails details = new HelpDetails();
      details.Description = "The prompt is set through the environment variable 'prompt'";
      details.Usage = "[Any characters] [%path%] [%itemname%] [%ver%] [%db%] [%lang%] [%langcode%] [%date%] [%time%]";
      details.AddParameter("%path%", "Provides the full path of the current item");
      details.AddParameter("%itemname%", "Provides the name of the current item");
      details.AddParameter("%ver%", "Provides the version number of the current item");
      details.AddParameter("%db%", "Provides the name of the current database");
      details.AddParameter("%lang%", "Provides the title of the current language");
      details.AddParameter("%langcode", "Provides the code of the current language");
      details.AddParameter("%date%", "Provides the current date");
      details.AddParameter("%time%", "Provides the current time");
      details.AddExample("%db%:%path% >");
      details.AddExample("%date% %lang%|%itemname% >");
      return details;
    }

    public static HelpDetails SubCommand()
    {
      HelpDetails details = new HelpDetails();
      details.Description = "Allows the evaluation of a command to be used as a parameter of another command";
      details.Usage = "< command";
      details.AddParameter("command", "The command to evaluate");
      details.AddExample("sf title < (gf -f title ../..)");
      details.AddExample("find echo < (ga -a key) < (ga -a id)");
      details.AddExample("cd < (ga -a templateid)");
      return details;
    }

    public static HelpDetails UseEnvironmentVariable()
    {
      HelpDetails details = new HelpDetails();
      details.Description = "Allows substitution of environment variables into commands";
      details.Usage = "$name$";
      details.AddParameter("name", "The name of the environment variable");
      details.AddExample("$prevpath$");
      details.AddExample("$myvar$");
      details.AddExample("echo $prevpath$");
      return details;
    }
  }
}
