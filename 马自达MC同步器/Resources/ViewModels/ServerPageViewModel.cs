using System.Collections.ObjectModel;
using fNbt;
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