using NUnit.Framework;
using Revolver.Core;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("NewID")]
  public class NewID : BaseCommandTest
  {
    [Test]
    public void GenerateNewID()
    {
      var cmd = new Cmd.NewID();
      InitCommand(cmd);

      var result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message.ToLower(), Is.StringMatching(@"\{[\da-f]{8}-[\da-f]{4}-[\da-f]{4}-[\da-f]{4}-[\da-f]{12}\}"));
    }
  }
}