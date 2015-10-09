using NUnit.Framework;
using Revolver.Core.Formatting;

namespace Revolver.Test
{
  [TestFixture]
  [Category("Parser")]
  public class Parser
  {
    [Test]
    public void ParseGroupSimple()
    {
      var groups = Revolver.Core.Parser.ParseFirstLevelGroups("this is a (simple) group", '(', ')');
      Assert.That(groups.Length, Is.EqualTo(5));
      Assert.That(groups, Is.EqualTo(new[] {"this", "is", "a", "simple", "group"}));
    }

    [Test]
    public void ParseGroupNested()
    {
      var groups = Revolver.Core.Parser.ParseFirstLevelGroups("this is (a (nested) group)", '(', ')');
      Assert.That(groups.Length, Is.EqualTo(3));
      Assert.That(groups, Is.EqualTo(new[] { "this", "is", "a (nested) group" }));
    }

    [Test]
    public void ParseGroupMultiple()
    {
      var groups = Revolver.Core.Parser.ParseFirstLevelGroups("this is a (simple) group with (multiple brackets)", '(', ')');
      Assert.That(groups.Length, Is.EqualTo(7));
      Assert.That(groups, Is.EqualTo(new[] { "this", "is", "a", "simple", "group", "with", "multiple brackets" }));
    }

    [Test]
    public void ParseGroupMultipleNested()
    {
      var groups = Revolver.Core.Parser.ParseFirstLevelGroups("asd ( qwe ( dsa ) ) oi ( sad )", '(', ')');
      Assert.That(groups.Length, Is.EqualTo(4));
      Assert.That(groups, Is.EqualTo(new[] { "asd", "qwe ( dsa )", "oi", "sad" }));
    }

    [Test]
    public void ParseGroupMissingEnding()
    {
      var groups = Revolver.Core.Parser.ParseFirstLevelGroups("asd ( qwe", '(', ')');
      Assert.That(groups.Length, Is.EqualTo(2));
      Assert.That(groups, Is.EqualTo(new[] { "asd", "qwe" }));
    }

    [Test]
    public void ParseGroupMissingEndingNested()
    {
      var groups = Revolver.Core.Parser.ParseFirstLevelGroups("asd ( qwe ) sdf (ert (oiu)", '(', ')');
      Assert.That(groups.Length, Is.EqualTo(5));
      Assert.That(groups, Is.EqualTo(new[] { "asd", "qwe", "sdf", "ert", "(oiu)" }));
    }

    [Test]
    public void ParseGroupNoGroups()
    {
      var groups = Revolver.Core.Parser.ParseFirstLevelGroups("there are no groups", '(', ')');
      Assert.That(groups.Length, Is.EqualTo(4));
      Assert.That(groups, Is.EqualTo(new[] { "there", "are", "no", "groups" }));
    }

    [Test]
    public void ParseGroupOtherBrackets()
    {
      var groups = Revolver.Core.Parser.ParseFirstLevelGroups("asd < qwe < dsa >> oi <sad >", '<', '>');
      Assert.That(groups.Length, Is.EqualTo(4));
      Assert.That(groups, Is.EqualTo(new[] { "asd", "qwe < dsa >", "oi", "sad" }));
    }

    [Test]
    public void ParseMultipleLinesSingleLine()
    {
      var input = "this is input";
      var formatter = new TextOutputFormatter();
      var lines = Revolver.Core.Parser.ParseScriptLines(input, formatter);

      Assert.That(lines, Is.EquivalentTo(new[]
      {
        input
      }));
    }

    [Test]
    public void ParseMultipleLinesWindowsEndings()
    {
      var input = "line 1\r\nline 2";
      var formatter = new TextOutputFormatter();
      var lines = Revolver.Core.Parser.ParseScriptLines(input, formatter);

      Assert.That(lines, Is.EquivalentTo(new[]
      {
        "line 1",
        "line 2"
      }));
    }

    [Test]
    public void ParseMultipleLinesUnixEndings()
    {
      var input = "line 1\nline 2";
      var formatter = new TextOutputFormatter();
      var lines = Revolver.Core.Parser.ParseScriptLines(input, formatter);

      Assert.That(lines, Is.EquivalentTo(new[]
      {
        "line 1",
        "line 2"
      }));
    }

    [Test]
    public void ParseMultipleLinesLineContinued()
    {
      var input = "start-\r\nend";
      var formatter = new TextOutputFormatter();
      var lines = Revolver.Core.Parser.ParseScriptLines(input, formatter);

      Assert.That(lines, Is.EquivalentTo(new[]
      {
        "startend"
      }));
    }

    [Test]
    public void ParseMultipleLinesLineContinuationEscaped()
    {
      var input = "start\\-\r\nend";
      var formatter = new TextOutputFormatter();
      var lines = Revolver.Core.Parser.ParseScriptLines(input, formatter);

      Assert.That(lines, Is.EquivalentTo(new[]
      {
        "start-",
        "end"
      }));
    }

    // todo: cover other methods
  }
}