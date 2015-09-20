using NUnit.Framework;
using Revolver.Core;
using Sitecore.Caching;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("Cache")]
  public class Cache : BaseCommandTest
  {
    private const string CACHE_NAME = "testcache";

    [Test]
    public void ListAll()
    {
      var cmd = new Cmd.Cache();
      InitCommand(cmd);

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      // Make sure several of the caches appear
      Assert.That(result.Message, Is.StringContaining("AccessResultCache"));
      Assert.That(result.Message, Is.StringContaining("web[standardValues]"));
    }

    [Test]
    public void ListSingleCache()
    {
      var testCache = new TestCache();
      testCache.InnerCache.Add("lorem", "ipsum");

      var cmd = new Cmd.Cache();
      InitCommand(cmd);
      cmd.CacheName = CACHE_NAME;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringContaining("lorem"));
    }

    [Test]
    public void ListSingleCache_InvalidCacheName()
    {
      var cmd = new Cmd.Cache();
      InitCommand(cmd);
      cmd.CacheName = "nonexisting cache";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void GetEntryExact()
    {
      var testCache = new TestCache();
      testCache.InnerCache.Add("lorem", "ipsum");
      testCache.InnerCache.Add("dolor", "sit");

      var cmd = new Cmd.Cache();
      InitCommand(cmd);
      cmd.CacheName = CACHE_NAME;
      cmd.KeyName = "dolor";

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringContaining("sit"));
    }

    [Test]
    public void GetEntriesPartial()
    {
      var testCache = new TestCache();
      testCache.InnerCache.Add("lorem1", "ipsum1");
      testCache.InnerCache.Add("lorem2", "ipsum2");
      testCache.InnerCache.Add("dolor", "sit");

      var cmd = new Cmd.Cache();
      InitCommand(cmd);
      cmd.CacheName = CACHE_NAME;
      cmd.KeyName = "lorem";
      cmd.KeyNamePartial = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringContaining("lorem1"));
      Assert.That(result.Message, Is.StringContaining("ipsum1"));
      Assert.That(result.Message, Is.StringContaining("lorem2"));
      Assert.That(result.Message, Is.StringContaining("ipsum2"));
    }

    [Test]
    public void ClearAll()
    {
      var cmd = new Cmd.Cache();
      InitCommand(cmd);
      cmd.Clear = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));

      var cache = CacheManager.GetHtmlCache(Sitecore.Context.Site);
      Assert.That(cache.InnerCache.Size, Is.EqualTo(0));
    }

    [Test]
    public void ClearSingleCache()
    {
      var testCache = new TestCache();
      testCache.InnerCache.Add("lorem", "ipsum");
      testCache.InnerCache.Add("dolor", "sit");

      var cmd = new Cmd.Cache();
      InitCommand(cmd);
      cmd.CacheName = CACHE_NAME;
      cmd.Clear = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(testCache.InnerCache["lorem"], Is.Null);
    }

    [Test]
    public void ClearSingelEntry()
    {
      var testCache = new TestCache();
      testCache.InnerCache.Add("lorem", "ipsum");
      testCache.InnerCache.Add("dolor", "sit");

      var cmd = new Cmd.Cache();
      InitCommand(cmd);
      cmd.CacheName = CACHE_NAME;
      cmd.KeyName = "dolor";
      cmd.Clear = true;

      var result = cmd.Run();
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(testCache.InnerCache["dolor"], Is.Null);
      Assert.That(testCache.InnerCache["lorem"], Is.EqualTo("ipsum"));
    }

    private class TestCache : CustomCache
    {
      public TestCache() : base(CACHE_NAME, 2000)
      {
      }
    }
  }
}