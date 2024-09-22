using System.Diagnostics;
using System.Net.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace WebSDK
{
  public class WebHelper
  {
    private readonly HttpClient httpClient;

    public WebHelper()
    {
      //var handler = new HttpClientHandler
      //{
      //  ServerCertificateCustomValidationCallback = (sender, cert, chain, SslPolicyErrors) => true,
      //};
      var handler = new SocketsHttpHandler()
      {
        ConnectTimeout = TimeSpan.FromSeconds(8), // 设置连接超时为10秒
        SslOptions = new SslClientAuthenticationOptions
        {
          RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        }
      };
      httpClient = new HttpClient(handler);
    }


    public async Task<string> GetAsync(string url)
    {
      try
      {
        // 发送 GET 请求
        var response = await httpClient.GetAsync(url);

        // 检查响应是否成功
        if (response.IsSuccessStatusCode)
        {
          // 读取响应内容
          var responseBody = await response.Content.ReadAsStringAsync();
          return responseBody;
        }
        return "";
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
        return "";
      }
    }

    public async Task DownloadImageAsync(string url, string localPath, int numberOfRetries = 1)
    {
      for (int i = 0; i < numberOfRetries; i++)
      {
        // 发送HTTP GET请求获取图片数据
        HttpResponseMessage response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
          continue;

        // 读取图片数据为字节数组
        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();

        // 将图片数据保存到本地文件
        await File.WriteAllBytesAsync(localPath, imageBytes);
        break;
      }
    }
    /// <summary>
    /// 从ModFrom下载mod
    /// </summary>
    /// <param name="sha1Hash"></param>
    /// <returns>下载成功的mod列表</returns>
    public async Task<List<string>?> DownLoadModFromModrinth(List<string> sha1Hash, string path)
    {
      var modInfos = await GetVersionListFromHashasync(sha1Hash);
      if (modInfos == null || !modInfos.Any())
        return null;

      List<string> successModList = new List<string>();
      foreach (var item in modInfos)
      {
        var fileDirectLink = item.Value["files"][0]["url"].ToString();
        var response = await httpClient.GetAsync(fileDirectLink);
        for (int i = 0; i < 3; i++)//3次重试
        {
          if (!response.IsSuccessStatusCode)
            continue;
          string fileName = Path.GetFileName(fileDirectLink);
          var fileFullPath = Path.Combine(path, fileName);
          if (File.Exists(fileFullPath))
            Console.Error.WriteLine($"{fileFullPath}已经存在");
          using (Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(fileFullPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            await contentStream.CopyToAsync(fileStream);
          successModList.Add(item.Key);
          break;
        }
      }
      return successModList;
    }

    public async Task<bool> DownLoadModFromModrinth(string sha1Hash, string path)
    {
      var modInfo = await GetVersionFromHashAsnyc(sha1Hash);
      if (modInfo == null)
        return false;

      var fileDirectLink = modInfo["files"][0]["url"].ToString();
      var response = await httpClient.GetAsync(fileDirectLink);
      for (int i = 0; i < 3; i++)//3次重试
      {
        if (!response.IsSuccessStatusCode)
          continue;
        string fileName = Path.GetFileName(fileDirectLink);
        var fileFullPath = Path.Combine(path, fileName);
        if (File.Exists(fileFullPath))
          Console.Error.WriteLine($"{fileFullPath}已经存在");
        using (Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(fileFullPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
          await contentStream.CopyToAsync(fileStream);
        return true;
      }
      return false;
    }

    public async Task DownloadModFromFriends(string url, string sha1, string savePath)
    {
      var formData = new Dictionary<string, string>
      {
        { "Sha1", sha1 }
      };
      var content = new FormUrlEncodedContent(formData);
      var response = await httpClient.PostAsync(url, content);
      // 确保响应成功
      response.EnsureSuccessStatusCode();
      // 读取响应内容
      if (response.Content.Headers.TryGetValues("Content-Disposition", out IEnumerable<string> values))
      {
        var contentDisposition = values.First();
        var index = contentDisposition.IndexOf("=") + 1;
        var fileName = contentDisposition[index..];
        fileName = HttpUtility.UrlDecode(fileName);
        var fileFullPath = Path.Combine(savePath, fileName);
        if (File.Exists(fileFullPath))
        {
          Console.Error.WriteLine($"{fileFullPath}已经存在");
        }
        else
        {
          // 读取响应内容并保存到文件
          using var fileStream = File.Create(fileFullPath); // 使用从头部获取的文件名保存文件
          // 将响应内容写入到文件流中
          await response.Content.CopyToAsync(fileStream);
        }
      }
      else
      {
        Console.WriteLine("未找到文件名信息。");
      }
    }

    #region modrinthAPi
    public async Task<JsonObject?> GetVersionListFromHashasync(List<string>? sha1HashList)
    {
      if (sha1HashList == null || sha1HashList.Count == 0)
        return null;

      var requestBody = new
      {
        hashes = sha1HashList,
        algorithm = "sha1"
      };
      var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

      var response = await httpClient.PostAsync("https://api.modrinth.com/v2/version_files", jsonContent);
      // 确保响应成功
      if (response.IsSuccessStatusCode)
      {
        // 读取响应内容
        string responseContent = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(responseContent) || responseContent == "{}")
          return null;
        return JsonSerializer.Deserialize<JsonObject>(responseContent); ;
      }
      return null;
    }

    public async Task<JsonArray?> GetProjectListFromID(List<string>? idList)
    {
      if (idList == null || !idList.Any()) return null;

      var resultStr = await
        GetAsync($"https://api.modrinth.com/v2/projects?ids={JsonSerializer.Serialize(idList)}");
      if (string.IsNullOrEmpty(resultStr))
        return null;

      return JsonSerializer.Deserialize<JsonArray?>(resultStr);
    }

    public async Task<JsonObject?> GetVersionFromHashAsnyc(string sha1Hash)
    {
      var response = await GetAsync($"https://api.modrinth.com/v2/version_file/{sha1Hash}");
      if (string.IsNullOrEmpty(response))
        return null;
      return JsonSerializer.Deserialize<JsonObject>(response);
    }

    public async Task<JsonObject?> GetProjectFromID(string id)
    {
      var response = await GetAsync($"https://api.modrinth.com/v2/project/{id}");
      if (string.IsNullOrEmpty(response))
        return null;
      return JsonSerializer.Deserialize<JsonObject>(response);
    }
    #endregion
  }
}