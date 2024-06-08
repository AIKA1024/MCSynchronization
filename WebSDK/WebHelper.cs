using System.Net.Security;
using WebSDK.Models;
using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace WebSDK
{
  public class WebHelper
  {
    private HttpClient httpClient;
    public WebHelper(Uri uri)
    {
      HttpClientHandler handler = new HttpClientHandler
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
        HttpResponseMessage response = await httpClient.GetAsync("GetModList");

        // 检查响应是否成功
        if (response.IsSuccessStatusCode)
        {
          // 读取响应内容
          string responseBody = await response.Content.ReadAsStringAsync();
          return responseBody;
        }
        else
        {
          return "";
        }
      }
      catch (HttpRequestException ex)
      {
        Debug.WriteLine(ex.Message);
        return "";
      }
    }

    public async Task DownloadMod(string md5, string savePath)
    {
      var formData = new Dictionary<string, string>
      {
         { "MD5", md5 }
      };
      var content = new FormUrlEncodedContent(formData);
      HttpResponseMessage response = await httpClient.PostAsync("Download", content);
      // 确保响应成功
      response.EnsureSuccessStatusCode();
      // 读取响应内容
      if (response.Content.Headers.TryGetValues("Content-Disposition", out IEnumerable<string> values))
      {
        string contentDisposition = values.First();
        int index = contentDisposition.IndexOf("=") + 1;
        string fileName = contentDisposition[index..];
        fileName = HttpUtility.UrlDecode(fileName);
        string fileFullPath = Path.Combine(savePath, fileName);
        if (File.Exists(fileFullPath))
        {
          Console.Error.WriteLine($"{fileFullPath}已经存在");
        }
        else
        {
          // 读取响应内容并保存到文件
          using FileStream fileStream = File.Create(fileFullPath); // 使用从头部获取的文件名保存文件
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
        HttpResponseMessage response = await httpClient.GetAsync("GetServerList");

        // 检查响应是否成功
        if (response.IsSuccessStatusCode)
        {
          // 读取响应内容
          string responseBody = await response.Content.ReadAsStringAsync();
          return responseBody;
        }
        else
        {
          return "";
        }
      }
      catch (HttpRequestException ex)
      {
        Console.WriteLine(ex.Message);
        return "";
      }
    }
  }
}

