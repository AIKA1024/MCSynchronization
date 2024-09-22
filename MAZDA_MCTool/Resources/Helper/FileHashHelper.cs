using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace MAZDA_MCTool.Resources.Helper;

public static class FileHashHelper
{
  public static async Task<string> ComputeMd5HashAsync(string filename)
  {
    var result = "";
    await Task.Run(() =>
    {
      using var md5 = MD5.Create();
      using var stream = File.OpenRead(filename);
      var hash = md5.ComputeHash(stream);
      result = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    });
    return result;
  }
  
  public static async Task<string> ComputeSha1HashForFileAsync(string filePath)
  {
    var result = "";
    await Task.Run(() =>
    {
      using (SHA1 sha1 = SHA1.Create())
      {
        using (FileStream fileStream = File.OpenRead(filePath))
        {
          byte[] hashBytes = sha1.ComputeHash(fileStream);
          // 将字节数组转换为十六进制字符串
          StringBuilder sb = new StringBuilder();
          for (int i = 0; i < hashBytes.Length; i++)
            sb.Append(hashBytes[i].ToString("X2")); // 使用大写的十六进制字符
          result = sb.ToString();
        }
      }
    });
    return result;
  }
}