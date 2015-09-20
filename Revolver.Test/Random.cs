using NUnit.Framework;
using Revolver.Core;
using System.Collections.Generic;
using System.Linq;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("Random")]
  public class Random
  {
    [Test]
    public void NoMaximum()
    {
      var cmd = new Cmd.Random();
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void OnlyMaximum()
    {
      var cmd = new Cmd.Random();
      cmd.Maximum = 10;
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      var num = int.Parse(result.Message);

      Assert.That(num, Is.GreaterThanOrEqualTo(0));
      Assert.That(num, Is.LessThanOrEqualTo(10));
    }

    [Test]
    public void MaximumAndMinimum()
    {
      var cmd = new Cmd.Random();
      cmd.Minimum = 50;
      cmd.Maximum = 5000;
      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      var num = int.Parse(result.Message);

      Assert.That(num, Is.GreaterThanOrEqualTo(50));
      Assert.That(num, Is.LessThanOrEqualTo(5000));
    }

    [Test]
    public void Fractions()
    {
      var cmd = new Cmd.Random();
      cmd.Maximum = 10;
      cmd.FractalDigits = 3;
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
      var cmd = new Cmd.Random();
      cmd.Minimum = -20;
      cmd.Maximum = -10;
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
        var command = new Cmd.Random();
        command.Maximum = int.MaxValue;
        commands.Add(command);
      }

      var returnedValues = new List<int>();
       
      foreach(var command in commands)
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
  }
}