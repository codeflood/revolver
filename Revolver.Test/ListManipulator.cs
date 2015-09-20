using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ListManipulator")]
  public class ListManipulator : BaseCommandTest
  {
    Cmd.ListManipulator cmd = null;

    [Test]
    public void AddWithCurrentEmpty()
    {
      cmd = new Cmd.ListManipulator();
      base.InitCommand(cmd);
      cmd.Add = true;
      cmd.Delimiter = "|";
      cmd.Element = "boo";
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual("boo", result.Message);
    }

    [Test]
    public void Add()
    {
      cmd = new Cmd.ListManipulator();
      base.InitCommand(cmd);
      cmd.Add = true;
      cmd.Input = "a-b-c";
      cmd.Delimiter = "-";
      cmd.Element = "d";
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual("a-b-c-d", result.Message);
    }

    [Test]
    public void AddWithAlreadyExists()
    {
      string id1 = ID.NewID.ToString();
      string id2 = ID.NewID.ToString();
      string id3 = ID.NewID.ToString();
      string input = id1 + "|" + id2 + "|" + id3;

      cmd = new Cmd.ListManipulator();
      base.InitCommand(cmd);
      cmd.Add = true;
      cmd.Input = input;
      cmd.Delimiter = "|";
      cmd.Element = id2;
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual(input, result.Message);
    }

    [Test]
    public void RemoveWithCurrentEmpty()
    {
      cmd = new Cmd.ListManipulator();
      base.InitCommand(cmd);
      cmd.Remove = true;
      cmd.Delimiter = "-";
      cmd.Element = "a";
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual("", result.Message);
    }

    [Test]
    public void Remove()
    {
      string id1 = ID.NewID.ToString();
      string id2 = ID.NewID.ToString();
      string id3 = ID.NewID.ToString();
      string input = id1 + "|" + id2 + "|" + id3;

      cmd = new Cmd.ListManipulator();
      base.InitCommand(cmd);
      cmd.Remove = true;
      cmd.Input = input;
      cmd.Delimiter = "|";
      cmd.Element = id2;
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual(id1 + "|" + id3, result.Message);
    }

    [Test]
    public void RemoveNotFound()
    {
      cmd = new Cmd.ListManipulator();
      base.InitCommand(cmd);
      cmd.Remove = true;
      cmd.Input = "a-b-c";
      cmd.Delimiter = "-";
      cmd.Element = "d";
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual("a-b-c", result.Message);
    }

    [Test]
    public void MissingOperation()
    {
      cmd = new Cmd.ListManipulator();
      base.InitCommand(cmd);
      cmd.Input = "a-b-c";
      cmd.Delimiter = "-";
      cmd.Element = "d";
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }

    [Test]
    public void MissingElement()
    {
      cmd = new Cmd.ListManipulator();
      base.InitCommand(cmd);
      cmd.Remove = true;
      cmd.Input = "a-b-c";
      cmd.Delimiter = "-";
      CommandResult result = cmd.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }

    [Test]
    public void ReverseOrder()
    {
      cmd = new Cmd.ListManipulator();
      base.InitCommand(cmd);

      cmd.Input = "a-b-c";
      cmd.Delimiter = "-";
      cmd.OrderList = true;
      cmd.ReverseOrder = true;
      
      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.EqualTo("c-b-a"));
    }

    [Test]
    public void AlphabeticalAndShuffle()
    {
      cmd = new Cmd.ListManipulator(42);
      base.InitCommand(cmd);

      cmd.Input = "a-b-c";
      cmd.Delimiter = "-";
      cmd.OrderList = true;
      cmd.Shuffle = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void Shuffle()
    {
      cmd = new Cmd.ListManipulator(42);
      base.InitCommand(cmd);

      cmd.Input = "a-b-c";
      cmd.Delimiter = "-";
      cmd.Shuffle = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      // This test won't always pass as the random soring may resort in the original order
      Assert.That(result.Message, Is.Not.EqualTo("a-b-c"));
      Assert.That(result.Message, Is.StringMatching(@"\w-\w-\w"));
    }
  }
}
