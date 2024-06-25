using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VPKInstaller
{
  public partial class InstallWindow : Form
  {
    const string defaultFolderName = "MAZDA MC Tool";
    FolderBrowserDialog FolderDialog = new FolderBrowserDialog();
    public InstallWindow()
    {
      InitializeComponent();
    }

    private bool IsPathValid()
    {
      if (string.IsNullOrWhiteSpace(PathTextBox.Text))
      {
        return false;
      }

      char[] invalidChars = Path.GetInvalidPathChars();
      foreach (char c in invalidChars)
      {
        if (PathTextBox.Text.Contains(c))
        {
          return false;
        }
      }
      return true;
    }
    private void SelectFolderBT_Click(object sender, EventArgs e)
    {
      if (FolderDialog.ShowDialog() != DialogResult.OK)
        return;

      if (Directory.Exists(FolderDialog.SelectedPath))
      {
        if (Directory.GetFiles(FolderDialog.SelectedPath).Length > 0)
          PathTextBox.Text = Path.Combine(FolderDialog.SelectedPath, defaultFolderName);
        else
          PathTextBox.Text = FolderDialog.SelectedPath;
      }
      else
      {
        Directory.CreateDirectory(FolderDialog.SelectedPath);
        PathTextBox.Text = FolderDialog.SelectedPath;
      }
    }

    private void PathTextBox_TextChanged(object sender, EventArgs e)
    {
      InstallBT.Enabled = IsPathValid();
    }

    private void InstallBT_Click(object sender, EventArgs e)
    {
      ProcessStartInfo processStartInfo = new ProcessStartInfo() 
      {
        FileName = "MAZDAMCTools-win-Setup.exe",
        WorkingDirectory = Directory.GetCurrentDirectory(),
        Arguments = $"--installto \"{PathTextBox.Text}\""
      };
      Process process = Process.Start(processStartInfo);
      if (process != null)
      {
        process.WaitForExit();
      }
      else
        MessageBox.Show("无法启动安装进程", "Failed to start the process");
      Close();
    }
  }
}
