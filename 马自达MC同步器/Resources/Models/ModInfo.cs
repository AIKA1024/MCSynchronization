using CommunityToolkit.Mvvm.ComponentModel;
using 马自达MC同步器.Resources.Enums;

namespace 马自达MC同步器.Resources.Models;

public partial class ModInfo : ObservableObject
{
  private string md5 = "";
  [ObservableProperty] public string name;

  [ObservableProperty] private SynchronizationStatus status;

  public ModInfo(string name, string md5, SynchronizationStatus status = SynchronizationStatus.未同步)
  {
    Name = name;
    MD5 = md5;
    this.status = status;
  }

  public string MD5
  {
    get => md5;
    set => SetProperty(ref md5, value);
  }
}