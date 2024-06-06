using fNbt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 马自达MC同步器.Resources.Models
{
  public class ServerInfoOnlyData
  {
    public ServerInfoOnlyData()
    {
    }
    public byte hidden { set; get; }
    public byte preventsChatReports { set; get; }
    public string ip { set; get; }
    public string name { set; get; }
  }
}
