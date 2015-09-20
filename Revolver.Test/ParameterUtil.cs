using NUnit.Framework;
using System.Collections.Specialized;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ParameterUtil")]
  public class ParameterUtil
  {
    [Test]
    public void ExtractParameters_SingleNamed()
    {
      StringDictionary named = new StringDictionary();
      string[] numbered = null;
      string[] input = new string[] { "-name", "value" };

      Revolver.Core.ParameterUtil.ExtractParameters(out named, out numbered, input, null);

      Assert.AreEqual(1, named.Count);
      Assert.AreEqual(0, numbered.Length);
      Assert.AreEqual("value", named["name"]);
    }

    [Test]
    public void ExtractParameters_MultipleNamed()
    {
      StringDictionary named = new StringDictionary();
      string[] numbered = null;
      string[] input = new string[] { "-name", "value", "-sd", "somedata" };

      Revolver.Core.ParameterUtil.ExtractParameters(out named, out numbered, input, null);

      Assert.AreEqual(2, named.Count);
      Assert.AreEqual(0, numbered.Length);
      Assert.AreEqual("value", named["name"]);
      Assert.AreEqual("somedata", named["sd"]);
    }

    [Test]
    public void ExtractParameters_SingleNamedMissingValue()
    {
      StringDictionary named = new StringDictionary();
      string[] numbered = null;
      string[] input = new string[] { "-name" };

      Revolver.Core.ParameterUtil.ExtractParameters(out named, out numbered, input, null);

      Assert.AreEqual(1, named.Count);
      Assert.AreEqual(0, numbered.Length);
      Assert.AreEqual(string.Empty, named["name"]);
    }

    [Test]
    public void ExtractParameters_MultipleNamedMissingValue()
    {
      StringDictionary named = new StringDictionary();
      string[] numbered = null;
      string[] input = new string[] { "-name", "value", "-mv", "-sd", "somedata", "mv2" };

      Revolver.Core.ParameterUtil.ExtractParameters(out named, out numbered, input);

      Assert.AreEqual(3, named.Count);
      Assert.AreEqual(1, numbered.Length);
      Assert.AreEqual("value", named["name"]);
      Assert.AreEqual(string.Empty, named["mv"]);
      Assert.AreEqual("somedata", named["sd"]);
      Assert.AreEqual("mv2", numbered[0]);
    }

    [Test]
    public void ExtractParameters_SingleNumbered()
    {
      StringDictionary named = new StringDictionary();
      string[] numbered = null;
      string[] input = new string[] { "myvalue" };

      Revolver.Core.ParameterUtil.ExtractParameters(out named, out numbered, input, null);

      Assert.AreEqual(0, named.Count);
      Assert.AreEqual(1, numbered.Length);
      Assert.AreEqual("myvalue", numbered[0]);
    }

    [Test]
    public void ExtractParameters_MutipleNumbered()
    {
      StringDictionary named = new StringDictionary();
      string[] numbered = null;
      string[] input = new string[] { "value1", "value2", "value3" };

      Revolver.Core.ParameterUtil.ExtractParameters(out named, out numbered, input);

      Assert.AreEqual(0, named.Count);
      Assert.AreEqual(3, numbered.Length);
      Assert.AreEqual("value1", numbered[0]);
      Assert.AreEqual("value2", numbered[1]);
      Assert.AreEqual("value3", numbered[2]);
    }

    [Test]
    public void ExtractParameters_SingleMixed()
    {
      StringDictionary named = new StringDictionary();
      string[] numbered = null;
      string[] input = new string[] { "value1", "-named", "value2" };

      Revolver.Core.ParameterUtil.ExtractParameters(out named, out numbered, input, null);

      Assert.AreEqual(1, named.Count);
      Assert.AreEqual(1, numbered.Length);
      Assert.AreEqual("value1", numbered[0]);
      Assert.AreEqual("value2", named["named"]);
    }

    [Test]
    public void ExtractParameters_MultipleMixed()
    {
      StringDictionary named = new StringDictionary();
      string[] numbered = null;
      string[] input = new string[] { "value1", "-named", "value2", "-named2", "value3", "value4", "value5", "-named3", "value6", "value7" };

      Revolver.Core.ParameterUtil.ExtractParameters(out named, out numbered, input, null);

      Assert.AreEqual(3, named.Count);
      Assert.AreEqual(4, numbered.Length);
      Assert.AreEqual("value1", numbered[0]);
      Assert.AreEqual("value4", numbered[1]);
      Assert.AreEqual("value5", numbered[2]);
      Assert.AreEqual("value7", numbered[3]);
      Assert.AreEqual("value2", named["named"]);
      Assert.AreEqual("value3", named["named2"]);
      Assert.AreEqual("value6", named["named3"]);
    }

    [Test]
    public void ExtractParameters_WithFlags()
    {
      StringDictionary named = new StringDictionary();
      string[] numbered = null;
      string[] input = new string[] { "-f2" };
      string[] flags = new string[] { "f1", "f2", "f3" };

      Revolver.Core.ParameterUtil.ExtractParameters(out named, out numbered, input, flags);

      Assert.AreEqual(1, named.Count);
      Assert.AreEqual(0, numbered.Length);
      Assert.IsTrue(named.ContainsKey("f2"));
      Assert.AreEqual(string.Empty, named["f2"]);
    }

    [Test]
    public void ExtractParameters_MultipleMixedWithFlags()
    {
      StringDictionary named = new StringDictionary();
      string[] numbered = null;
      string[] input = new string[] { "value1", "-named", "value2", "-named2", "value3", "-f3", "value4", "-f2" };
      string[] flags = new string[] { "f1", "f2", "f3" };

      Revolver.Core.ParameterUtil.ExtractParameters(out named, out numbered, input, flags);

      Assert.AreEqual(4, named.Count);
      Assert.AreEqual(2, numbered.Length);
      Assert.AreEqual("value1", numbered[0]);
      Assert.AreEqual("value4", numbered[1]);
      Assert.AreEqual("value2", named["named"]);
      Assert.AreEqual("value3", named["named2"]);
      Assert.IsTrue(named.ContainsKey("f2"));
      Assert.AreEqual(string.Empty, named["f2"]);
      Assert.IsTrue(named.ContainsKey("f3"));
      Assert.AreEqual(string.Empty, named["f3"]);
    }

    [Test]
    public void ExtractParameters_MultipleMixedWithFlagsBlockingNamed()
    {
      StringDictionary named = new StringDictionary();
      string[] numbered = null;
      string[] input = new string[] { "value1", "-named", "value2", "-named2", "-f3", "value3", "value4", "-f2" };
      string[] flags = new string[] { "f1", "f2", "f3" };

      Revolver.Core.ParameterUtil.ExtractParameters(out named, out numbered, input, flags);

      Assert.AreEqual(4, named.Count);
      Assert.AreEqual(3, numbered.Length);
      Assert.AreEqual("value1", numbered[0]);
      Assert.AreEqual("value3", numbered[1]);
      Assert.AreEqual("value4", numbered[2]);
      Assert.AreEqual("value2", named["named"]);
      Assert.AreEqual(string.Empty, named["named2"]);
      Assert.IsTrue(named.ContainsKey("f2"));
      Assert.AreEqual(string.Empty, named["f2"]);
      Assert.IsTrue(named.ContainsKey("f3"));
      Assert.AreEqual(string.Empty, named["f3"]);
    }

    [Test]
    public void ExtractParameters_NumberedStartsWithSlash()
    {
      StringDictionary named = new StringDictionary();
      string[] numbered = null;
      string[] input = new string[] { "value1", "\\-named", "value2" };

      Revolver.Core.ParameterUtil.ExtractParameters(out named, out numbered, input, null);

      Assert.AreEqual(0, named.Count);
      Assert.AreEqual(3, numbered.Length);
      Assert.AreEqual("value1", numbered[0]);
      Assert.AreEqual("-named", numbered[1]);
      Assert.AreEqual("value2", numbered[2]);
    }

    [Test]
    public void RemoveParameter_Flag()
    {
      string[] args = new string[] { "num1", "-named1", "val1", "-rem1", "num2" };
      args = Revolver.Core.ParameterUtil.RemoveParameter(args, "-rem1", 0);
      Assert.AreEqual(4, args.Length);
      Assert.AreEqual("num1", args[0]);
      Assert.AreEqual("-named1", args[1]);
      Assert.AreEqual("val1", args[2]);
      Assert.AreEqual("num2", args[3]);
    }

    [Test]
    public void RemoveParameter_Single()
    {
      string[] args = new string[] { "num1", "-named1", "val1", "-rem1", "remval1" };
      args = Revolver.Core.ParameterUtil.RemoveParameter(args, "-rem1", 1);
      Assert.AreEqual(3, args.Length);
      Assert.AreEqual("num1", args[0]);
      Assert.AreEqual("-named1", args[1]);
      Assert.AreEqual("val1", args[2]);
    }

    [Test]
    public void RemoveParameter_Multiple()
    {
      string[] args = new string[] { "num1", "-named1", "val1", "-rem1", "remval1", "remval2", "remval3", "-f1" };
      args = Revolver.Core.ParameterUtil.RemoveParameter(args, "-rem1", 3);
      Assert.AreEqual(4, args.Length);
      Assert.AreEqual("num1", args[0]);
      Assert.AreEqual("-named1", args[1]);
      Assert.AreEqual("val1", args[2]);
      Assert.AreEqual("-f1", args[3]);
    }

    [Test]
    public void RemoveParameter_Mixed()
    {
      string[] args = new string[] { "-rem1", "remval1", "num2", "num1", "-named1", "val1" };
      args = Revolver.Core.ParameterUtil.RemoveParameter(args, "-rem1", 1);
      Assert.AreEqual(4, args.Length);
      Assert.AreEqual("num2", args[0]);
      Assert.AreEqual("num1", args[1]);
      Assert.AreEqual("-named1", args[2]);
      Assert.AreEqual("val1", args[3]);
    }

    [Test]
    public void RemoveParameter_Missing()
    {
      string[] args = new string[] { "num2", "num1", "-named1", "val1" };
      args = Revolver.Core.ParameterUtil.RemoveParameter(args, "-rem1", 1);
      Assert.AreEqual(4, args.Length);
      Assert.AreEqual("num2", args[0]);
      Assert.AreEqual("num1", args[1]);
      Assert.AreEqual("-named1", args[2]);
      Assert.AreEqual("val1", args[3]);
    }
  }
}