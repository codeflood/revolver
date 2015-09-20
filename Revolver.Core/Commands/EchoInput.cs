using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Revolver.Core.Commands
{
  [Command("echo")]
  public class EchoInput : BaseCommand
  {
    [NamedParameter("f", "filename")]
    [Description("Direct the output to a file.")]
    [Optional]
    public string FileName { get; set; }

    [FlagParameter("i")]
    [Description("Read input from a file.")]
    [Optional]
    public bool ReadInputFromFile { get; set; }

    [FlagParameter("a")]
    [Description("Append output when echoing to a file.")]
    [Optional]
    public bool Append { get; set; }

    [ListParameter("input")]
    [Description("The input to echo back after substitution.")]
    [Optional]
    public IList<string> Input { get; set; }

    public override CommandResult Run()
    { 
      string filename = FileName ?? string.Empty;
       
      if (ReadInputFromFile && filename == string.Empty)
        return new CommandResult(CommandStatus.Failure, "-i can only be used with -f");

      if (ReadInputFromFile && Append)
        return new CommandResult(CommandStatus.Failure, "-a and -i cannot be used together");

      if (filename != string.Empty && !filename.Contains(":") && !filename.StartsWith("\\\\"))
      {
        filename = Sitecore.Configuration.Settings.TempFolderPath + "\\" + filename;

        if (!filename.Contains(":"))
          filename = System.Web.HttpContext.Current.Server.MapPath(filename);
      }

      if (ReadInputFromFile)
      {
        // Convert \r to \r\n so the UI doesn't break
        string fileContent = File.ReadAllText(filename);
        fileContent = Regex.Replace(fileContent, "\r(?!\n)", Environment.NewLine);
        return new CommandResult(CommandStatus.Success, Parser.PerformSubstitution(Context, fileContent));
      }
      else if (Input != null && Input.Count > 0)
      {
        if (string.IsNullOrEmpty(filename))
        {
#if NET35
          return new CommandResult(CommandStatus.Success, string.Join(" ", Input.ToArray()));
#else
          return new CommandResult(CommandStatus.Success, string.Join(" ", Input));
#endif
        }
        else
        {
          string dir = Path.GetDirectoryName(filename);
          if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

          if (Append)
          {
#if NET35
            File.AppendAllText(filename, string.Join(" ", Input.ToArray()) + Environment.NewLine);
#else
            File.AppendAllText(filename, string.Join(" ", Input) + Environment.NewLine);
#endif
            return new CommandResult(CommandStatus.Success, "Output appended to " + filename);
          }
          else
          {
#if NET35
            File.WriteAllText(filename, string.Join(" ", Input.ToArray()) + Environment.NewLine);
#else
            File.WriteAllText(filename, string.Join(" ", Input) + Environment.NewLine);
#endif
            return new CommandResult(CommandStatus.Success, "Output written to " + filename);
          }
        }
      }
      else
        return new CommandResult(CommandStatus.Success, string.Empty);
    }

    public override string Description()
    {
      return "Echo input after substitution";
    }

    public override void Help(HelpDetails details)
    {
      var comments = new StringBuilder();
      Formatter.PrintLine("If echoing to a file and the filename given is not absolute the file will be written to the temp folder.", comments);
      comments.Append("-a and -i cannot be used together");

      details.Comments = comments.ToString();
      details.AddExample("this is input");
      details.AddExample("hello");
      details.AddExample("$prevpath$");
      details.AddExample("-f c:\\temp\\output.txt This is some output");
      details.AddExample("-f temp.xml < gi");
      details.AddExample("-a -f temp.txt < gf");
      details.AddExample("-i -f c:\\temp\\item.xml");
    }
  }
}
