using System.Windows;
using Velopack;
using 马自达MC同步器.Resources.Views.Windows;

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

    var startWindow = new StartWindow();
    startWindow.ShowDialog();

    if (startWindow.MainWindow!=null)
      app.Run(startWindow.MainWindow);
    else
      app.Shutdown();
  }
}