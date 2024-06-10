namespace WebSDK.Models
{
  public class ModInfo
  {
    public ModInfo(string name, string md5)
    {
      Name = name;
      MD5 = md5;
    }

    public string Name { get; set; }

    public string MD5 { get; set; }
  }
}