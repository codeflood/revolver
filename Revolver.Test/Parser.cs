using NUnit.Framework;

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

    // todo: cover other methods
  }
}