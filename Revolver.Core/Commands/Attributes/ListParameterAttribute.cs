using System;

namespace Revolver.Core.Commands
{
	/// <summary>
	/// Attach to an IList&lt;string&gt; or any type to which 
	/// a List&ltstring&gt; may be implicitly cast.
	/// Property receives all unmatched parameters
	/// after flags, named and numbered parameters
	/// are accounted for.
	/// Assign to at most one parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ListParameterAttribute : Attribute
	{
		/// <summary>
    /// Gets or sets the value placeholder to use in help messages
    /// </summary>
    public string HelpValuePlaceholder
    {
      get;
      protected set;
    }

    /// <summary>
	/// Construct a new instance of ListParameterAttribute
    /// </summary>
    /// <param name="helpValuePlaceholder">The value placeholder to use in help messages</param>
    public ListParameterAttribute(string helpValuePlaceholder = "")
    {
      HelpValuePlaceholder = helpValuePlaceholder;
    }
	}
}