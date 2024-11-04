using CustomInstaller.Resources.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace CustomInstaller.Resources.Views.Pages
{
  /// <summary>
  /// InstallPage.xaml 的交互逻辑
  /// </summary>
  public partial class InstallPage : Page
  {
    const string defaultFolderName = "MAZDA MC Tool";
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    InstallSetting installInfo = new InstallSetting();
    public InstallPage()
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

      string shortcutStr = "";
      
      if (installInfo.CreateDeskTopShortcuts && installInfo.CreateStartMenuShortcuts)
      {
        shortcutStr = " -- --shortcuts Desktop,StartMenuRoot";
      }
      else if (installInfo.CreateDeskTopShortcuts)
      {
        shortcutStr = " -- --shortcuts Desktop";
      }
      else if (installInfo.CreateStartMenuShortcuts)
      {
        shortcutStr = " -- --shortcuts StartMenuRoot";
      }
      string installStr = string.IsNullOrWhiteSpace(installInfo.IntallPath) ?"": $" -t \"{installInfo.IntallPath}\"";
      string autoLaunchStr = installInfo.AutoLaunch ? "" : " --silent";
      string installProgramPath = CopyCoreInstanllProgram("MAZDAMCTools-win-Setup.exe");
      if (string.IsNullOrEmpty(installProgramPath))
        return;

      ProcessStartInfo processStartInfo = new ProcessStartInfo()
      {
        FileName = installProgramPath,
        WorkingDirectory = Directory.GetCurrentDirectory(),
        Arguments = $"{installStr}{autoLaunchStr}{shortcutStr}",
        UseShellExecute = false
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
      File.Delete(installProgramPath);
      App.Current.Shutdown();
    }

    private string CopyCoreInstanllProgram(string embeddedExeName)
    {
      // 获取当前程序集
      var assembly = Assembly.GetExecutingAssembly();

      // 构建嵌入资源的完整名称
      var resourceName = $"{assembly.GetName().Name}.Resources.CoreProgram.{embeddedExeName}";

      using (Stream stream = assembly.GetManifestResourceStream(resourceName))
      {
        if (stream == null)
        {
          MessageBox.Show("该安装包已经损坏！", "警告");
          return "";
        }

        // 创建临时文件路径
        string tempFilePath = Path.Combine(Path.GetTempPath(), embeddedExeName);

        // 将嵌入的 EXE 写入临时文件
        using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
        {
          stream.CopyTo(fileStream);
        }

        // 设置临时文件为可执行
        File.SetAttributes(tempFilePath, FileAttributes.Normal);

        // 运行 EXE 文件目录
        return tempFilePath;
      }
    }
  }
}
