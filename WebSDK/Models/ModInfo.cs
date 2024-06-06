using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSDK.Models
{
  public class ModInfo
  {
    public string Name { get; set; }

    public string MD5 { get; set; }

    public ModInfo(string name, string md5)
    {
      Name = name;
      MD5 = md5;
    }
  }
}
