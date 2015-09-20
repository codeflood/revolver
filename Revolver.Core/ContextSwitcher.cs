using Sitecore.Data.Items;
using Sitecore.Globalization;
using System;

namespace Revolver.Core
{
  /// <summary>
  /// Utility class to temporarily switch context. To be used in the "using" pattern.
  /// </summary>
  public class ContextSwitcher : IDisposable
  {
    #region Member Variables
    private Context _context = null;
    private Item _prevItem = null;
    private Language _prevLanguage = null;
    private bool _active = false;
    private CommandResult _result = null;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the CommandResult for the switch operation
    /// </summary>
    public CommandResult Result
    {
      get { return _result; }
    }
    #endregion

    /// <summary>
    /// Create a new instance of the ContextSwitcher
    /// </summary>
    /// <param name="context">The context to switch. ContextSwitcher stores the initial context.</param>
    /// <param name="path">The path to switc to</param>
    public ContextSwitcher(Context context, string path)
    {
      if (!string.IsNullOrEmpty(path))
      {
        _active = true;
        StoreContext(context);
        _result = context.SetContext(path);
      }
      else
        _result = new CommandResult(CommandStatus.Success, "ContextSwitcher not active");
    }

    /// <summary>
    /// Stores the given context internally
    /// </summary>
    /// <param name="context">The context to store</param>
    private void StoreContext(Context context)
    {
      _context = context;
      _prevItem = context.CurrentItem;
      _prevLanguage = _context.CurrentLanguage;
    }

    /// <summary>
    /// Restore the previous context
    /// </summary>
    public void Dispose()
    {
      if (_active && _result.Status == CommandStatus.Success)
      {
        _context.CurrentItem = _prevItem;
        _context.CurrentLanguage = _prevLanguage;
      }
    }
  }
}
