using System.Collections.Specialized;
using NUnit.Framework;
using Revolver.Core;
using Revolver.Core.Exceptions;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SecurityModel;

namespace Revolver.Test
{
	[TestFixture]
	[Category("Util")]
  [Ignore("Not stable. Need conversion.")]
	public class Util
	{
		private Context m_context = null;
		private Item m_testRoot = null;
		private TemplateItem m_template = null;

		[TestFixtureSetUp]
		public void Init()
		{
			m_context = new Context();

			Item home = m_context.CurrentDatabase.GetItem(Constants.Paths.Home);
			m_template = m_context.CurrentDatabase.Templates[Constants.Paths.DocTemplate];

			using (new SecurityDisabler())
			{
				m_testRoot = home.Add("testroot", m_template);
			}
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			using (new SecurityDisabler())
			{
				m_testRoot.Delete();
			}
		}

		[SetUp]
		public void Setup()
		{
			m_context.CurrentDatabase = Factory.GetDatabase("master");
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");

			using (new Sitecore.SecurityModel.SecurityDisabler())
			{
				Item a = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
				a.Editing.BeginEdit();
				a["title"] = "Title a";
				a.Editing.EndEdit();

				a = m_context.CurrentDatabase.Items.GetItem(Constants.Paths.Home + "/a", Language.Parse("da"));
				a.Editing.BeginEdit();
				a["title"] = "Vendling a";
				a.Editing.EndEdit();

				Item b = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
				b.Editing.BeginEdit();
				b["title"] = "Title b";
				b.Editing.EndEdit();

				b = m_context.CurrentDatabase.Items.GetItem(Constants.Paths.Home + "/b", Language.Parse("da"));
				b.Editing.BeginEdit();
				b["title"] = "Vendling b";
				b.Editing.EndEdit();
			}
		}

		[TearDown]
		public void TearDown()
		{
			using (new Sitecore.SecurityModel.SecurityDisabler())
			{
				Item a = Factory.GetDatabase("master").GetItem(Constants.Paths.Home + "/a");
				ResetVersionsToOne(a);

				Item b = Factory.GetDatabase("master").GetItem(Constants.Paths.Home + "/b"); ;
				ResetVersionsToOne(b);

				m_testRoot.DeleteChildren();
			}
		}

		private void ResetVersionsToOne(Item item)
		{
			foreach (Language lang in item.Languages)
			{
				item = item.Database.GetItem(item.ID, lang);
				while (item.Versions.Count > 1)
					item.Versions.RemoveVersion();
			}
		}

		[Test]
		public void EvaluatePath_AFromBBRelative()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/f/bb");
			string path = Revolver.Core.Util.EvaluatePath(m_context, "../../a");
			Assert.AreEqual(Constants.Paths.Home + "/a", path);
		}

		[Test]
		public void EvaluatePath_AFromHomeRelative()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home);
			string path = Revolver.Core.Util.EvaluatePath(m_context, "a");
			Assert.AreEqual(Constants.Paths.Home + "/a", path);
		}

		[Test]
		public void EvaluatePath_AAFromFRelative()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/f");
			string path = Revolver.Core.Util.EvaluatePath(m_context, "../f/aa");
			Assert.AreEqual(Constants.Paths.Home + "/f/aa", path);
		}

		[Test]
		public void EvaluatePath_HomeFromBBAbsolute()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/f/bb");
			string path = Revolver.Core.Util.EvaluatePath(m_context, Constants.Paths.Home);
			Assert.AreEqual(Constants.Paths.Home, path);
		}

		[Test]
		public void SetContext_BFromHomeRelative()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home);
			Revolver.Core.Util.SetContext(m_context, "b");
			Assert.AreEqual(Constants.Paths.Home + "/B", m_context.CurrentItem.Paths.FullPath);
			Assert.AreEqual("master", m_context.CurrentDatabase.Name);
		}

		[Test]
		public void SetContext_BFromHomeAbsolute()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home);
			Revolver.Core.Util.SetContext(m_context, Constants.Paths.Home + "/b");
			Assert.AreEqual(Constants.Paths.Home + "/B", m_context.CurrentItem.Paths.FullPath);
			Assert.AreEqual("master", m_context.CurrentDatabase.Name);
		}

		[Test]
		public void SetContext_WebHomeFromMasterHome()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home);
			Assert.AreEqual(CommandStatus.Success, Revolver.Core.Util.SetContext(m_context, "/web" + Constants.Paths.Home).Status);
			Assert.AreEqual(Constants.Paths.Home, m_context.CurrentItem.Paths.FullPath);
			Assert.AreEqual("web", m_context.CurrentDatabase.Name);
		}

		[Test]
		public void SetContext_SetContextByPathWithLanguageAbsolute()
		{
			Assert.Ignore("Not Complete");
			Database db = m_context.CurrentDatabase;
			Language lang = m_context.CurrentLanguage;
			Item item = m_context.CurrentItem;
			Revolver.Core.Util.SetContext(m_context, Constants.Paths.Home + "/b:da");

			Assert.AreEqual(Constants.Paths.Home + "/b", m_context.CurrentItem.Paths.FullPath.ToLower(), "Item path isn't correct");
			Assert.AreEqual(db, m_context.CurrentDatabase, "Context database changed");
			Assert.AreNotEqual(lang, m_context.CurrentLanguage, "Context language didn't change");
			Assert.AreEqual("Danish", m_context.CurrentLanguage.GetDisplayName(), "Context langauge is incorrect");
		}

		[Test]
		public void SetContext_SetContextByPathWithLanguageRelative()
		{
			Assert.Ignore("Not Complete");
			Database db = m_context.CurrentDatabase;
			Language lang = m_context.CurrentLanguage;
			Item item = m_context.CurrentItem;
			Revolver.Core.Util.SetContext(m_context, "..:da");

			Assert.AreEqual(Constants.Paths.Home + "/a", m_context.CurrentItem.Paths.FullPath.ToLower(), "Item path isn't correct");
			Assert.AreEqual(db, m_context.CurrentDatabase, "Context database changed");
			Assert.AreNotEqual(lang, m_context.CurrentLanguage, "Context language changed");
			Assert.AreEqual("Danish", m_context.CurrentLanguage.GetDisplayName(), "Context language is incorrect");
		}

		[Test]
		public void SetContext_SetContextByPathWithVersionAbsolute()
		{
			Assert.Ignore("Not Complete");
			Database db = m_context.CurrentDatabase;
			Language lang = m_context.CurrentLanguage;
			Item item = m_context.CurrentItem;
			string v1title = item["title"];
			item = NewVersionSetTitle(item, "a title 2");
			item = NewVersionSetTitle(item, "a title 3");
			item = NewVersionSetTitle(item, "a title 4");

			Revolver.Core.Util.SetContext(m_context, Constants.Paths.Home + "/a::3");

			Assert.AreEqual(Constants.Paths.Home + "/a", m_context.CurrentItem.Paths.FullPath.ToLower(), "Item path isn't correct");
			Assert.AreEqual(db, m_context.CurrentDatabase, "Context database changed");
			Assert.AreEqual(lang, m_context.CurrentLanguage, "Context language changed");
			Assert.AreNotEqual(v1title, m_context.CurrentItem["title"], "Title is incorrect");
			Assert.AreEqual("a title 3", m_context.CurrentItem["title"], "Title is not of version 3");
		}

		private Item NewVersionSetTitle(Item item, string title)
		{
			Item toRet;
			using (new Sitecore.SecurityModel.SecurityDisabler())
			{
				toRet = item.Versions.AddVersion();
				toRet.Editing.BeginEdit();
				toRet["title"] = title;
				toRet.Editing.EndEdit();
			}
			return toRet;
		}

		[Test]
		public void SetContext_SetContextByPathWithVersionRelative()
		{
			Assert.Ignore("Not Complete");
			Database db = m_context.CurrentDatabase;
			Language lang = m_context.CurrentLanguage;
			Item item = m_context.CurrentDatabase.GetItem(Constants.Paths.Home);
			item = NewVersionSetTitle(item, "home 2");
			Revolver.Core.Util.SetContext(m_context, "..::2");

			Assert.AreEqual(Constants.Paths.Home, m_context.CurrentItem.Paths.FullPath.ToLower(), "Item path isn't correct");
			Assert.AreEqual(db, m_context.CurrentDatabase, "Context database changed");
			Assert.AreEqual(lang, m_context.CurrentLanguage, "Context language changed");
			Assert.AreEqual("home 2", m_context.CurrentItem["title"], "Title is incorrect");
		}

		[Test]
		public void SetContext_SetContextByPathWithVersionAndLanguageAbsolute()
		{
			Assert.Ignore("Not Completed");
			Database db = m_context.CurrentDatabase;
			Language lang = m_context.CurrentLanguage;
			Item item = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			item = NewVersionSetTitle(item, "b title 2");
			Revolver.Core.Util.SetContext(m_context, Constants.Paths.Home + "/b:da:2");

			Assert.AreEqual(Constants.Paths.Home + "/b", m_context.CurrentItem.Paths.FullPath.ToLower(), "Item path isn't correct");
			Assert.AreEqual(db, m_context.CurrentDatabase, "Context database changed");
			Assert.AreNotEqual(lang, m_context.CurrentLanguage, "Context language didn't change");
			Assert.AreEqual("Danish", m_context.CurrentLanguage.GetDisplayName(), "Context language is incorrect");
			Assert.AreEqual("b title 2", m_context.CurrentItem["title"], "Title is incorrect");
		}

		[Test]
		public void SetContext_SetContextByPathWithVersionAndLanguageAndDatabase()
		{
			Assert.Ignore("Not Complete");
			Database db = m_context.CurrentDatabase;
			Language lang = m_context.CurrentLanguage;
			Item item = m_context.CurrentItem;
			Revolver.Core.Util.SetContext(m_context, "/web/" + Constants.Paths.Home + "/b:da:2");
		}

		[Test]
		public void SetContext_SetContextByPathWithVersionAndLanguageRelative()
		{
			Assert.Ignore("Not Complete");
			Database db = m_context.CurrentDatabase;
			Language lang = m_context.CurrentLanguage;
			Item item = m_context.CurrentItem;
			Revolver.Core.Util.SetContext(m_context, "b:da:2");
		}

		[Test]
		public void SetContext_SecondChildOfSameName()
		{
			m_context.CurrentItem = m_testRoot;

			using (new SecurityDisabler())
			{
				Item a1 = m_testRoot.Add("aaaa", m_template);
				Item a2 = m_testRoot.Add("aaaa", m_template);
				Item b = m_testRoot.Add("bbbb", m_template);

				CommandResult res = Revolver.Core.Util.SetContext(m_context, "aaaa[1]");
				Assert.AreEqual(CommandStatus.Success, res.Status);
				Assert.AreEqual(a1.ID, m_context.CurrentItem.ID);
			}
		}

		[Test]
		public void SetContext_FirstChildOfSameName()
		{
			m_context.CurrentItem = m_testRoot;

			using (new SecurityDisabler())
			{
				Item a1 = m_testRoot.Add("aaaa", m_template);
				Item a2 = m_testRoot.Add("aaaa", m_template);
				Item b = m_testRoot.Add("bbbb", m_template);

				CommandResult res = Revolver.Core.Util.SetContext(m_context, "aaaa[0]");
				Assert.AreEqual(CommandStatus.Success, res.Status);
				Assert.AreEqual(a2.ID, m_context.CurrentItem.ID);
			}
		}

		[Test]
		public void SetContext_SecondChildOfSameName_NoDuplicateName()
		{
			m_context.CurrentItem = m_testRoot;

			using (new SecurityDisabler())
			{
				Item a = m_testRoot.Add("aaaa", m_template);
				Item b = m_testRoot.Add("bbbb", m_template);
				Item c = m_testRoot.Add("cccc", m_template);

				CommandResult res = Revolver.Core.Util.SetContext(m_context, "aaaa[1]");
				Assert.AreEqual(CommandStatus.Failure, res.Status);
				Assert.AreEqual(m_testRoot.ID, m_context.CurrentItem.ID);
			}
		}

		[Test]
		public void SetContext_SecondChild()
		{
			m_context.CurrentItem = m_testRoot;

			using (new SecurityDisabler())
			{
				Item a = m_testRoot.Add("aaaa", m_template);
				Item b = m_testRoot.Add("bbbb", m_template);
				Item c = m_testRoot.Add("cccc", m_template);

				CommandResult res = Revolver.Core.Util.SetContext(m_context, "[1]");
				Assert.AreEqual(CommandStatus.Success, res.Status);
				Assert.AreEqual(b.ID, m_context.CurrentItem.ID);
			}
		}

		[Test]
		public void SetContext_NumberedChildBeyondChildCount()
		{
			m_context.CurrentItem = m_testRoot;

			using (new SecurityDisabler())
			{
				Item a = m_testRoot.Add("aaaa", m_template);
				Item b = m_testRoot.Add("bbbb", m_template);
				Item c = m_testRoot.Add("cccc", m_template);

				CommandResult res = Revolver.Core.Util.SetContext(m_context, "[5]");
				Assert.AreEqual(CommandStatus.Failure, res.Status);
				Assert.AreEqual(m_testRoot.ID, m_context.CurrentItem.ID);
			}
		}

		[Test]
		public void SetContext_NumberedChildMalformed()
		{
			m_context.CurrentItem = m_testRoot;

			using (new SecurityDisabler())
			{
				Item a = m_testRoot.Add("aaaa", m_template);
				Item b = m_testRoot.Add("bbbb", m_template);
				Item c = m_testRoot.Add("cccc", m_template);

				CommandResult res = Revolver.Core.Util.SetContext(m_context, "[5");
				Assert.AreEqual(CommandStatus.Failure, res.Status);
				Assert.AreEqual(m_testRoot.ID, m_context.CurrentItem.ID);
			}
		}

		[Test]
		public void SetContext_NumberedChildNegative()
		{
			m_context.CurrentItem = m_testRoot;

			using (new SecurityDisabler())
			{
				Item a = m_testRoot.Add("aaaa", m_template);
				Item b = m_testRoot.Add("bbbb", m_template);
				Item c = m_testRoot.Add("cccc", m_template);

				CommandResult res = Revolver.Core.Util.SetContext(m_context, "[-1]");
				Assert.AreEqual(CommandStatus.Failure, res.Status);
				Assert.AreEqual(m_testRoot.ID, m_context.CurrentItem.ID);
			}
		}

		[Test]
		public void EvaluateSingleExpression_StringsEqual()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "a = a");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_StringsNotEqual()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "a != b");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_StringsLess()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "a < aa");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_StringsLessOrEqual()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "a <= b");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_StringsGreater()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "b > a");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_StringsGreaterOrEqual()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "b >= a");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_StringsEqualEF()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "a = b");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateSingleExpression_StringsNotEqualEF()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "a != a");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateSingleExpression_StringsLessEF()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "b < a");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateSingleExpression_NumEqual()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "6 = 6 as number");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_NumNotEqual()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "7 != 1 as number");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_NumLess()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "3 < 4 as number");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_NumLessOrEqual()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "2 <= 100 as number");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_NumGreater()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "5 > 2 as number");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_NumGreaterOrEqual()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "3 >= 2 as number");
			Assert.IsTrue(res);
		}

		[Test]
		[ExpectedException(typeof(ExpressionException))]
		public void EvaluateSingleExpression_InvalidInpVal1NotNum()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			Revolver.Core.Util.EvaluateSingleExpression(m_context, "t >= 2 as number");
		}

		[Test]
		[ExpectedException(typeof(ExpressionException))]
		public void EvaluateSingleExpression_InvalidInpVal2NotNum()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			Revolver.Core.Util.EvaluateSingleExpression(m_context, "6 >= \"2007-03-01\" as number");
		}

		[Test]
		public void EvaluateSingleExpression_DateEqual()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "2007-01-12 = 12/01/2007 as date");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_DateNotEqual()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "2007-01-12 != 12/02/2007 as date");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_DateLess()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "2007-01-12 < 13/01/2007 as date");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_DateLessOrEqual()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "2007-01-12 <= 12/01/2008 as date");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_DateGreater()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "2007-01-12 > 12/01/2006 as date");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_DateGreaterOrEqual()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "2007-01-12 >= 12-04-2004 as date");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_DateEqualEF()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "2007-01-12 = 12/11/2007 as date");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateSingleExpression_DateGreaterOrEqualEF()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "2007-01-12 >= 12/11/2008 as date");
			Assert.IsFalse(res);
		}

		[Test]
		[ExpectedException(typeof(ExpressionException))]
		public void EvaluateSingleExpression_InvalidDate()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			Revolver.Core.Util.EvaluateSingleExpression(m_context, "2007-01-12 = 2009-15-20 as date");
		}

		[Test]
		[ExpectedException(typeof(ExpressionException))]
		public void EvaluateSingleExpression_NotNumber()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			Revolver.Core.Util.EvaluateSingleExpression(m_context, "2007-01-12 = 7 as number");
		}

		[Test]
		public void EvaluateSingleExpression_UsingFieldString()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "@title = a");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateSingleExpression_UsingFieldWithSpacesString()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "(@__created by) = b");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_UsingValueWithSpacesString()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "(@__created by) != (not admin)");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_UsingFieldDate()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "2010-01-12 > @__created as date");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_UsingAttributeString()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "@@name = a");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_UsingAttributeAndFieldString()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "@@name = @title");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateSingleExpression_IgnoreCase()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "cAsE1 = CASE1 with ignorecase");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_IgnoreCaseEF()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "cAsE1 = CASE2 with ignorecase");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateSingleExpression_IgnoreDecimal()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "17.3 = 17.947 as number with ignoredecimal");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_IgnoreDecimalEF()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "17.3 = 18 as number with ignoredecimal");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateSingleExpression_CeilingGreaterThan()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "18.8 > 18.4 as number with ceiling");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateSingleExpression_CeilingGreaterThanEF()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "18.8 > 19 as number with ceiling");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateSingleExpression_FloorLessThan()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "16.4 < 17.4 as number with floor");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_FloorLessThanEF()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "17 < 17.4 as number with floor");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateSingleExpression_Round()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "15.3 = 14.8 as number with round");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_StartsWithPresent()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "sitecore [ si");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_StartsWithMissing()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "sitecore [ boo");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateSingleExpression_EndsWithPresent()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "sitecore ] core");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_EndsWithMissing()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "sitecores ] core");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateSingleExpression_ContainsPresent()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "sitecore ? ec");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateSingleExpression_ContainsMissing()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/a");
			bool res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "sitecore ? ct");
			Assert.IsFalse(res);
		}

    [Test]
    public void EvaluateSingleExpression_Negate()
    {
      var res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "not true");
      Assert.IsFalse(res);
    }

    [Test]
    public void EvaluateSingleExpression_Negate2()
    {
      var res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "not false");
      Assert.IsTrue(res);
    }

    [Test]
    public void EvaluateSingleExpression_IsEmpty_NotEmpty()
    {
      var res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "isempty something");
      Assert.IsFalse(res);
    }

    [Test]
    public void EvaluateSingleExpression_IsEmpty_Empty()
    {
      var res = Revolver.Core.Util.EvaluateSingleExpression(m_context, "isempty ()");
      Assert.IsTrue(res);
    }

		[Test]
		public void EvaluateExpression()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = a");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateExpression_And()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = a and b = b");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateExpression_And2()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = a and c = c");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateExpression_AndEF()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = c and b = b");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateExpression_AndEF2()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = a and b = c");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateExpression_MultiAnd()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = a and b = b and c = c");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateExpression_MultiAndEF()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = a and b = b and d = c");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateExpression_MultiAndEF2()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = c and b = b and c = c");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateExpression_Or1()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = a or b = b");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateExpression_Or2()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = c or b = b");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateExpression_Or3()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = a or b = c");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateExpression_OrEF()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = b or b = a");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateExpression_MultipleOr()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = b or b = a or 2 = 2 as number");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateExpression_ExtraSpaces()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "  a =  b or  b = a");
			Assert.IsFalse(res);
		}

		[Test]
		public void EvaluateExpression_MixedAndOr()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = b and b = b or 3 = 3 as number");
			Assert.IsTrue(res);
		}

		[Test]
		public void EvaluateExpression_MixedAndOr2()
		{
			m_context.CurrentItem = m_context.CurrentDatabase.GetItem(Constants.Paths.Home + "/b");
			bool res = Revolver.Core.Util.EvaluateExpression(m_context, "a = b or b = b and 3 = 3 as number");
			Assert.IsTrue(res);
		}

		[Test]
		public void ParseGroupSimple()
		{
			string[] groups = Revolver.Core.Util.ParseFirstLevelGroups("this is a (simple) group", '(', ')');
			Assert.AreEqual(5, groups.Length, "Wrong number of groups parsed out");
			Assert.AreEqual("this", groups[0]);
			Assert.AreEqual("is", groups[1]);
			Assert.AreEqual("a", groups[2]);
			Assert.AreEqual("simple", groups[3]);
			Assert.AreEqual("group", groups[4]);
		}

		[Test]
		public void ParseGroupNested()
		{
			string[] groups = Revolver.Core.Util.ParseFirstLevelGroups("this is (a (nested) group)", '(', ')');
			Assert.AreEqual(3, groups.Length, "Wrong number of groups parsed out");
			Assert.AreEqual("this", groups[0]);
			Assert.AreEqual("is", groups[1]);
			Assert.AreEqual("a (nested) group", groups[2]);

		}

		[Test]
		public void ParseGroupMultiple()
		{
			string[] groups = Revolver.Core.Util.ParseFirstLevelGroups("this is a (simple) group with (multiple brackets)", '(', ')');
			Assert.AreEqual(7, groups.Length, "Wrong number of groups parsed out");
			Assert.AreEqual("this", groups[0]);
			Assert.AreEqual("is", groups[1]);
			Assert.AreEqual("a", groups[2]);
			Assert.AreEqual("simple", groups[3]);
			Assert.AreEqual("group", groups[4]);
			Assert.AreEqual("with", groups[5]);
			Assert.AreEqual("multiple brackets", groups[6]);
		}

		[Test]
		public void ParseGroupMultipleNested()
		{
			string[] groups = Revolver.Core.Util.ParseFirstLevelGroups("asd ( qwe ( dsa ) ) oi ( sad )", '(', ')');
			Assert.AreEqual(4, groups.Length, "Wrong number of groups parsed out");
			Assert.AreEqual("asd", groups[0]);
			Assert.AreEqual("qwe ( dsa )", groups[1]);
			Assert.AreEqual("oi", groups[2]);
			Assert.AreEqual("sad", groups[3]);
		}

		[Test]
		public void ParseGroupMissingEnding()
		{
			string[] groups = Revolver.Core.Util.ParseFirstLevelGroups("asd ( qwe", '(', ')');
			Assert.AreEqual(2, groups.Length, "Wrong number of groups parsed out");
			Assert.AreEqual("asd", groups[0]);
			Assert.AreEqual("qwe", groups[1]);
		}

		[Test]
		public void ParseGroupMissingEndingNested()
		{
			string[] groups = Revolver.Core.Util.ParseFirstLevelGroups("asd ( qwe ) sdf (ert (oiu)", '(', ')');
			Assert.AreEqual(5, groups.Length, "Wrong number of groups parsed out");
			Assert.AreEqual("asd", groups[0]);
			Assert.AreEqual("qwe", groups[1]);
			Assert.AreEqual("sdf", groups[2]);
			Assert.AreEqual("ert", groups[3]);
			Assert.AreEqual("(oiu)", groups[4]);
		}

		[Test]
		public void ParseGroupNoGroups()
		{
			string[] groups = Revolver.Core.Util.ParseFirstLevelGroups("there are no groups", '(', ')');
			Assert.AreEqual(4, groups.Length, "Wrong number of groups parsed out");
			Assert.AreEqual("there", groups[0]);
			Assert.AreEqual("are", groups[1]);
			Assert.AreEqual("no", groups[2]);
			Assert.AreEqual("groups", groups[3]);
		}

		[Test]
		public void ParseGroupOtherBrackets()
		{
			string[] groups = Revolver.Core.Util.ParseFirstLevelGroups("asd < qwe < dsa >> oi <sad >", '<', '>');
			Assert.AreEqual(4, groups.Length, "Wrong number of groups parsed out");
			Assert.AreEqual("asd", groups[0]);
			Assert.AreEqual("qwe < dsa >", groups[1]);
			Assert.AreEqual("oi", groups[2]);
			Assert.AreEqual("sad", groups[3]);
		}

		[Test]
		public void ExtractParameters_SingleNamed()
		{
			StringDictionary named = new StringDictionary();
			string[] numbered = null;
			string[] input = new string[] {"-name", "value" };

			Revolver.Core.Util.ExtractParameters(out named, out numbered, input, null);

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

			Revolver.Core.Util.ExtractParameters(out named, out numbered, input, null);

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

			Revolver.Core.Util.ExtractParameters(out named, out numbered, input, null);

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

			Revolver.Core.Util.ExtractParameters(out named, out numbered, input);

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

			Revolver.Core.Util.ExtractParameters(out named, out numbered, input, null);

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

			Revolver.Core.Util.ExtractParameters(out named, out numbered, input);

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

			Revolver.Core.Util.ExtractParameters(out named, out numbered, input, null);

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

			Revolver.Core.Util.ExtractParameters(out named, out numbered, input, null);

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

			Revolver.Core.Util.ExtractParameters(out named, out numbered, input, flags);

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

			Revolver.Core.Util.ExtractParameters(out named, out numbered, input, flags);

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

			Revolver.Core.Util.ExtractParameters(out named, out numbered, input, flags);

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

			Revolver.Core.Util.ExtractParameters(out named, out numbered, input, null);

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
			args = Revolver.Core.Util.RemoveParameter(args, "-rem1", 0);
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
			args = Revolver.Core.Util.RemoveParameter(args, "-rem1", 1);
			Assert.AreEqual(3, args.Length);
			Assert.AreEqual("num1", args[0]);
			Assert.AreEqual("-named1", args[1]);
			Assert.AreEqual("val1", args[2]);
		}

		[Test]
		public void RemoveParameter_Multiple()
		{
			string[] args = new string[] { "num1", "-named1", "val1", "-rem1", "remval1", "remval2", "remval3", "-f1" };
			args = Revolver.Core.Util.RemoveParameter(args, "-rem1", 3);
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
			args = Revolver.Core.Util.RemoveParameter(args, "-rem1", 1);
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
			args = Revolver.Core.Util.RemoveParameter(args, "-rem1", 1);
			Assert.AreEqual(4, args.Length);
			Assert.AreEqual("num2", args[0]);
			Assert.AreEqual("num1", args[1]);
			Assert.AreEqual("-named1", args[2]);
			Assert.AreEqual("val1", args[3]);
		}
	}
}
