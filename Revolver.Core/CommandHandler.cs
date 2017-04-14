using Revolver.Core.Commands;
using Revolver.Core.Exceptions;
using Revolver.Core.Formatting;
using Revolver.Core.ScriptLocator;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Revolver.Core
{
  [Serializable]
  public class CommandHandler
  {
    private readonly Dictionary<string, Type> _commands;
    private readonly Dictionary<string, Type> _custcommands;
    private readonly Dictionary<string, CommandArgs> _commandAliases;
    private readonly ICommandFormatter _formatter;
    
    private Context _context;
    private bool _executionFaulted = false;

    /// <summary>
    /// Gets or sets the context for this command handler
    /// </summary>
    public Context Context
    {
      get { return _context; }
      set
      {
        _context = value;
        _context.CommandHandler = this; // todo: This is bad design. Need to fix this.
      }
    }

#if NET45
    /// <summary>
    /// Gets a dictionary of the current core commands
    /// </summary>
    public ReadOnlyDictionary<string, Type> CoreCommands
    {
      get { return new ReadOnlyDictionary<string,Type>(_commands); }
    }

    /// <summary>
    /// Gets a dictionary of the current custom commands
    /// </summary>
    public ReadOnlyDictionary<string, Type> CustomCommands
    {
      get { return new ReadOnlyDictionary<string, Type>(_custcommands); }
    }

#else

    /// <summary>
    /// Gets a dictionary of the current core commands
    /// </summary>
    public Dictionary<string, Type> CoreCommands
    {
      get { return _commands;  }
    }

    /// <summary>
    /// Gets a dictionary of the current custom commands
    /// </summary>
    public Dictionary<string, Type> CustomCommands
    {
      get { return _custcommands; }
    }
#endif

    /// <summary>
    /// Gets the script locator
    /// </summary>
    public IScriptLocator ScriptLocator { get; private set; }

    public CommandHandler(ICommandFormatter formatter)
      : this(new Context(), formatter)
    {
    }

    /// <summary>
    /// Create a new instance of this class
    /// </summary>
    /// <param name="context">The Revolver vontext to operate on</param>
    /// <param name="formatter">The formatter to use</param>
    public CommandHandler(Context context, ICommandFormatter formatter)
    {
      _commands = new Dictionary<string, Type>();
      _custcommands = new Dictionary<string, Type>();
      _commandAliases = new Dictionary<string, CommandArgs>();
      Context = context;
      _formatter = formatter;
      ScriptLocator = new ScriptLocator.ScriptLocator();
      
      CommandInspector.FindAllCommands(_commands);
    }

    /// <summary>
    /// Perform session initialization
    /// </summary>
    /// <returns>The results of session initialization</returns>
    public CommandResult[] Init()
    {
      var buffer = new List<CommandResult>();
      var directive = ExecutionDirective.GetDefault();
      directive.IgnoreUnknownCommands = true;

      var initResult = Execute("init", directive);
      buffer.Add(initResult);

      foreach (var role in Sitecore.Context.User.Roles)
      {
        var localRoleName = role.Name;
        if (localRoleName.StartsWith(role.Domain.Name + "\\"))
          localRoleName = localRoleName.Substring(role.Domain.Name.Length + 1);

        var roleResult = Execute("(init-" + localRoleName.ToLower() + ")", directive);
        buffer.Add(roleResult);
      }

      var userResult = Execute("(init-" + Sitecore.Context.User.LocalName.ToLower() + ")", directive);
      buffer.Add(userResult);

      return buffer.ToArray();
    }

    /// <summary>
    /// Execute a single command line
    /// </summary>
    /// <param name="commandLine">The command line to execute</param>
    /// <returns>String output from the execution</returns>
    public CommandResult Execute(string commandLine)
    {
      return Execute(commandLine, ExecutionDirective.GetDefault());
    }

    /// <summary>
    /// Execute a single command line
    /// </summary>
    /// <param name="commandLine">The command line to execute</param>
    /// <param name="directive">Execution directives to control how execution should perform</param>
    /// <param name="scriptArgs">Numbered script arguments to use when executing a line from script</param>
    /// <returns>String output from the execution</returns>
    public CommandResult Execute(string commandLine, ExecutionDirective directive, string[] scriptArgs = null)
    {
      var elements = Parser.ParseInputLine(commandLine);

      // Dispatch the command to the core command handler
      var result = Execute(elements, directive, scriptArgs);
      return new CommandResult(result.Status, result.Message);
    }

    /// <summary>
    /// Adds a custom command class to the command handler
    /// </summary>
    /// <param name="name">The name to bind the command to</param>
    /// <param name="commandClass">The class which implements the command</param>
    public bool AddCustomCommand(string name, Type commandClass)
    {
      if (_custcommands.ContainsKey(name))
        _custcommands.Remove(name);

      _custcommands.Add(name, commandClass);

      return true;
    }

    /// <summary>
    /// Removes a bound custom command from the command handler
    /// </summary>
    /// <param name="name">The name the command is bound to</param>
    public bool RemoveCustomCommand(string name)
    {
      if (_custcommands.ContainsKey(name))
      {
        _custcommands.Remove(name);
        return true;
      }

      return false;
    }

    /// <summary>
    /// Adds a command alias.
    /// </summary>
    /// <param name="name">The name of the alias.</param>
    /// <param name="command">The command to alias.</param>
    /// <param name="parameters">Additional parameters to apply with the alias.</param>
    /// <returns>The status of the addition.</returns>
    public CommandResult AddCommandAlias(string name, string command, params string[] parameters)
    {
      if (_commandAliases.ContainsKey(name))
        return new CommandResult(CommandStatus.Failure, string.Format("Alias '{0}' already exists", name));

      if (_commands.ContainsKey(name))
        return new CommandResult(CommandStatus.Failure, string.Format("Cannot add alias '{0}' with the same name as an existing command", name));

      _commandAliases.Add(name, new CommandArgs(command, parameters));

      return new CommandResult(CommandStatus.Success, string.Format("Alias '{0}' added", name));
    }

    /// <summary>
    /// Removes a command alias.
    /// </summary>
    /// <param name="name">The alias to remove.</param>
    /// <returns>The status of the removal.</returns>
    public CommandResult RemoveCommandAlias(string name)
    {
      if (!_commandAliases.ContainsKey(name))
        return new CommandResult(CommandStatus.Failure, string.Format("Alias '{0}' not found", name));

      if (_commandAliases.Remove(name))
        return new CommandResult(CommandStatus.Success, string.Format("Alias '{0}' removed", name));

      return new CommandResult(CommandStatus.Failure, string.Format("Failed to remove alias '{0}'", name));
    }

    /// <summary>
    /// Get all aliases for a given command.
    /// </summary>
    /// <param name="command">The command to search for aliases for.</param>
    /// <returns>An enumeration of aliases for the command.</returns>
    public IEnumerable<string> GetCommandAliasesFor(string command)
    {
      return from alias in _commandAliases
             where alias.Value.CommandName == command
             select alias.Key;
    }

    /// <summary>
    /// Find command aliases by name.
    /// </summary>
    /// <param name="alias">The alias to search for.</param>
    /// <returns>The alias if found, otherwise null.</returns>
    public CommandArgs FindCommandAlias(string alias)
    {
      if (_commandAliases.ContainsKey(alias))
        return _commandAliases[alias];

      return null;
    }

    /// <summary>
    /// Executes a command
    /// </summary>
    /// <param name="elements">The elements forming the command and arguments</param>
    /// <param name="directive">Execution directives to control how execution should perform</param>
    /// <param name="scriptArgs">Numbered script arguments to use when executing a line from script</param>
    private CommandResult Execute(string[] elements, ExecutionDirective directive, string[] scriptArgs = null)
    {
      EnsureValidContextItem();

      // Check if we have any subcommands
      var ind = Array.IndexOf(elements, Constants.SubcommandSymbol);
      while (ind >= 0)
      {
        var innerOutput = string.Empty;
        if (elements.Length >= (ind + 2))
        {
          var subelements = Parser.ParseInputLine(elements[ind + 1]);

          var innerResult = Execute(subelements, directive, scriptArgs);
          if (innerResult.Status != CommandStatus.Success)
            return new CommandResult(innerResult.Status, "Subcommand failure: " + innerResult.Message);

          innerOutput = innerResult.Message;

          elements = elements.Take(ind).Concat(new[] { innerOutput }).Concat(elements.Skip(ind + 2)).ToArray();
        }

        ind = Array.IndexOf(elements, Constants.SubcommandSymbol);
      }

      // Check if we have any chained commands
      ind = Array.IndexOf(elements, Constants.CommandChainSymbol);
      while (ind >= 0)
      {
        var subelements = elements.Take(ind);

        var innerResult = Execute(subelements.ToArray(), directive, scriptArgs);
        if (innerResult.Status != CommandStatus.Success)
          return new CommandResult(innerResult.Status, "Subcommand failure: " + innerResult.Message);

        _context.EnvironmentVariables[Constants.CommandChainedValue] = innerResult.Message;

        elements = elements.Skip(ind + 1).ToArray();

        ind = Array.IndexOf(elements, Constants.CommandChainSymbol);
      }

      if (elements.Length > 0)
      {
        for (var i = 0; i < elements.Length; i++)
        {
          if (elements[i].Contains(Constants.EscapedSubcommandSymbol))
            elements[i] = elements[i].Replace(Constants.EscapedSubcommandSymbol, Constants.SubcommandSymbol);

          if (elements[i].Contains(Constants.EscapedCommandChainSymbol))
            elements[i] = elements[i].Replace(Constants.EscapedCommandChainSymbol, Constants.CommandChainSymbol);
        }

        var args = elements.Skip(1).ToArray();

        var result = Execute(elements[0].Trim(), args, directive, scriptArgs);
        if (result != null)
          return result;
        
        if (directive.IgnoreUnknownCommands == null || !directive.IgnoreUnknownCommands.Value)
          return new CommandResult(CommandStatus.Failure, "Unknown command or script name " + elements[0].Trim());
      }

      return new CommandResult(CommandStatus.Success, string.Empty);
    }

    /// <summary>
    /// Execute a command or script
    /// </summary>
    /// <param name="command">The command or script name</param>
    /// <param name="args">Arguments to pass to the command</param>
    /// <param name="directive">Execution directives to control how execution should perform</param>
    /// <param name="scriptArgs">Numbered script arguments to use when executing a line from script</param>
    /// <returns>True if the command was run, otherwise returns false</returns>
    private CommandResult Execute(string command, string[] args, ExecutionDirective directive, string[] scriptArgs = null)
    {
      // update current date and time in environment variable
      Context.EnvironmentVariables["now"] = Sitecore.DateUtil.IsoNow;

      command = Parser.PerformSubstitution(_context, command);
      command = Parser.PerformScriptSubstitution(command, scriptArgs);

      if (command == string.Empty)
        return new CommandResult(CommandStatus.Success, string.Empty);

      ICommand cmd = null;

      if (_commands.ContainsKey(command))
      {
        cmd = (ICommand)Activator.CreateInstance(_commands[command]);
      }
      else if (_custcommands.ContainsKey(command))
      {
        cmd = (ICommand)Activator.CreateInstance(_custcommands[command]);
      }
      else if(_commandAliases.ContainsKey(command) && _commands.ContainsKey(_commandAliases[command].CommandName))
      {
        var alias = _commandAliases[command];
        cmd = (ICommand)Activator.CreateInstance(_commands[alias.CommandName]);
        
        if(alias.Parameters != null && alias.Parameters.Length > 0)
        {
          var paramsList = new List<string>(alias.Parameters);
          paramsList.AddRange(args);
          args = paramsList.ToArray();
        }
      }

      if (cmd != null)
        return ExecuteCommand(cmd, args, directive, scriptArgs);
      
      var substitutedArgs = from arg in args
                            select Parser.PerformScriptSubstitution(arg, scriptArgs);

      return ExecuteScript(command, substitutedArgs.ToArray(), directive);
    }

    /// <summary>
    /// Executes a command
    /// </summary>
    /// <param name="cmd">The command to execute</param>
    /// <param name="args">Arguments to apply to the command</param>
    /// <param name="directive">Directives to execute with</param>
    /// <param name="scriptArgs">Numbered script arguments to use when executing a line from script</param>
    /// <returns>The outcome of the execution</returns>
    private CommandResult ExecuteCommand(ICommand cmd, string[] args, ExecutionDirective directive, string[] scriptArgs)
    {
      cmd.Initialise(_context, _formatter);
      CommandResult result = null;

      var manualParseInterface = CommandInspector.GetManualParseCommand(cmd);
      if (manualParseInterface != null)
      {
        var subArgs = from arg in args
                      let wip = Parser.PerformScriptSubstitution(arg, scriptArgs) 
                      select Parser.PerformSubstitution(_context, wip);

        result = manualParseInterface.Run(subArgs.ToArray());
      }
      else
      {
        var properties = cmd.GetType().GetProperties();

        object propValue = null;
        var processingArgs = args;

        // Process multi-word named parameters first
        foreach (var prop in properties)
        {
          propValue = null;

          var noSub = CommandInspector.GetNoSubstitutionAttribute(prop);

          var namedParameterAttribute = CommandInspector.GetNamedParameter(prop);
          if (namedParameterAttribute != null && namedParameterAttribute.WordCount > 1)
          {
            var words = new List<string>();

            for (var i = 0; i < namedParameterAttribute.WordCount; i++)
            {
              var value = string.Empty;
              ParameterUtil.GetParameter(args, "-" + namedParameterAttribute.Name, i, ref value);

                if (!string.IsNullOrEmpty(value))
                {
                  // Substitute individual values as ConvertAndAssignParameter won't handle word arguments
                  if (noSub == null)
                  {
                      value = Parser.PerformSubstitution(_context, (string) value);
                  }

                  value = Parser.PerformScriptSubstitution((string) value, scriptArgs);
                  words.Add(value);
                }
            }

            if (words.Count > 0)
            {
              propValue = words.ToArray();
              processingArgs = ParameterUtil.RemoveParameter(processingArgs, "-" + namedParameterAttribute.Name, namedParameterAttribute.WordCount);
            }
          }

          ConvertAndAssignParameter(prop, cmd, propValue, scriptArgs);
        }

        // Find flags
        var flags = from prop in properties
                    let attr = CommandInspector.GetFlagParameter(prop)
                    where attr != null
                    select attr.Name;

        // Parse remaining arguments
        StringDictionary named = null;
        string[] numbered = null;
        ParameterUtil.ExtractParameters(out named, out numbered, processingArgs, flags.ToArray());

        // Map the parameters to properties
        foreach (var prop in properties)
        {
          propValue = null;

          var namedParameterAttribute = CommandInspector.GetNamedParameter(prop);
          if (namedParameterAttribute != null)
          {
            if (named.ContainsKey(namedParameterAttribute.Name))
              propValue = named[namedParameterAttribute.Name];
          }

          var numberedParameterAttribute = CommandInspector.GetNumberedParameter(prop);
          if (numberedParameterAttribute != null)
          {
            if (numberedParameterAttribute.Number < numbered.Length)
              propValue = numbered[numberedParameterAttribute.Number];
          }

          var flagParameterAttribute = CommandInspector.GetFlagParameter(prop);
          if (flagParameterAttribute != null)
          {
            if (named.ContainsKey(flagParameterAttribute.Name))
            {
#if NET45
              var origVal = (bool)prop.GetValue(cmd);
#else
              var origVal = (bool)prop.GetValue(cmd, null);
#endif
              propValue = !origVal;
            }
          }

          ConvertAndAssignParameter(prop, cmd, propValue, scriptArgs);
        }

        AssignListParameter(cmd, properties, numbered, scriptArgs);

        result = cmd.Run();
      }

      if ((bool)(directive.StopOnError ?? false) && (result.Status == CommandStatus.Failure))
        _executionFaulted = true;

      return result;
    }

    /// <summary>
    /// Assign the property using necessary conversions
    /// </summary>
    /// <param name="property">The property to assign</param>
    /// <param name="hostObject">The object holding the property</param>
    /// <param name="value">The value to assign to the property</param>
    /// <param name="scriptArgs">Numbered script arguments to use when executing a line from script</param>
    private void ConvertAndAssignParameter(PropertyInfo property, object hostObject, object value, string[] scriptArgs = null)
    {
      if (value == null)
        return;

      if (value is string)
      {
        var noSub = CommandInspector.GetNoSubstitutionAttribute(property);
        if (noSub == null)
        {
          value = Parser.PerformSubstitution(_context, (string) value);
        }

        value = Parser.PerformScriptSubstitution((string)value, scriptArgs);
      }

      if (property.PropertyType.IsAssignableFrom(value.GetType()))
      {
#if NET45
        property.SetValue(hostObject, value);
#else
        property.SetValue(hostObject, value, null);
#endif
      }
      else
      {
        // Find an appropriate type converter.
        TypeConverter converter = null;

        // First look for type converters on the property itself
#if NET45
        var propertyTypeConverter = property.GetCustomAttribute(typeof (TypeConverterAttribute));
#else
        Attribute propertyTypeConverter = null;

        var attributes = property.GetCustomAttributes(typeof(TypeConverterAttribute), true);
          
        if (attributes != null && attributes.Any())
        {
          propertyTypeConverter = attributes[0] as Attribute;
        }
          
#endif
        if (propertyTypeConverter != null)
        {
          var converterType = Type.GetType((propertyTypeConverter as TypeConverterAttribute).ConverterTypeName, true,
            false);
          converter = Activator.CreateInstance(converterType) as TypeConverter;
        }
        else
        {
          // Otherwise allow type descriptor to find a converter
          converter = TypeDescriptor.GetConverter(property.PropertyType);
        }

        if (converter != null)
        {
#if NET45
          property.SetValue(hostObject, converter.ConvertFrom(value));
#else
          property.SetValue(hostObject, converter.ConvertFrom(value), null);
#endif
        }
      }
    }

    /// <summary>
    /// If command has a ListParameter, assign unmatched  values from <c>numbered</c> to it.
    /// </summary>
    /// <param name="cmd">The command</param>
    /// <param name="properties">The command properties</param>
    /// <param name="numbered">Unmatched parameteres from ExtractParameters call</param>
    /// <param name="scriptArgs">Numbered script arguments to use when executing a line from script</param>
    private void AssignListParameter(ICommand cmd, PropertyInfo[] properties, string[] numbered, string[] scriptArgs = null)
    {
      var listProperty = properties.FirstOrDefault(p => CommandInspector.GetListParameter(p) != null);
      if (listProperty == null)
        return;

      var numberedParameters = properties.Select(CommandInspector.GetNumberedParameter)
                         .Where(p => p != null)
                         .Select(p => p.Number);

      int numberedParametersToSkip = (numberedParameters.Any()) ? numberedParameters.Max() + 1 : 0;
      var list = numbered.Skip(numberedParametersToSkip).ToList();
      list = (from element in list
              let wip = Parser.PerformScriptSubstitution(element, scriptArgs)
              select Parser.PerformSubstitution(_context, wip)).ToList();
#if NET45
      listProperty.SetValue(cmd, list);
#else
      listProperty.SetValue(cmd, list, null);
#endif
    }

    /// <summary>
    /// Execute a script
    /// </summary>
    /// <param name="name">The name of the script to process</param>
    /// <param name="args">The arguments to the script</param>
    /// <param name="directive">Execution directives to control how execution should perform</param>
    /// <returns>The output from the script if the script was found, otherwise null</returns>
    private CommandResult ExecuteScript(string name, string[] args, ExecutionDirective directive)
    {
      try
      {
        var script = ScriptLocator.GetScript(name);
        if (script != null)
        {
          return ExecuteScriptSource(script, directive, args);
        }
      }
      catch (MultipleScriptsFoundException ex)
      {
        var message = new StringBuilder();
        foreach (var scriptName in ex.Names)
        {
          _formatter.PrintLine(scriptName, message);
        }

        message.Append("Multiple scripts found by name '");
        message.Append(name);
        _formatter.PrintLine("'", message);
        message.Append("Script names must be unique");

        return new CommandResult(CommandStatus.Failure, message.ToString());
      }

      if (directive.StopOnError.HasValue && directive.StopOnError.Value)
        _executionFaulted = true;

      return null;
    }

    /// <summary>
    /// Performs execution of the source of the script
    /// </summary>
    /// <param name="scriptSource">The source of the script to execute</param>
    /// <param name="directive">The execution directives to execute with</param>
    /// <param name="args">The arguments to exeucte the script with</param>
    /// <returns>The outcome of the execution</returns>
    private CommandResult ExecuteScriptSource(string scriptSource, ExecutionDirective directive, string[] args = null)
    {
      var output = new StringBuilder();
      var status = CommandStatus.Success;
      var lines = Parser.ParseScriptLines(scriptSource, _formatter);

      var workingDirective = ExecutionDirective.GetDefault();

      if (!directive.IsEmpty())
        workingDirective.Patch(directive);

      _executionFaulted = false;

      // Execute each line
      foreach(var line in lines)
      {
        // Don't execute a comment line (starts with #) and don't execute if faulted
        if (!line.StartsWith(Constants.ScriptCommentLineIndicator) && !_executionFaulted)
        {
          // Look for directives
          if (line.StartsWith(Constants.ScriptDirectiveIndicator))
          {
            // Extract directive in string
            var directiveString = line.Substring(1);

            // Parse string to get directive
            var currentDirective = ExecutionDirective.Parse(directiveString);

            if (currentDirective.IsEmpty())
              _formatter.PrintLine("Unknown directive '" + directiveString + "'", output);

            workingDirective.Patch(currentDirective);
          }
          else
          {
            var res = Execute(line, workingDirective, args);
	        var outputAdded = false;

            if (res != null)
            {
              if (!(bool)(workingDirective.EchoOff ?? false))
              {
                _formatter.PrintLine(res.Message, output);
	            outputAdded = true;
              }

              if (res.Status == CommandStatus.Abort)
			  {
				if(!outputAdded)
				  _formatter.PrintLine(res.Message, output);

				return new CommandResult(CommandStatus.Success, output.ToString());
			  }

              if (res.Status != CommandStatus.Success)
                status = CommandStatus.Failure;
            }
            else
            {
              _executionFaulted = false;
              return null;
            }
          }
        }
      }

      _executionFaulted = false;
      return new CommandResult(status, output.ToString());
    }

    /// <summary>
    /// Ensure the current context is valid
    /// </summary>
    private void EnsureValidContextItem()
    {
      if (_context.CurrentItem == null)
      {
        Item item = null;
        var root = _context.CurrentDatabase.GetRootItem();
        var path = _context.LastGoodPath;

        item = _context.CurrentDatabase.GetItem(path);
        while (item == null && string.Compare(path, root.Paths.FullPath, true) != 0)
        {
          path = PathParser.EvaluatePath(_context, path + "/..");
          item = _context.CurrentDatabase.GetItem(path);
        }

        _context.CurrentItem = item;
      }
    }
  }
}
