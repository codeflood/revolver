using System;
using System.Linq;
using System.Text;
using Sitecore.Caching;
using Sitecore.Caching.Generics;
using Sitecore.StringExtensions;

namespace Revolver.Core.Commands
{
  [Command("cache")]
  public class Cache : BaseCommand
  {
    [NumberedParameter(0, "cachename")]
    [Description("The name of the cache to match on.")]
    [Optional]
    public string CacheName { get; set; }

    [NumberedParameter(1, "keyname")]
    [Description("A partial name of the key to match keys on.")]
    [Optional]
    public string KeyName { get; set; }

    [FlagParameter("kp")]
    [Description("Match the cache key partially")]
    [Optional]
    public bool KeyNamePartial { get; set; }

    [FlagParameter("c")]
    [Description("Clear the cache entries.")]
    [Optional]
    public bool Clear { get; set; }

    public Cache()
    {
      KeyNamePartial = false;
      Clear = false;
    }

    public override CommandResult Run()
    {
	  try
      {
        if (Clear)
        {
          if (string.IsNullOrEmpty(KeyName))
            return ClearCaches();

          return ClearEntries();
        }
      
        if (string.IsNullOrEmpty(CacheName))
          return ListCaches();

        return ListCacheKeys();
	  }
	  catch(InvalidOperationException)
	  {
		return new CommandResult(CommandStatus.Failure, "Cache type not supported.");
      }
    }

    protected virtual CommandResult ListCaches()
    {
      var buffer = new StringBuilder();

      var widths = new[] { 40, 15, 15, 15 };
      Formatter.PrintTable(new []
      {
        "Name",
        "Count",
        "Size",
        "Max Size"
      }, widths, buffer);

      Formatter.PrintTable(new[]
      {
        "----",
        "-----",
        "----",
        "--------"
      }, widths, buffer);

      var caches = CacheManager.GetAllCaches().OrderBy(x => x.Name);

      var cacheCount = 0;

      foreach (var cache in caches)
      {
        if (string.IsNullOrEmpty(CacheName) || cache.Name.Equals(CacheName))
        {
          Formatter.PrintTable(new[]
          {
            cache.Name,
            cache.Count.ToString(),
            cache.Size.ToString(),
            cache.MaxSize.ToString()
          }, widths, buffer);

          cacheCount++;
        }
      }

      Formatter.PrintLine(string.Empty, buffer);
      Formatter.PrintLine("Listing {0} caches".FormatWith(cacheCount), buffer);

      return new CommandResult(CommandStatus.Success, buffer.ToString());
    }

    protected virtual CommandResult ListCacheKeys()
    {
      var buffer = new StringBuilder();

	  var cache = GetCacheByName();
	  if (cache == null)
      {
        return new CommandResult(CommandStatus.Failure, "Cannot find cache by name '{0}'".FormatWith(CacheName));
      }

      var widths = new[]
	  {
		45,
#if !FEATURE_ABSTRACTS
		15,
#endif
		80
	  };

      Formatter.PrintTable(new[]
      {
        "Key",
#if !FEATURE_ABSTRACTS
        "Size",
#endif
        "Value"
      }, widths, buffer);

      Formatter.PrintTable(new[]
      {
        "---",
#if !FEATURE_ABSTRACTS
        "----",
#endif
        "-----"
      }, widths, buffer);

#if FEATURE_ABSTRACTS
	  var keyCount = EnumerateCacheEntries(cache, key =>
	  {
		Formatter.PrintTable(new[]
		{
		  key,
		  cache.GetValue(key).ToString()
		}, widths, buffer);
	  });
#else
	  var keyCount = EnumerateCacheEntries(cache, entry =>
      {
        Formatter.PrintTable(new[]
        {
          entry.Key.ToString(),
          entry.DataLength.ToString(),
          entry.Data.ToString()
        }, widths, buffer);
      });
#endif

	  Formatter.PrintLine(string.Empty, buffer);
      Formatter.PrintLine("Listing {0} keys".FormatWith(keyCount), buffer);

      return new CommandResult(CommandStatus.Success, buffer.ToString());
    }

    protected virtual CommandResult ClearCaches()
    {
      if (string.IsNullOrEmpty(CacheName))
      {
        CacheManager.ClearAllCaches();
        return new CommandResult(CommandStatus.Success, "All caches have been cleared");
      }
      else
      {
		var cache = GetCacheByName();
		if (cache == null)
        {
          return new CommandResult(CommandStatus.Failure, "Cannot find cache by name '{0}'".FormatWith(CacheName));
        }

        var entryCount = cache.Count;
        cache.Clear();
        return new CommandResult(CommandStatus.Success, "Cleared {0} entr{1} from cache '{2}'".FormatWith(entryCount, entryCount == 1 ? "y" : "ies", cache.Name));
      }
    }

    protected virtual CommandResult ClearEntries()
    {
	  var cache = GetCacheByName();

	  if (cache == null)
      {
        return new CommandResult(CommandStatus.Failure, "Cannot find cache by name '{0}'".FormatWith(CacheName));
      }

#if FEATURE_ABSTRACTS
	  var keyCount = EnumerateCacheEntries(cache, key =>
	  {
		cache.Remove(key);
	  });
#else
	  var keyCount = EnumerateCacheEntries(cache, entry =>
      {
        cache.Remove(entry.Key);
      });
#endif

	  return new CommandResult(CommandStatus.Success, "Cleared {0} entr{1} from cache '{2}'".FormatWith(keyCount, keyCount == 1 ? "y" : "ies", cache.Name));
    }

#if FEATURE_ABSTRACTS
	protected virtual ICache<string> GetCacheByName()
	{
	  var cacheInfo = CacheManager.GetAllCaches().FirstOrDefault(x => x.Name.Equals(CacheName));
	  if(cacheInfo == null)
	    return null;

	  if (!(cacheInfo is ICache<string>))
		throw new InvalidOperationException();

	  return (ICache<string>) cacheInfo;
	}

	protected virtual int EnumerateCacheEntries(ICache<string> cache, Action<string> action)
	{
	  var keys = cache.GetCacheKeys().OrderBy(x => x);
	  var keyCount = 0;

	  foreach (var key in keys)
	  {
		var match = true;
		if (!string.IsNullOrEmpty(KeyName))
		  match = KeyNamePartial ? key.ToString().Contains(KeyName) : key.ToString().Equals(KeyName);

		if (match)
		{
		  action(key);

		  keyCount++;
		}
	  }

	  return keyCount;
	}
#else
	protected virtual Cache GetCacheByName()
	{
	  return CacheManager.GetAllCaches().FirstOrDefault(x => x.Name.Equals(CacheName));
	}

	protected virtual int EnumerateCacheEntries(Sitecore.Caching.Cache cache, Action<Sitecore.Caching.Cache.CacheEntry> action)
    {
      var keys = cache.GetCacheKeys().OrderBy(x => x.Name);
      var keyCount = 0;

      foreach (var key in keys)
      {
        var match = true;
        if (!string.IsNullOrEmpty(KeyName))
          match = KeyNamePartial ? key.ToString().Contains(KeyName) : key.ToString().Equals(KeyName);

        if (match)
        {
          var entry = cache.GetEntry(key, false);
          action(entry);

          keyCount++;
        }
      }

      return keyCount;
    }
#endif

	public override string Description()
    {
      return "List information about the caches.";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample("");
      details.AddExample("core[items]");
      details.AddExample("core[items] somekey");
      details.AddExample("core[items] somekey -c");
      details.AddExample("core[items] -c");
    }
  }
}