using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 马自达MC同步器.Resources.Helper;

internal partial class TaskInfoHelper : ObservableObject
{
  public static TaskInfoHelper Instance { get; } = new();

  [ObservableProperty] private string taskInfo = "";
}