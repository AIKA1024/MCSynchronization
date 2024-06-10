using fNbt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using 马自达MC同步器.Resources.Models;

namespace 马自达MC同步器.Resources.ViewModels;

public class ServerPageViewModel
{
  private NbtFile nbtFile;

  public ServerPageViewModel(NbtFile nbtFile)
  {
    this.nbtFile = nbtFile;
    if (nbtFile == null)
      return;
    foreach (var item in ((NbtList)nbtFile.RootTag["servers"]).Cast<NbtCompound>())
      ServerUrl.Add(new ServerInfo(item));
  }

  public ObservableCollection<ServerInfo> ServerUrl { get; set; } = [];
}