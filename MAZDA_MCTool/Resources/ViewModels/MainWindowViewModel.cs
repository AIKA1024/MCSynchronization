using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using MAZDA_MCTool.Resources.Pages;
using MAZDA_MCTool.Resources.Views.Pages;

namespace MAZDA_MCTool.Resources.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
  public OpenFolderDialog FolderBrowserDialog = new();

  #region Page

  [ObservableProperty] private ModPage modPage;
  [ObservableProperty] private ServerPage serverPage;
  [ObservableProperty] private SettingPage settingPage;

  #endregion
}