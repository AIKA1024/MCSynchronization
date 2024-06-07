using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Velopack;

namespace 马自达MC同步器
{
  public class Program
  {
    [STAThread]
    public static void Main()
    {
      var vpk = VelopackApp.Build();
      vpk.Run();
      var app = new App();
      app.InitializeComponent();
      app.Run();
    }
  }
}
