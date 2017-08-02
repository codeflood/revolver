using System.Linq;
using Revolver.Core.Commands;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Revolver.Core
{
  // todo: write unit tests for this class

  /// <summary>
  /// Utility methods for inspecting command classes
  /// </summary>
  public static class CommandInspector
  {
    /// <summary>
    /// Find all command classes and populate them into the provided dictionary
    /// </summary>
    /// <param name="commands">A dictionary to populate the found commands into</param>
    public static void FindAllCommands(Dictionary<string, Type> commands)
    {
      foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
      {
        var commandInterface = type.GetInterface("Revolver.Core.Commands.ICommand", true);
        if (commandInterface != null)
        {
          var commandAttr = GetCommandAttribute(type);
          if (commandAttr != null)
          {
            commands.Add(commandAttr.Binding, type);
          }
        }
      }
    }

    /// <summary>
    /// If the command implements the IManualParseCommand interface, retrieve it.
    /// </summary>
    /// <param name="command">The command to try and find the interface</param>
    /// <returns>The command cast as IManualParseCommand or null if it does not implement the interface</returns>
    public static IManualParseCommand GetManualParseCommand(ICommand command)
    {
      var manualParseInterface = command.GetType().GetInterface("Revolver.Core.Commands.IManualParseCommand", true);
      if (manualParseInterface != null)
        return command as IManualParseCommand;

      return null;
    }

    /// <summary>
    /// Get the CommandAttribute from a class
    /// </summary>
    /// <param name="type">The class type to get the attribute from</param>
    /// <returns>The attribute if found, otherwise null</returns>
    public static CommandAttribute GetCommandAttribute(Type type)
    {
      var commandAttr = type.GetCustomAttribute<CommandAttribute>();

      if (commandAttr != null)
        return commandAttr as CommandAttribute;

      return null;
    }

    /// <summary>
    /// Get the NamedParameterAttribute from a property if it exists.
    /// </summary>
    /// <param name="property">The property to get the attribute from</param>
    /// <returns>The attribute if found, otherwise null</returns>
    public static NamedParameterAttribute GetNamedParameter(PropertyInfo property)
    {
      return GetCustomAttribute<NamedParameterAttribute>(property);
    }

    /// <summary>
    /// Get the NumberedParameterAttribute from a property if it exists.
    /// </summary>
    /// <param name="property">The property to get the attribute from</param>
    /// <returns>The attribute if found, otherwise null</returns>
    public static NumberedParameterAttribute GetNumberedParameter(PropertyInfo property)
    {
      return GetCustomAttribute<NumberedParameterAttribute>(property);
    }

  /// <summary>
  /// Get the ListParameterAttribute from a property if it exists.
  /// </summary>
  /// <param name="property">The property to get the attribute from</param>
  /// <returns>The attribute if found, otherwise null</returns>
  public static ListParameterAttribute GetListParameter(PropertyInfo property)
  {
    return GetCustomAttribute<ListParameterAttribute>(property);
  }

    /// <summary>
    /// Get the FlagParameterAttribute from a property if it exists.
    /// </summary>
    /// <param name="property">The property to get the attribute from</param>
    /// <returns>The attribute if found, otherwise null</returns>
    public static FlagParameterAttribute GetFlagParameter(PropertyInfo property)
    {
      return GetCustomAttribute<FlagParameterAttribute>(property);
    }

    /// <summary>
    /// Get the OptionalAttribute from a property if it exists.
    /// </summary>
    /// <param name="property">The property to get the attribute from</param>
    /// <returns>The attribute if found, otherwise null</returns>
    public static OptionalAttribute GetOptionalParameter(PropertyInfo property)
    {
      return GetCustomAttribute<OptionalAttribute>(property);
    }

    /// <summary>
    /// Get the GetDescriptionAttribute from a property if it exists.
    /// </summary>
    /// <param name="property">The property to get the attribute from</param>
    /// <returns>The attribute if found, otherwise null</returns>
    public static DescriptionAttribute GetDescriptionAttribute(PropertyInfo property)
    {
      return GetCustomAttribute<DescriptionAttribute>(property);
    }

    /// <summary>
    /// Get the NoSubstitutionAttribute from a property if it exists.
    /// </summary>
    /// <param name="property">The property to get the attribute from</param>
    /// <returns>The attribute if found, otherwise null</returns>
    public static NoSubstitutionAttribute GetNoSubstitutionAttribute(PropertyInfo property)
    {
      return GetCustomAttribute<NoSubstitutionAttribute>(property);
    }

    /// <summary>
    /// Find a custom attribute on a property
    /// </summary>
    /// <typeparam name="T">The type of the attribute</typeparam>
    /// <param name="property">The property to inspect</param>
    /// <returns>The custom attribute if found, otherwise null</returns>
    public static T GetCustomAttribute<T>(PropertyInfo property) where T : class
    {
      var attribute = property.GetCustomAttribute(typeof(T));
      
      if (attribute != null)
        return attribute as T;

      return default(T);
    }
  }
}