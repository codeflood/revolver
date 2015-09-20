using Sitecore;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System.IO;
using System.Linq;
using System.Web;

namespace Revolver.Test
{
  public static class TestUtil
  {
    public static Item CreateContentFromFile(string filename, Item parent, bool changeIds = true)
    {
      var xml = File.ReadAllText(HttpContext.Current.Server.MapPath(filename));
      if (string.IsNullOrEmpty(xml))
        return null;

      return parent.PasteItem(xml, changeIds, PasteMode.Merge);
    }

    public static bool IsGermanRegistered(Revolver.Core.Context context)
    {
      return (from l in context.CurrentDatabase.Languages
               where l.Name == "de"
               select l).Any();
    }

    public static Item RegisterGermanLanaguage(Revolver.Core.Context context)
    {
      using (new SecurityDisabler())
      {
          var languageRoot = context.CurrentDatabase.GetItem(ItemIDs.LanguageRoot);
          return TestUtil.CreateContentFromFile("TestResources\\German Language.xml", languageRoot);
      }
    }
  }
}