using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using 马自达MC同步器.Resources.Models;

namespace 马自达MC同步器.Resources.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
  public OpenFolderDialog FolderBrowserDialog = new();
  [ObservableProperty] private string gamePath = "";
  [ObservableProperty] private string tip = "";

  public ObservableCollection<ModInfo>? ModInfos { get; set; } = [];
}