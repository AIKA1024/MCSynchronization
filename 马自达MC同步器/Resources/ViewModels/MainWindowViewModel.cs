using System.IO;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using 马自达MC同步器.Resources.Pages;
using 马自达MC同步器.Resources.Views.Pages;

namespace 马自达MC同步器.Resources.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
  public OpenFolderDialog FolderBrowserDialog = new();

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

  public void SelectDirectory(object sender, ExecutedRoutedEventArgs e)
  {
    if (FolderBrowserDialog.ShowDialog() != true)
      return;
    if (!CheckPath(FolderBrowserDialog.FolderName))
    {
      MessageBox.Show("路径不正确");
      return;
    }

    SetGamePath(FolderBrowserDialog.FolderName);
  }

  #region Page

  [ObservableProperty] private ModPage modPage;
  [ObservableProperty] private ServerPage serverPage;
  [ObservableProperty] private SettingPage settingPage;

  #endregion
}