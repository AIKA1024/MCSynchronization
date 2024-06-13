using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace 马自达MC同步器.Resources.Commands
{
  internal class SelectDirectory : ICommand
  {
    private SelectDirectory() { }

    public bool DialogButtonResult = false;

    public static SelectDirectory Instance = new SelectDirectory();

    public event EventHandler? CanExecuteChanged;

    public OpenFolderDialog FolderBrowserDialog = new();

    public bool CanExecute(object? parameter)
    {
      return true;
    }

    public void Execute(object? parameter)
    {
      if (FolderBrowserDialog.ShowDialog() != true)
      {
        DialogButtonResult = false;
        return;
      }

      DialogButtonResult = true;
      if (!CheckPath(FolderBrowserDialog.FolderName))
      {
        MessageBox.Show("路径不正确");
        return;
      }

      SetGamePath(FolderBrowserDialog.FolderName);
      return;
    }


    private bool CheckPath(string path)
    {
      if (string.IsNullOrEmpty(path) || !Directory.Exists(Path.Combine(path, "mods")))
        return false;
      return true;
    }

    private void SetGamePath(string gamePath)
    {
      Settings.Default.GamePath = gamePath;
      Settings.Default.Save();
    }
  }
}
