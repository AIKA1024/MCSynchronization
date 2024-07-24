using Velopack;
using Velopack.Locators;
using Velopack.Windows;

namespace 马自达MC同步器;

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