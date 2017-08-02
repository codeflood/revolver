using Revolver.Core;
using Sitecore;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using Revolver.Core.Formatting;
using Sitecore.Configuration;

namespace Revolver.UI
{
  public class RevolverForm : BaseForm
  {
    private const string SESSION_CONTEXT_KEY = "revolver_context_";
    private const string CONTEXT_PERSIST_PATH = "revolver";

    private readonly ICommandFormatter _formatter = null;
    private CommandHandler _commandHandler = null;

    #region Controls
    protected Border output;
    protected Border prompt;
    protected Edit hdnSessionId;
    #endregion Controls

    public RevolverForm()
    {
      _formatter = new TextOutputFormatter();
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      var persistContext = Settings.GetBoolSetting("Revolver.PersistContextOnDisk", true);

      NameValueCollection parameters = null;
      string sessionId = string.Empty;

      if (Sitecore.Context.ClientPage.IsEvent)
      {
        var rawParameters = Sitecore.Context.ClientPage.ClientRequest.Parameters;
        if (rawParameters.StartsWith("r:"))
          rawParameters = rawParameters.Remove(0, 2);

        parameters = StringUtil.ParseNameValueCollection(rawParameters, '&', '=');
        sessionId = parameters["sessionId"];
      }

      if (string.IsNullOrEmpty(sessionId))
      {
        sessionId = Guid.NewGuid().ToString();
        hdnSessionId.Value = sessionId;
      }

      _commandHandler = GetCommandHandler(sessionId);

      CommandResult result = null;

      if (_commandHandler != null)
      {
        if (persistContext)
        {
          var persistedContext = GetRevolverContext(sessionId);
          if (persistedContext != null)
            _commandHandler.Context = persistedContext;
        }

        if (Sitecore.Context.ClientPage.IsEvent)
        {
          var commandText = Sitecore.Context.ClientPage.Server.UrlDecode(parameters["command"]);
          result = Execute(commandText);
        }
        else
        {
          ExecuteStartup();

          // Display command prompt to user
          prompt.InnerHtml = PrintCommandPrompt();

          if (persistContext)
            PersistRevolverContext(sessionId, _commandHandler.Context);

          return;
        }
      }

      var jsonOutput = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
          prompt = PrintCommandPrompt(),
          outputBuffer = _commandHandler != null ? _commandHandler.Context.EnvironmentVariables["outputbuffer"] : null,
          output = result.Message,
          exit = result.Status == CommandStatus.Abort ? "true" : null,
          error = result.Status != CommandStatus.Success ? result.Message : null
        });
  
      SheerResponse.SetReturnValue(jsonOutput);

      if (persistContext)
        PersistRevolverContext(sessionId, _commandHandler.Context);
    }

    protected CommandResult Execute(string command)
    {
      CommandResult result = null;
      try
      {
        // Dispatch the command to the core command handler
        result = _commandHandler.Execute(command.Trim());
      }
      catch (Exception ex)
      {
        result = new CommandResult(CommandStatus.Failure, ex.Message);
      }

      return result;
    }

    private void ExecuteStartup()
    {
      // Write banner
      var buffer = new StringBuilder();
      _formatter.PrintLine(ProductInfo.GetProductName() + " " + ProductInfo.GetProductVersion(), buffer);
      _formatter.PrintLine("Type 'help' for help.", buffer);
      _formatter.PrintLine(null, buffer);

      output.InnerHtml += buffer.ToString();

      var initOutput = _commandHandler.Init();

      foreach (var res in initOutput)
      {
        output.InnerHtml += res.Message;
      }

      output.InnerHtml = "<pre>" + output.InnerHtml + "</pre>";
      output.Focus();
    }

    // Prints the current command prompt
    private string PrintCommandPrompt()
    {
      var output = string.Empty;
      if (_commandHandler != null)
      {
        output = Parser.PerformSubstitution(_commandHandler.Context, "$prompt$");
        output = Prompt.EvaluatePrompt(_commandHandler.Context, output);
      }

      return output;
    }

    /// <summary>
    /// Gets a session context with the given id
    /// </summary>
    /// <param name="id">The id of the context to get</param>
    /// <returns>The context for the client</returns>
    protected virtual CommandHandler GetCommandHandler(string id)
    {
      string key = string.Format("{0}{1}", SESSION_CONTEXT_KEY, id);

      if (Sitecore.Context.ClientPage.Session[key] == null)
      {
        Sitecore.Context.ClientPage.Session[key] = new CommandHandler(_formatter);
      }

      return (CommandHandler)Sitecore.Context.ClientPage.Session[key];
    }

    /// <summary>
    /// Perists the Revolver context to disk
    /// </summary>
    /// <param name="id">The ID of the session</param>
    /// <param name="context">The context to persist</param>
    protected virtual void PersistRevolverContext(string id, Revolver.Core.Context context)
    {
      var path = GetContextPersistanceDirectory();
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);

      var fullPath = Path.Combine(path, id.ToString());
      fullPath = Path.ChangeExtension(fullPath, ".session");

      using (var stream = File.OpenWrite(fullPath))
      {
        var formatter = new BinaryFormatter();
        formatter.Serialize(stream, context);
      }
    }

    /// <summary>
    /// Get a Revolver context from disk
    /// </summary>
    /// <param name="id">The ID of the session</param>
    /// <returns>The context if found, otherwise null</returns>
    protected virtual Revolver.Core.Context GetRevolverContext(string id)
    {
      var path = GetContextPersistanceDirectory();
      var fullPath = Path.Combine(path, id.ToString());
      fullPath = Path.ChangeExtension(fullPath, ".session");
      
      if (!File.Exists(fullPath))
        return null;

      using (var stream = File.OpenRead(fullPath))
      {
        var formatter = new BinaryFormatter();
        var ob = formatter.Deserialize(stream);
        return ob as Revolver.Core.Context;
      }
    }

    /// <summary>
    /// Gets the persistance directory path
    /// </summary>
    /// <returns>The path to the persisted contexts</returns>
    protected virtual string GetContextPersistanceDirectory()
    {
      var tempPath = Sitecore.Configuration.Settings.TempFolderPath;
      if (HttpContext.Current != null)
        tempPath = HttpContext.Current.Server.MapPath(tempPath);

      return Path.Combine(tempPath, CONTEXT_PERSIST_PATH);
    }
  }
}
