using NUnit.Framework;
using System.Threading;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("PrettyPrint")]
  public class PrettyPrint : BaseCommandTest
  {
    [Test]
    public void XmlAndDate()
    {
      var cmd = new Cmd.PrettyPrint();
      InitCommand(cmd);

      cmd.FormatDate = true;
      cmd.FormatXml = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void NoFormat()
    {
      var cmd = new Cmd.PrettyPrint();
      InitCommand(cmd);

      cmd.FormatDate = false;
      cmd.FormatXml = false;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void Xml_NoInput()
    {
      var cmd = new Cmd.PrettyPrint();
      InitCommand(cmd);

      cmd.FormatDate = false;
      cmd.FormatXml = true;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

	[Test]
    public void Xml_NoXmlInInput()
    {
      var cmd = new Cmd.PrettyPrint();
      InitCommand(cmd);

      cmd.FormatDate = false;
      cmd.FormatXml = true;
      cmd.Input = "abc";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

	[Test]
    public void Xml_Valid()
    {
      var cmd = new Cmd.PrettyPrint();
      InitCommand(cmd);

      cmd.FormatDate = false;
      cmd.FormatXml = true;
      cmd.Input = "<a><b>c</b></a>";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringMatching(@"<a>\s+<b>c</b>\s+</a>"));
    }

	[Test]
    public void Date_NoInput()
    {
      var cmd = new Cmd.PrettyPrint();
      InitCommand(cmd);

      cmd.FormatDate = true;
      cmd.FormatXml = false;

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

	[Test]
    public void Date_NotDate()
    {
      var cmd = new Cmd.PrettyPrint();
      InitCommand(cmd);

      cmd.FormatDate = true;
      cmd.FormatXml = false;
      cmd.Input = "blah";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [TestCase("en-GB", "2/06/2014")]
    [TestCase("en-US", "6/2/2014")]
    [TestCase("de-DE", "02.06.2014 00:00:00")]
    public void Date_Valid(string culture, string expected)
    {
      var cmd = new Cmd.PrettyPrint();
      InitCommand(cmd);

      cmd.FormatDate = true;
      cmd.FormatXml = false;
      cmd.Input = "20140602";

      var oldCulture = Thread.CurrentThread.CurrentCulture;
      CommandResult result;

      try
      {
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);
        result = cmd.Run();
      }
      finally
      {
        Thread.CurrentThread.CurrentCulture = oldCulture;
      }

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringContaining(expected));
    }

    [Test]
    public void Json_Valid()
    {
      var cmd = new Cmd.PrettyPrint();
      InitCommand(cmd);

      cmd.FormatJson = true;
      cmd.Input = "{\"a\":3,\"b\":\"c\"}";

      var result = cmd.Run();

      var expected = "{\r\n  \"a\": 3,\r\n  \"b\": \"c\"\r\n}";

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(expected));
    }

    [Test]
    public void Json_Invalid()
    {
      var cmd = new Cmd.PrettyPrint();
      InitCommand(cmd);

      cmd.FormatJson = true;
      cmd.Input = "{\"a\":3\"b\":\"c\"}";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }
  }
}