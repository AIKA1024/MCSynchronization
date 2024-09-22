using CustomInstaller.Resources.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace CustomInstaller.Resources.Views.Pages
{
  /// <summary>
  /// UnInstallPage.xaml 的交互逻辑
  /// </summary>
  public partial class UnInstallPage : Page
  {
    public UnInstallPage()
    {
      InitializeComponent();
    }

    private void ReIntallBT_Click(object sender, RoutedEventArgs e)
    {
      InstallInfo installInfo = (InstallInfo)DataContext;
      ProcessStartInfo processStartInfo = new ProcessStartInfo()
      {
        FileName = "MAZDAMCTools-win-Setup.exe",
        WorkingDirectory = Directory.GetCurrentDirectory(),
        Arguments = $"--installto \"{installInfo.InstallLocation}\""
      };
      Process process = Process.Start(processStartInfo);
      if (process != null)
      {
        process.WaitForExit();
      }
      else
        System.Windows.MessageBox.Show("无法启动安装进程", "Failed to start the process");

      App.Current.Shutdown();
    }

    private void LauncherBT_Click(object sender, RoutedEventArgs e)
    {
      InstallInfo installInfo = (InstallInfo)DataContext;
      Process.Start(Path.Combine(installInfo.InstallLocation, "current", "马自达MC同步器.exe"));
      App.Current.Shutdown();
    }
  }
}
