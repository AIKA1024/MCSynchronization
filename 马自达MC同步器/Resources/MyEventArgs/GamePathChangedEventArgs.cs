using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 马自达MC同步器.Resources.MyEventArgs
{
  public class GamePathChangedEventArgs : EventArgs
  {
    public string oldPath;
    public string NewPath;

    public GamePathChangedEventArgs(string oldPath,string NewPath)
    {
      this.oldPath = oldPath;
      this.NewPath = NewPath;
    }
  }
}
