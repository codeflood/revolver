using NUnit.Framework;
using Revolver.Core;
using Sitecore.Globalization;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
	[TestFixture]
	[Category("PrintCurrentLanguage")]
	public class PrintCurrentLanguage : BaseCommandTest
	{
		Cmd.PrintCurrentLanguage _pcl = null;

		[TestFixtureSetUp]
		public void Init()
		{
			_pcl = new Revolver.Core.Commands.PrintCurrentLanguage();
			base.InitCommand(_pcl);
		}

		[Test]
		public void English()
		{
			_context.CurrentLanguage = Language.Parse("en");
			var result = _pcl.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("English [en]", result.Message);
		}

		[Test]
		public void Danish()
		{
			_context.CurrentLanguage = Language.Parse("da");
			var result = _pcl.Run();
			Assert.AreEqual(CommandStatus.Success, result.Status);
			Assert.AreEqual("Danish [da]", result.Message);
		}
	}
}
