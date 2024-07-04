using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 马自达MC同步器.Resources.Helper;


//todo 应该实现某种机制，可以实现多任务进度汇报
internal partial class TaskInfoHelper : ObservableObject
{
  public static TaskInfoHelper Instance { get; } = new();

  [ObservableProperty] private string taskInfo = "";
}