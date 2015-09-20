using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Revolver.Core
{
  public class ItemInspector
  {
    /// <summary>
    /// Gets or sets the Item to inspect.
    /// </summary>
    public Item Item { get; protected set; }

    /// <summary>
    /// Create a new <see cref="ItemInspector"/>
    /// </summary>
    /// <param name="item">The item to inspect.</param>
    public ItemInspector([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      this.Item = item;
    }

    /// <summary>
    /// Get an attribute from an item
    /// </summary>
    /// <param name="name">The name of the attribute. Must be one of name, id, key, template, templateid, master, masterid</param>
    /// <returns>The attribute from the item</returns>
    public string GetItemAttribute(string name)
    {
      name = name.Replace("@", "").ToLower();
      switch (name)
      {
        case "name":
          return this.Item.Name;

        case "id":
          return this.Item.ID.ToString();

        case "key":
          return this.Item.Key;

        case "template":
          if (this.Item.Template != null)
            return this.Item.Template.InnerItem.Paths.FullPath;
          else
            return null;

        case "templateid":
          return this.Item.TemplateID.ToString();

        case "branch":
          if (this.Item.Branch != null)
            return this.Item.Branch.InnerItem.Paths.FullPath;
          else
            return null;

        case "branchid":
          return this.Item.BranchId.ToString();

        case "language":
          return this.Item.Language.GetDisplayName() + " [" + this.Item.Language.Name + "]";

        case "version":
          return this.Item.Version.Number.ToString();
      }

      return null;
    }

    /// <summary>
    /// Count the number of items below the given item including the item itself
    /// </summary>
    /// <returns>The number of items found</returns>
    public int CountDescendants()
    {
      int count = 1;
      if (this.Item.HasChildren)
      {
        for (int i = 0; i < this.Item.Children.Count; i++)
        {
          var inspector = new ItemInspector(this.Item.Children[i]);
          count += inspector.CountDescendants();
        }
      }

      return count;
    }

    /// <summary>
    /// Count the number of child items below the given item
    /// </summary>
    /// <returns>The number of items found</returns>
    public int CountChildren()
    {
      return this.Item.Children.Count;
    }
  }
}