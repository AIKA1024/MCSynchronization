namespace WebSDK.Models
{
  public class ModInfo
  {
    public ModInfo(string name, string sha1)
    {
      Name = name;
      Sha1 = sha1;
    }

    public string Name { get; set; }

    public string Sha1 { get; set; }
  }
}