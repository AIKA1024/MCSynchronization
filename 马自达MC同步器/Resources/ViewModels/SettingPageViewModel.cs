using CommunityToolkit.Mvvm.ComponentModel;

namespace 马自达MC同步器.Resources.ViewModels;

public partial class SettingPageViewModel : ObservableObject
{
  [ObservableProperty] public string? address;
  [ObservableProperty] public string? gamePath;
  [ObservableProperty] public int maxDownloadCount;
  [ObservableProperty] public string? version;
}