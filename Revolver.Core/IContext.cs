using Sitecore.Data;
using Sitecore.Data.Items;

namespace Revolver.Core
{
	public interface IContext
	{
		Item CurrentItem
		{
			get;
			set;
		}

		Database CurrentDatabase
		{
			get;
			set;
		}
	}
}
