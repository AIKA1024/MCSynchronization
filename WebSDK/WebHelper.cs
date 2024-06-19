using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using System.Text;

namespace WebSDK
{
  public class WebHelper
  {
    private readonly HttpClient httpClient;

    public WebHelper()
    {
      var handler = new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, SslPolicyErrors) => true
      };
      httpClient = new HttpClient(handler)
      {
        Timeout = TimeSpan.FromSeconds(200)
      };
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
      catch (HttpRequestException ex)
      {
        Debug.WriteLine(ex.Message);
        return "";
      }
    }

    public async Task DownloadImageAsync(string url, string localPath,int numberOfRetries = 1)
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

    public async Task DownloadMod(string url, string sha1, string savePath)
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