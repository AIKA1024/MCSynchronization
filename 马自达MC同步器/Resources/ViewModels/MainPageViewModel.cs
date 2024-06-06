using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 马自达MC同步器.Resources.Models;

namespace 马自达MC同步器.Resources.ViewModels
{
  public partial class MainPageViewModel : ObservableObject
  {
    [ObservableProperty]
    private string gamePath = "";
    [ObservableProperty]
    private string tip = "";

    public ObservableCollection<ModInfo>? ModInfos { get; set; } = [];
    public OpenFolderDialog FolderBrowserDialog = new OpenFolderDialog();
  }
}
