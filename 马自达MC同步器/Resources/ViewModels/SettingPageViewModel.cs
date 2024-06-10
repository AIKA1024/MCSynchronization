using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 马自达MC同步器.Resources.ViewModels;

public partial class SettingPageViewModel : ObservableObject
{
  [ObservableProperty] public string? version;
  [ObservableProperty] public string? address;
  [ObservableProperty] public int maxDownloadCount;
  [ObservableProperty] public string? gamePath;
}