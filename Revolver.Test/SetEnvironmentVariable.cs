using NUnit.Framework;
using Revolver.Core;
using System.Collections;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
	[TestFixture]
	[Category("SetEnvironmentVariable")]
	public class SetEnvironmentVariable : BaseCommandTest
	{ 
    [SetUp]
    public void SetUp()
    {
      _context.EnvironmentVariables.Clear();
    }

    [Test]
    public void ListAll_None()
    {
      var cmd = new Cmd.SetEnvironmentVariable();
      InitCommand(cmd);

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("0 variables"));
    }

    [Test]
    public void ListAll_Some()
    {
      var cmd = new Cmd.SetEnvironmentVariable();
      InitCommand(cmd);

      _context.EnvironmentVariables.Add("lorem", "ipsum");
      _context.EnvironmentVariables.Add("dolor", "sit");

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("2 variables"));
      Assert.That(result.Message, Is.StringMatching("lorem\\s+ipsum"));
      Assert.That(result.Message, Is.StringMatching("dolor\\s+sit"));
    }

    [Test]
    public void SetReserved()
    {
      var cmd = new Cmd.SetEnvironmentVariable();
      InitCommand(cmd);

      cmd.Name = "prev";
      cmd.Value = "something";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void SetNotExisting()
    {
      var cmd = new Cmd.SetEnvironmentVariable();
      InitCommand(cmd);

      _context.EnvironmentVariables.Add("lorem", "ipsum");
      cmd.Name = "dolor";
      cmd.Value = "sit";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_context.EnvironmentVariables.Count, Is.EqualTo(2));
      Assert.That(_context.EnvironmentVariables, Contains.Item(new DictionaryEntry("dolor", "sit")));
    }

    [Test]
    public void SetExisting()
    {
      var cmd = new Cmd.SetEnvironmentVariable();
      InitCommand(cmd);

      _context.EnvironmentVariables.Add("lorem", "ipsum");
      cmd.Name = "lorem";
      cmd.Value = "dolor";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_context.EnvironmentVariables.Count, Is.EqualTo(1));
      Assert.That(_context.EnvironmentVariables, Contains.Item(new DictionaryEntry("lorem", "dolor")));
    }

    [Test]
    public void ClearExisting()
    {
      var cmd = new Cmd.SetEnvironmentVariable();
      InitCommand(cmd);

      _context.EnvironmentVariables.Add("lorem", "ipsum");
      _context.EnvironmentVariables.Add("dolor", "sit");
      cmd.Name = "lorem";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("Cleared"));
      Assert.That(_context.EnvironmentVariables.Count, Is.EqualTo(1));
      Assert.That(_context.EnvironmentVariables, Contains.Item(new DictionaryEntry("dolor", "sit")));
    }

    [Test]
    public void ClearNotExisting()
    {
      var cmd = new Cmd.SetEnvironmentVariable();
      InitCommand(cmd);

      _context.EnvironmentVariables.Add("lorem", "ipsum");
      _context.EnvironmentVariables.Add("dolor", "sit");
      cmd.Name = "amed";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Contains.Substring("wasn't set"));
      Assert.That(_context.EnvironmentVariables.Count, Is.EqualTo(2));
      Assert.That(_context.EnvironmentVariables, Contains.Item(new DictionaryEntry("lorem", "ipsum")));
      Assert.That(_context.EnvironmentVariables, Contains.Item(new DictionaryEntry("dolor", "sit")));
    }

    [Test]
    public void SetExistingWithPrev()
    {
      var cmd = new Cmd.SetEnvironmentVariable();
      InitCommand(cmd);

      _context.EnvironmentVariables.Add("lorem", "ipsum");
      cmd.Name = "lorem";
      cmd.Value = "$prev dolor";

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(_context.EnvironmentVariables.Count, Is.EqualTo(1));
      Assert.That(_context.EnvironmentVariables, Contains.Item(new DictionaryEntry("lorem", "ipsum dolor")));
    }
	}
}
