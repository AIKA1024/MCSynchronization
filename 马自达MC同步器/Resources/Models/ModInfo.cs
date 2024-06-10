using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 马自达MC同步器.Resources.Enums;

namespace 马自达MC同步器.Resources.Models;

public partial class ModInfo : ObservableObject
{
  [ObservableProperty] public string name;

  [ObservableProperty] private SynchronizationStatus status;

  private string md5 = "";

  public string MD5
  {
    get => md5;
    set => SetProperty(ref md5, value);
  }

  public ModInfo(string name, string md5, SynchronizationStatus status = SynchronizationStatus.未同步)
  {
    Name = name;
    MD5 = md5;
    this.status = status;
  }
}