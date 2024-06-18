using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 马自达MC同步器.Resources.MyEventArgs
{
  public class GamePathChangedEventArgs : EventArgs
  {
    public string OldPath { get; set; }
    public string NewPath{ get; set; }

    public GamePathChangedEventArgs(string oldPath,string newPath)
    {
      OldPath = oldPath;
      NewPath = newPath;
    }
  }
}
