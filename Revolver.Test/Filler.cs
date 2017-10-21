using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("Filler")]
  public class Filler : BaseCommandTest
  {
    [Test]
    public void WithBothParagraphsAndSentences_ReturnsError()
    {
      // arange
      var cmd = new Cmd.Filler();
      InitCommand(cmd);
      cmd.Paragraphs = true;
      cmd.Sentences = true;

      // act
      var output = cmd.Run();

      // assert
      Assert.That(output.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void NoParameters_GeneratesSomething()
    {
      // arange
      var cmd = new Cmd.Filler();
      InitCommand(cmd);

      // act
      var output = cmd.Run();

      // assert
      Assert.That(output.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(output.Message.Length, Is.GreaterThan(10));
    }

    [Test]
    public void WithBothLimits_GeneratesWordCountWithinRange()
    {
      // arange
      var cmd = new Cmd.Filler();
      InitCommand(cmd);
      cmd.FirstLimit = 5;
      cmd.SecondLimit = 10;

      // act
      var output = cmd.Run();

      // assert
      Assert.That(output.Status, Is.EqualTo(CommandStatus.Success));

      var spaceCount = output.Message.Count(x => x == ' ');
      Assert.That(spaceCount, Is.GreaterThanOrEqualTo(5));
      Assert.That(spaceCount, Is.LessThanOrEqualTo(10));
    }

    [Test]
    public void WithUpperLimitOnly_GeneratesWordCountLessThanLimit()
    {
      // arange
      var cmd = new Cmd.Filler();
      InitCommand(cmd);
      cmd.FirstLimit = 20;

      // act
      var output = cmd.Run();

      // assert
      Assert.That(output.Status, Is.EqualTo(CommandStatus.Success));

      var spaceCount = output.Message.Count(x => x == ' ');
      Assert.That(spaceCount, Is.LessThanOrEqualTo(20));
    }

    [Test]
    public void WithParagraphFlag_GeneratesParagraphs()
    {
      // arrange
      var cmd = new Cmd.Filler();
      InitCommand(cmd);
      cmd.FirstLimit = 2;
      cmd.SecondLimit = 5;
      cmd.Paragraphs = true;
      cmd.ParagraphDelimiter = "??delim??";

      // act
      var output = cmd.Run();

      // assert
      Assert.That(output.Status, Is.EqualTo(CommandStatus.Success));

      var paragraphCount = output.Message.Split(new[] {"??delim??"}, StringSplitOptions.None).Count();
      var spaceCount = output.Message.Count(x => x == ' ');
      Assert.That(spaceCount, Is.GreaterThan(4)); // 2 paragraphs minimum, 2 words minimum in each
      Assert.That(paragraphCount, Is.GreaterThanOrEqualTo(2));
      Assert.That(paragraphCount, Is.LessThanOrEqualTo(5));
    }

    [Test]
    public void WithSentencesFlag_GeneratesSentences()
    {
      // arrange
      var cmd = new Cmd.Filler();
      InitCommand(cmd);
      cmd.FirstLimit = 2;
      cmd.SecondLimit = 5;
      cmd.Sentences = true;

      // act
      var output = cmd.Run();

      // assert
      Assert.That(output.Status, Is.EqualTo(CommandStatus.Success));

      var sentenceCount = output.Message.Count(x => x == '.');
      var spaceCount = output.Message.Count(x => x == ' ');
      Assert.That(spaceCount, Is.GreaterThan(4)); // 2 sentences minimum, 2 words minimum in each
      Assert.That(sentenceCount, Is.GreaterThanOrEqualTo(2));
      Assert.That(sentenceCount, Is.LessThanOrEqualTo(5));
    }

    [Test]
    public void WithSameLimits_GeneratesExactWordCount()
    {
      // arrange
      var cmd = new Cmd.Filler();
      InitCommand(cmd);
      cmd.FirstLimit = 3;
      cmd.SecondLimit = 3;

      // act
      var output = cmd.Run();

      // assert
      Assert.That(output.Status, Is.EqualTo(CommandStatus.Success));

      var spaceCount = output.Message.Count(x => x == ' ');
      Assert.That(spaceCount, Is.EqualTo(2));
    }
  }
}