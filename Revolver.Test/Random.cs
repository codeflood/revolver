using System;
using NUnit.Framework;
using Revolver.Core;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Shell.Applications.ContentEditor;
using Cmd = Revolver.Core.Commands;
using Sitecore;

namespace Revolver.Test
{
  [TestFixture]
  [Category("Random")]
  public class Random
  {
    [Test]
    public void FractalWithDate()
    {
      var cmd = new Cmd.Random
      {
        FractalDigits = 2,
        GenerateDates = true
      };

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void FractalWithTime()
    {
      var cmd = new Cmd.Random
      {
        FractalDigits = 2,
        GenerateDates = true
      };

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void OnlyMaximum()
    {
      var cmd = new Cmd.Random
      {
        Maximum = "10"
      };
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      var num = int.Parse(result.Message);

      Assert.That(num, Is.GreaterThanOrEqualTo(0));
      Assert.That(num, Is.LessThanOrEqualTo(10));
    }

    [Test]
    public void MaximumAndMinimum()
    {
      var cmd = new Cmd.Random
      {
        Minimum = "50",
        Maximum = "5000"
      };
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      var num = int.Parse(result.Message);

      Assert.That(num, Is.GreaterThanOrEqualTo(50));
      Assert.That(num, Is.LessThanOrEqualTo(5000));
    }

    [Test]
    public void HumanFriendlyWithoutDateOrTime()
    {
      var cmd = new Cmd.Random
      {
        DateTimeFormatHuman = true
      };
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void Fractions()
    {
      var cmd = new Cmd.Random
      {
        Maximum = "10",
        FractalDigits = 3
      };
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      var num = double.Parse(result.Message);

      Assert.That(num, Is.GreaterThanOrEqualTo(0));
      Assert.That(num, Is.LessThanOrEqualTo(10));

      var index = result.Message.IndexOf('.');
      Assert.That(index, Is.EqualTo(result.Message.Length - 4), message: "Incorrect number of decimal digits in " + result.Message);
    }

    [Test]
    public void Negative()
    {
      var cmd = new Cmd.Random
      {
        Minimum = "-20",
        Maximum = "-10"
      };
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      var num = int.Parse(result.Message);

      Assert.That(num, Is.GreaterThanOrEqualTo(-20));
      Assert.That(num, Is.LessThanOrEqualTo(-10));
    }

    [Test]
    public void MultipleInstances()
    {
      var commands = new List<Cmd.Random>();

      for (int i = 0; i < 10; i++)
      {
        var command = new Cmd.Random
        {
          Maximum = int.MaxValue.ToString()
        };
        commands.Add(command);
      }

      var returnedValues = new List<int>();

      foreach (var command in commands)
      {
        var result = command.Run();
        returnedValues.Add(int.Parse(result.Message));
      }

      int firstValue = returnedValues.First();

      if (returnedValues.Any(x => x != firstValue))
      {
        return;
      }

      Assert.Fail("Multiple instances of random command return same value.");
    }

    [Test]
    public void DateNoRange()
    {
      var command = new Cmd.Random
      {
        GenerateDates = true
      };

      var result = command.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      var generatedDate = DateUtil.ParseDateTime(result.Message, System.DateTime.UtcNow);
    }

    [Test]
    public void DateNoRangeHumanFriendly()
    {
      var command = new Cmd.Random
      {
        GenerateDates = true,
        DateTimeFormatHuman = true
      };

      var result = command.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      var generatedDate = System.DateTime.Parse(result.Message);
    }

    [Test]
    public void DateWithLimits()
    {
      var command = new Cmd.Random
      {
        GenerateDates = true,
        Minimum = "2012-01-01",
        Maximum = "2012-06-01"
      };

      var result = command.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      var generatedDate = DateUtil.ParseDateTime(result.Message, System.DateTime.UtcNow);
      Assert.That(generatedDate, Is.LessThanOrEqualTo(new System.DateTime(2012, 6, 1)));
      Assert.That(generatedDate, Is.GreaterThanOrEqualTo(new System.DateTime(2012, 1, 1)));
    }
  }
}