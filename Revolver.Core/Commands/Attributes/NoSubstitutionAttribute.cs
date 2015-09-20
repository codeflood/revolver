using System;

namespace Revolver.Core.Commands
{
  /// <summary>
  /// Indicates variable substitution should not be applied to this property.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class NoSubstitutionAttribute : Attribute
  {
  }
}