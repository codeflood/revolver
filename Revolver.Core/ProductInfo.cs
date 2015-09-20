using System.Reflection;

namespace Revolver.Core
{
  /// <summary>
  /// Utility class to provide information about Revolver
  /// </summary>
  public static class ProductInfo
  {
    /// <summary>
    /// Gets the product name of this project
    /// </summary>
    /// <returns>The product name</returns>
    public static string GetProductName()
    {
      return "Revolver";
    }

    /// <summary>
    /// Gets the version of this project
    /// </summary>
    /// <returns>The product version</returns>
    public static string GetProductVersion()
    {
      var assembly = Assembly.GetExecutingAssembly();
      return assembly.GetName().Version.ToString();
    }
  }
}