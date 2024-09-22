using Velopack;
using Velopack.Locators;
using Velopack.Windows;

namespace MAZDA_MCTool;

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
