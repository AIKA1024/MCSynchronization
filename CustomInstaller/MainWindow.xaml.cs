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

namespace CustomInstaller
{
  public partial class MainWindow : Window
  {
    const string defaultFolderName = "MAZDA MC Tool";
    System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
    InstallInfo installInfo = new InstallInfo();
    public MainWindow()
    {
      InitializeComponent();
      DataContext = installInfo;
    }
    private bool IsPathValid(string path)
    {
      if (string.IsNullOrWhiteSpace(path))
      {
        return false;
      }

      char[] invalidChars = Path.GetInvalidPathChars();
      foreach (char c in invalidChars)
      {
        if (path.Contains(c))
        {
          return false;
        }
      }
      return true;
    }
    private void SelectFolderBT_Click(object sender, RoutedEventArgs e)
    {
      if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        return;

      DirectoryInfo directoryInfo = new DirectoryInfo(folderBrowserDialog.SelectedPath);
      if (directoryInfo.GetFileSystemInfos().Length > 0)
        installInfo.IntallPath = Path.Combine(folderBrowserDialog.SelectedPath, defaultFolderName);
      else
        installInfo.IntallPath = folderBrowserDialog.SelectedPath;
    }

    private void IntallBT_Click(object sender, RoutedEventArgs e)
    {
      if (!IsPathValid(installInfo.IntallPath))
      {
        MessageBox.Show("路径不合法", "提示");
        return;
      }
      string autoLaunchStr = installInfo.AutoLaunch ? "" : " --silent";

      ProcessStartInfo processStartInfo = new ProcessStartInfo()
      {
        FileName = "MAZDAMCTools-win-Setup.exe",
        WorkingDirectory = Directory.GetCurrentDirectory(),
        Arguments = $"--installto \"{installInfo.IntallPath}\"{autoLaunchStr}"
      };
      Process process = Process.Start(processStartInfo);
      if (process != null)
      {
        process.WaitForExit();
      }
      else
        MessageBox.Show("无法启动安装进程", "Failed to start the process");

      if (!installInfo.AutoLaunch)
        MessageBox.Show("安装完成");
      Close();
    }
  }
}
