using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MAZDA_MCTool.Resources.Models;

namespace MAZDA_MCTool.Resources.Messages
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
