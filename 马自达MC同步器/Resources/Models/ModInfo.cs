using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using 马自达MC同步器.Resources.Enums;

namespace 马自达MC同步器.Resources.Models;

public partial class ModInfo : ObservableObject
{
  [ObservableProperty] private string fullFileName;
  private string md5 = "";
  [ObservableProperty] private string disPlayName;

  [ObservableProperty] private SynchronizationStatus status;

  [ObservableProperty] private string? version;

  [ObservableProperty] private BitmapImage? logo;

  [ObservableProperty] private string? description;

  public string MD5
  {
    get => md5;
    set => SetProperty(ref md5, value);
  }
}