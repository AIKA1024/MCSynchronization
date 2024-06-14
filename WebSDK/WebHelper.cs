using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace WebSDK
{
  public class WebHelper
  {
    private readonly HttpClient httpClient;

    public WebHelper(Uri uri)
    {
      var handler = new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, SslPolicyErrors) => true
      };
      httpClient = new HttpClient(handler)
      {
        BaseAddress = uri
      };
    }

    public void ChangeBaseAddress(Uri uri)
    {
      httpClient.BaseAddress = uri;
    }

    public async Task<string> GetRemoteModList()
    {
      try
      {
        // 发送 GET 请求
        var response = await httpClient.GetAsync("GetModList");

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

    public async Task DownloadMod(string sha1, string savePath)
    {
      var formData = new Dictionary<string, string>
      {
        { "Sha1", sha1 }
      };
      var content = new FormUrlEncodedContent(formData);
      var response = await httpClient.PostAsync("Download", content);
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

    public async Task<string> GetRemoteServerList()
    {
      try
      {
        // 发送 GET 请求
        var response = await httpClient.GetAsync("GetServerList");

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
        Console.WriteLine(ex.Message);
        return "";
      }
    }
  }
}