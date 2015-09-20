using NUnit.Framework;
using Revolver.Core.Exceptions;
using Sitecore.Data.Items;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ExpressionParser")]
  public class ExpressionParser : BaseCommandTest
  {
    Item _testTreeRoot = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      InitContent();
      _testTreeRoot = TestUtil.CreateContentFromFile("TestResources\\narrow tree with duplicate names.xml", _testRoot);
    }

    [Test]
    public void EvaluateExpression()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = a");
      Assert.That(res, Is.True);
    }

    [SetUp]
    public void SetUp()
    {
      _context.CurrentItem = _testTreeRoot;
    }

    [Test]
    public void EvaluateExpression_And()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = a and b = b");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateExpression_And2()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = a and c = c");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateExpression_AndEF()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = c and b = b");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateExpression_AndEF2()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = a and b = c");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateExpression_MultiAnd()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = a and b = b and c = c");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateExpression_MultiAndEF()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = a and b = b and d = c");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateExpression_MultiAndEF2()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = c and b = b and c = c");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateExpression_Or1()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = a or b = b");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateExpression_Or2()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = c or b = b");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateExpression_Or3()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = a or b = c");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateExpression_OrEF()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = b or b = a");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateExpression_MultipleOr()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = b or b = a or 2 = 2 as number");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateExpression_ExtraSpaces()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "  a =  b or  b = a");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateExpression_MixedAndOr()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = b and b = b or 3 = 3 as number");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateExpression_MixedAndOr2()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateExpression(_context, "a = b or b = b and 3 = 3 as number");
      Assert.That(res, Is.True);
    }


    [Test]
    public void EvaluateSingleExpression_StringsEqual()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "a = a");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_StringsNotEqual()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "a != b");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_StringsLess()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "a < aa");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_StringsLessOrEqual()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "a <= b");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_StringsGreater()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "b > a");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_StringsGreaterOrEqual()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "b >= a");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_StringsEqualEF()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "a = b");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_StringsNotEqualEF()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "a != a");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_StringsLessEF()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "b < a");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_NumEqual()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "6 = 6 as number");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_NumNotEqual()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "7 != 1 as number");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_NumLess()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "3 < 4 as number");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_NumLessOrEqual()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "2 <= 100 as number");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_NumGreater()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "5 > 2 as number");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_NumGreaterOrEqual()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "3 >= 2 as number");
      Assert.That(res, Is.True);
    }

    [Test]
    [ExpectedException(typeof(ExpressionException))]
    public void EvaluateSingleExpression_InvalidInpVal1NotNum()
    {
      Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "t >= 2 as number");
    }

    [Test]
    [ExpectedException(typeof(ExpressionException))]
    public void EvaluateSingleExpression_InvalidInpVal2NotNum()
    {
      Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "6 >= (2007-03-01) as number");
    }

    [Test]
    public void EvaluateSingleExpression_DateEqual()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "2007-01-12 = (12 Jan 2007) as date");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_DateNotEqual()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "2007-01-12 != 12/02/2007 as date");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_DateLess()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "2007-01-12 < (13 Jan 2007) as date");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_DateLessOrEqual()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "2007-01-12 <= 12/01/2008 as date");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_DateGreater()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "2007-01-12 > 12/01/2006 as date");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_DateGreaterOrEqual()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "2007-01-12 >= 12-04-2004 as date");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_DateEqualEF()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "2007-01-12 = 12/11/2007 as date");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_DateGreaterOrEqualEF()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "2007-01-12 >= 12/11/2008 as date");
      Assert.That(res, Is.False);
    }

    [Test]
    [ExpectedException(typeof(ExpressionException))]
    public void EvaluateSingleExpression_InvalidDate()
    {
      Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "2007-01-12 = 2009-15-20 as date");
    }

    [Test]
    [ExpectedException(typeof(ExpressionException))]
    public void EvaluateSingleExpression_NotNumber()
    {
      Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "2007-01-12 = 7 as number");
    }

    [Test]
    public void EvaluateSingleExpression_UsingFieldString()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "@title = a");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_UsingFieldWithSpacesString()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "(@__created by) = sitecore\\admin");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_UsingValueWithSpacesString()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "(@__created by) != (not admin)");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_UsingFieldDate()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "2016-01-12 > @__created as date");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_UsingAttributeString()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "@@name = Callirrhoe");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_UsingAttributeAndFieldString()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "@@name = @title");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_IgnoreCase()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "cAsE1 = CASE1 with ignorecase");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_IgnoreCaseEF()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "cAsE1 = CASE2 with ignorecase");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_IgnoreDecimal()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "17.3 = 17.947 as number with ignoredecimal");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_IgnoreDecimalEF()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "17.3 = 18 as number with ignoredecimal");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_CeilingGreaterThan()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "18.8 > 18.4 as number with ceiling");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_CeilingGreaterThanEF()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "18.8 > 19 as number with ceiling");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_FloorLessThan()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "16.4 < 17.4 as number with floor");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_FloorLessThanEF()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "17 < 17.4 as number with floor");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_Round()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "15.3 = 14.8 as number with round");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_StartsWithPresent()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "sitecore [ si");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_StartsWithMissing()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "sitecore [ boo");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_EndsWithPresent()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "sitecore ] core");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_EndsWithMissing()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "sitecores ] core");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_ContainsPresent()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "sitecore ? ec");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_ContainsMissing()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "sitecore ? ct");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_Negate()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "not true");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_Negate2()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "not false");
      Assert.That(res, Is.True);
    }

    [Test]
    public void EvaluateSingleExpression_IsEmpty_NotEmpty()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "isempty something");
      Assert.That(res, Is.False);
    }

    [Test]
    public void EvaluateSingleExpression_IsEmpty_Empty()
    {
      var res = Revolver.Core.ExpressionParser.EvaluateSingleExpression(_context, "isempty ()");
      Assert.That(res, Is.True);
    }
  }
}