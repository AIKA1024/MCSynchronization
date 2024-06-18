using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace 马自达MC同步器.Resources.Helper
{
  public class CacheHelper
  {
    public bool HasModCache(string sha1Hash) =>
      File.Exists($"{Path.Combine(App.Current.CachePath, sha1Hash)}.json");

    public JsonObject? GetModCache(string sha1Hash)
    {
      var cacheFilePath = $"{Path.Combine(App.Current.CachePath, sha1Hash)}.json";
      var fileText = File.ReadAllText(cacheFilePath);
      if (string.IsNullOrEmpty(fileText))
        return null;
      return JsonSerializer.Deserialize<JsonObject>(fileText);
    }
    public void DelectAllCache()
    {

    }
  }
}
