using System.Collections.Specialized;

namespace Revolver.Core
{
  public class HelpDetails
  {
    /// <summary>The parameters for the command.</summary>
    private readonly NameValueCollection _parameters;

    /// <summary>The examples of command usage.</summary>
    private readonly StringCollection _examples;

    /// <summary>
    /// Gets or sets the description for the comman.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the usage for the command.
    /// </summary>
    public string Usage { get; set; }

    /// <summary>
    /// Gets or sets the comments.
    /// </summary>
    public string Comments { get; set; }

    /// <summary>
    /// Gets the parameters for the command.
    /// </summary>
    public NameValueCollection Parameters
    {
      get { return _parameters; }
    }

    /// <summary>
    /// Gets the examples for command usage.
    /// </summary>
    public StringCollection Examples
    {
      get { return _examples; }
    }

    /// <summary>
    /// Create a new instance of the class.
    /// </summary>
    public HelpDetails()
    {
      _parameters = new NameValueCollection(10);
      _examples = new StringCollection();
    }

    /// <summary>
    /// Adds a parameter definition
    /// </summary>
    /// <param name="name">The name of the parameter</param>
    /// <param name="description">The description of the parameter</param>
    public void AddParameter(string name, string description)
    {
      _parameters.Add(name, description);
    }

    /// <summary>
    /// Adds an example
    /// </summary>
    /// <param name="example">The example to add</param>
    public void AddExample(string example)
    {
      _examples.Add(example);
    }
  }
}
