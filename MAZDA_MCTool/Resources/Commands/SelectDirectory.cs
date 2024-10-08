﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MAZDA_MCTool.Resources.MyEventArgs;

namespace MAZDA_MCTool.Resources.Commands
{
  internal class SelectDirectory : ICommand
  {
    private SelectDirectory() { }

    public bool DialogButtonResult = false;

    //todo: 应该改用MVVM工具包的信使传递消息，而不是使用单例
    public static SelectDirectory Instance { get; } = new SelectDirectory();

    public event EventHandler? CanExecuteChanged;

    public OpenFolderDialog FolderBrowserDialog = new();

    public event Action<object,GamePathChangedEventArgs> PathChanged;

    public bool CanExecute(object? parameter)
    {
      return true;
    }

    public void Execute(object? parameter)
    {
      var oldPath = FolderBrowserDialog.FolderName;
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
      if (oldPath != FolderBrowserDialog.FolderName)
        PathChanged?.Invoke(this,new GamePathChangedEventArgs(oldPath, FolderBrowserDialog.FolderName));
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
