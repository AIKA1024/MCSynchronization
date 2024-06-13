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

  #region Page

  [ObservableProperty] private ModPage modPage;
  [ObservableProperty] private ServerPage serverPage;
  [ObservableProperty] private SettingPage settingPage;

  #endregion
}