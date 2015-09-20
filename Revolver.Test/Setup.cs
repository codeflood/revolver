using Sitecore.Data;
using Sitecore.Data.Items;
using System;

namespace Revolver.Test
{
	public class Setup
	{
		public static void SetupSitecoreItems()
		{
			using(new Sitecore.SecurityModel.SecurityDisabler())
			{
				// Get databases
				Database database = Sitecore.Configuration.Factory.GetDatabase("master");
				if (database == null)
					throw new Exception("Failed to find master database");

				Item itemHome = database.GetItem(Constants.Paths.Home);
				if (itemHome == null)
					throw new Exception("Failed to find the home node");

				// Grab templates
				TemplateItem simpleTemplate = database.Templates[Constants.Paths.DocTemplate];
				if (simpleTemplate == null)
					throw new Exception("Failed to find document template");

				TemplateItem folderTemplate = database.Templates[Constants.Paths.FolderTemplate];
				if (folderTemplate == null)
					throw new Exception("Failed to find folder template");

				// Grab presentation
				LayoutItem documentLayout = database.Resources.Layouts[Constants.Paths.Layout];
				if (documentLayout == null)
					throw new Exception("Failed to find the document layout");

				DeviceItem defaultDevice = database.Resources.Devices["Default"];
				if(defaultDevice == null)
					throw new Exception("Failed to find the default device");

				RenderingItem itemRendering = database.Resources.Renderings[Constants.Paths.Rendering];
				if(itemRendering == null)
					throw new Exception("Failed to find the item rendering");

				// Get user items
				if(!Sitecore.Security.Accounts.User.Exists("sitecore\\a"))
					Sitecore.Security.Accounts.User.Create("sitecore\\a", "a");

				if (!Sitecore.Security.Accounts.User.Exists("sitecore\\b"))
					Sitecore.Security.Accounts.User.Create("sitecore\\b", "b");

				itemHome.Delete();

				Item content = database.GetItem(Constants.Paths.Content);

				itemHome = content.Add(Constants.Names.Home, simpleTemplate);

				itemHome.Editing.BeginEdit();
				itemHome["title"] = "Sitecore";
				itemHome["text"] = "Welcome to sitecore";
				itemHome["__icon"] = "Network/16x16/home.png";
				itemHome["__Security"] = @"ar|sitecore\Author|pe|+item:write|+item:create|pd|+item:write|+item:create|";
				itemHome.Editing.EndEdit();

				// Create items
				Item itemA = itemHome.Add("a", simpleTemplate);

				itemA.Editing.BeginEdit();
				itemA["title"] = "Title A";
				itemA["text"] = "This is the <b>text</b> of node a";
				itemA.Editing.EndEdit();

				Item itemB = itemHome.Add("B", simpleTemplate);

				itemB.Editing.BeginEdit();
				itemB["title"] = "title b";
				itemB["text"] = "Some text for item b";

				// Set layout for default device
				Sitecore.Data.Fields.LayoutField layout = itemB.Fields[Sitecore.FieldIDs.LayoutField];
				Sitecore.Layouts.LayoutDefinition ld = Sitecore.Layouts.LayoutDefinition.Parse(layout.Data.OuterXml);
				Sitecore.Layouts.DeviceDefinition dd = ld.GetDevice(defaultDevice.ID.ToString());
				dd.Layout = documentLayout.ID.ToString();
				Sitecore.Layouts.RenderingDefinition rd = new Sitecore.Layouts.RenderingDefinition();
				rd.ItemID = itemRendering.ID.ToString();
				dd.AddRendering(rd);
				itemB[Sitecore.FieldIDs.LayoutField] = ld.ToXml();

				itemB.Editing.EndEdit();

				Item itemF = folderTemplate.CreateItemFrom("f", itemHome);

				Item itemAA = itemF.Add("AA", simpleTemplate);
				itemAA.Editing.BeginEdit();
				itemAA["title"] = "title aA";
				itemAA.Editing.EndEdit();

				Item itemBB = itemF.Add("bB", simpleTemplate);
				itemBB.Editing.BeginEdit();
				itemBB["title"] = "title Bb";
				itemBB["text"] = "<h1>This is node bB</h1>";
				itemBB.Editing.EndEdit();
			}
		}
	}
}
