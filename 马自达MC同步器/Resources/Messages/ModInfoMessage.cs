using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 马自达MC同步器.Resources.Models;

namespace 马自达MC同步器.Resources.Messages
{
  public class ModInfoMessage
  {
    public ModInfo modInfo { get; set; }
    public ModInfoMessage(ModInfo _modInfo)
    {
      modInfo = _modInfo;
    }
  }
}
