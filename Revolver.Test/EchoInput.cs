using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace Revolver.Test
{
	[TestFixture]
	[Category("EchoInput")]
	public class EchoInput : BaseCommandTest
	{
		private Revolver.Core.Commands.EchoInput _echo = null;

		[SetUp]
		public void Init()
		{
			_echo = new Revolver.Core.Commands.EchoInput();
			base.InitCommand(_echo);
		}

		[Test]
		public void Text()
		{
			_echo.Input = new List<string> {"this", "is text"};
			string output = _echo.Run();
			Assert.AreEqual("this is text", output);
		}

		[Test]
		public void TextSingleWord()
		{
			_echo.Input = new List<string> {"bler"};
			string output = _echo.Run();
			Assert.AreEqual("bler", output);
		}

		[Test]
		public void TextMultipleWords()
		{
			_echo.Input =  new string[] { "this", "is", "bler" };
			string output = _echo.Run();
			Assert.AreEqual("this is bler", output);
		}

		[Test]
		public void ToFileRelative()
		{
			string path = Sitecore.Configuration.Settings.TempFolderPath;
			if (!path.Contains(":"))
				path = System.Web.HttpContext.Current.Server.MapPath(path);

			string filename = path + "\\echotext.txt";
			string content = "This is some text";

			if (File.Exists(filename))
				File.Delete(filename);
			Assert.IsFalse(File.Exists(filename));

			_echo.FileName = filename;
			_echo.Input = new string[] {content};
			
			Revolver.Core.CommandResult res = _echo.Run();
			Assert.AreEqual("Output written to " + filename, res.Message);
			Assert.IsTrue(File.Exists(filename));

			Assert.AreEqual(content + "\r\n", File.ReadAllText(filename));

			// Cleanup
			File.Delete(filename);
		}

		 

		[Test]
		public void ToFileAbsolute()
		{
			string filename = @"c:\temp\echotext.txt";
			string content = "This is some different text";

			if (File.Exists(filename))
				File.Delete(filename);
			Assert.IsFalse(File.Exists(filename));

			_echo.FileName = filename;
			_echo.Input = new string[]{content};
			Revolver.Core.CommandResult res = _echo.Run();
			Assert.AreEqual("Output written to " + filename, res.Message);
			Assert.IsTrue(File.Exists(filename));

			Assert.AreEqual(content + "\r\n", File.ReadAllText(filename));

			// Cleanup
			File.Delete(filename);
		}

		[Test]
		public void ToNonExistingDir()
		{
			string dir = @"c:\temp\nonexfolder";
			string filename = dir + @"\echotext.txt";
			string content = "This is some different text";

			if (Directory.Exists(dir))
				Directory.Delete(dir);

			if (File.Exists(filename))
				File.Delete(filename);
			Assert.IsFalse(File.Exists(filename));

			_echo.FileName = filename;
			_echo.Input = new string[]{content};

			Revolver.Core.CommandResult res = _echo.Run();
			Assert.AreEqual("Output written to " + filename, res.Message);
			Assert.IsTrue(File.Exists(filename));

			Assert.AreEqual(content + "\r\n", File.ReadAllText(filename));

			// Cleanup
			File.Delete(filename);
			Directory.Delete(dir);
		}

		[Test]
		public void EscapedParameter()
		{
			_echo.FileName = "something.txt";
			string output = _echo.Run();
			Assert.AreEqual(string.Empty, output);
		}

		[Test]
		public void ToFileRelativeAppend()
		{
			string path = Sitecore.Configuration.Settings.TempFolderPath;
			if (!path.Contains(":"))
				path = System.Web.HttpContext.Current.Server.MapPath(path);

			string filename = path + "\\echotext.txt";
			string content = "This is some text";
			Assert.IsFalse(File.Exists(filename));

			_echo.FileName = "echotext.txt";
			_echo.Input = new string[] {content};
			Revolver.Core.CommandResult res = _echo.Run();
			Assert.AreEqual("Output written to " + filename, res.Message);
			Assert.IsTrue(File.Exists(filename));

			_echo.FileName = "echotext.txt";
			_echo.Append = true;
			_echo.Input = new string[]{"boo"};

			res = _echo.Run();
			Assert.AreEqual("Output appended to " + filename, res.Message);

			Assert.AreEqual(content + "\r\nboo\r\n", File.ReadAllText(filename));

			// Cleanup
			File.Delete(filename);
		}

		[Test]
		public void FromFile()
		{
			string filename = @"c:\temp\echotext_input.txt";
			string content = "This is some content";
			File.WriteAllText(filename, content);

			_echo.ReadInputFromFile= true;
			_echo.FileName = filename;

			Revolver.Core.CommandResult res = _echo.Run();
			Assert.AreEqual(Revolver.Core.CommandStatus.Success, res.Status);
			Assert.AreEqual(content, res.Message);
		}

		[Test]
		public void FromFileWithSub()
		{
			string filename = @"c:\temp\echotext_input.txt";
			string content = "This is some content $var$";
			_context.EnvironmentVariables.Add("var", "with var");
			File.WriteAllText(filename, content);

			_echo.ReadInputFromFile = true;
			_echo.FileName = filename;
			Revolver.Core.CommandResult res = _echo.Run();

			Assert.AreEqual(Revolver.Core.CommandStatus.Success, res.Status);
			Assert.AreEqual("This is some content with var", res.Message);
		}

		[Test]
		public void InvalidArgs1()
		{
			_echo.Append = true;
			_echo.ReadInputFromFile = true;
			_echo.FileName = "somefile.txt";

			Revolver.Core.CommandResult res = _echo.Run();
			Assert.AreEqual(Revolver.Core.CommandStatus.Failure, res.Status);
		}

		[Test]
		public void InvalidArgs2()
		{
			_echo.ReadInputFromFile = true;
			_echo.Input = new string[]{"Hello"};
			Revolver.Core.CommandResult res = _echo.Run();
			Assert.AreEqual(Revolver.Core.CommandStatus.Failure, res.Status);
		}
	}
}
