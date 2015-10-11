using NUnit.Framework;
using NUnit.Framework.Constraints;
using Revolver.Core;
using Revolver.Core.Formatting;
using Revolver.UI;
using Sitecore;
using Sitecore.Data.Items;
using Mod = Revolver.Core;

namespace Revolver.Test
{
  [TestFixture]
  [Category("CommandHandler")]
  public class CommandHandler : BaseCommandTest
  {
    private Item _testContent = null;

    [TestFixtureSetUp]
    public void Init()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      InitContent();
      _testContent = TestUtil.CreateContentFromFile("TestResources\\narrow tree.xml", _testRoot);
    }

    [Test]
    public void NoCommand()
    {
      var ctx = new Mod.Context();
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());
      var result = handler.Execute("");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.Empty);
    }

    [Test]
    public void CommandWithoutParameters()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());
      var result = handler.Execute("ls");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Luna"));
      Assert.That(result.Message, Contains.Substring("Deimos"));
      Assert.That(result.Message, Contains.Substring("phobos"));
      Assert.That(result.Message, Contains.Substring("Adrastea Phobos"));
    }

    [Test]
    public void CommandSingleNumberedParameter()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());
      var result = handler.Execute("cd luna");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(ctx.CurrentItem.ID, Is.EqualTo(_testContent.Axes.GetChild("Luna").ID));
    }

    [Test]
    public void CommandMultipleNumberedParameters()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());
      var result = handler.Execute("find pwd luna");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Carme"));
      Assert.That(result.Message, Contains.Substring("Ganymede"));
      Assert.That(result.Message, Contains.Substring("Metis"));
    }

    [Test]
    public void CommandSingleNamedParameter()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());
      var result = handler.Execute("ls -r os");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.Not.Contains("Luna"));
      Assert.That(result.Message, Contains.Substring("Deimos"));
      Assert.That(result.Message, Contains.Substring("phobos"));
      Assert.That(result.Message, Contains.Substring("Adrastea Phobos"));
    }

    [Test]
    public void CommandMixedParameterTypes()
    {
      var deimosId = _testContent.Axes.GetChild("Deimos").ID.ToString();
      var phobosId = _testContent.Axes.GetChild("phobos").ID.ToString();

      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());
      var result = handler.Execute(string.Format("find -i {0}|{1} -a name phobos -ns (ga -a id)", deimosId, phobosId));

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo(phobosId));
    }

    [Test]
    public void CommandFlagParameter()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());
      var result = handler.Execute("ls -a -d");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo("  phobos\r\n+ Luna\r\n  Deimos\r\n  Adrastea Phobos"));
    }

    [Test]
    public void SubCommand()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());
      var result = handler.Execute("replace < (ga -a name) B c -c");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success), result.Message);
      Assert.That(result.Message, Is.EqualTo("cebhionn"));
    }

    [Test]
    public void SubSubCommand()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());
      var result = handler.Execute("replace < (ga -a < (echo name)) B c -c");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo("cebhionn"));
    }

    [Test]
    public void MultipleSubCommands()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());
      var result = handler.Execute("replace < (ga -a name) B < (echo c) -c");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo("cebhionn"));
    }

    [Test]
    public void CommandChaining()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());
      var result = handler.Execute("ga -a name > replace $~$ B c -c");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success), result.Message);
      Assert.That(result.Message, Is.EqualTo("cebhionn"));
    }

    [Test]
    public void MultipleCommandChaining()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());
      var result = handler.Execute("ga -a name > replace $~$ B c -c > split -s h $~$ (echo $current$)");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success), result.Message);
      Assert.That(result.Message, Contains.Substring("ceb"));
      Assert.That(result.Message, Contains.Substring("ionn"));
    }

    [Test]
    public void EscapedParameterDelimiter()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());
      var result = handler.Execute("echo lorem \\(ipsum dolor\\)");

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo("lorem (ipsum dolor)"));
    }

    [Test]
    public void AddAlias()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());

      var result = handler.AddCommandAlias("a", "ls");
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      // Verify the alias was added
      var exResult = handler.Execute("a");
      Assert.That(exResult.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(exResult.Message, Contains.Substring("Luna"));
      Assert.That(exResult.Message, Contains.Substring("Deimos"));
      Assert.That(exResult.Message, Contains.Substring("phobos"));
      Assert.That(exResult.Message, Contains.Substring("Adrastea Phobos"));
    }

    [Test]
    public void AddAliasWithParameters()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());

      var result = handler.AddCommandAlias("aa", "ls", "-a", "-d");
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      // Verify the alias was added
      var exResult = handler.Execute("aa");
      Assert.That(exResult.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(exResult.Message, Contains.Substring("  phobos\r\n+ Luna\r\n  Deimos\r\n  Adrastea Phobos"));
    }

    [Test]
    public void AddAliasSameAsCommand()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());

      var result = handler.AddCommandAlias("search", "csearch");
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void AddAliasSameAsAlias()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = _testContent;
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());

      handler.AddCommandAlias("aa", "ls");

      var result = handler.AddCommandAlias("aa", "ga");
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void ScriptArgsNoEscaping()
    {
      var ctx = new Mod.Context();
      ctx.CurrentItem = ctx.CurrentDatabase.GetRootItem();
      var handler = new Mod.CommandHandler(ctx, new TextOutputFormatter());

      var result = handler.Execute("args (media library)");
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringContaining(ItemIDs.MediaLibraryRoot.ToString()));
    }
  }
}
