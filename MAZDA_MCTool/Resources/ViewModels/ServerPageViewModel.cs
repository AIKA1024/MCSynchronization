using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using fNbt;
using MAZDA_MCTool.Resources.Commands;
using MAZDA_MCTool.Resources.Helper;
using MAZDA_MCTool.Resources.Models;
using MAZDA_MCTool.Resources.MyEventArgs;

namespace MAZDA_MCTool.Resources.ViewModels;

public partial class ServerPageViewModel
{
  public ServerPageViewModel()
  {
    if (NBTHelper.GetNBTFile() == null)
      return;

    foreach (var item in ((NbtList)NBTHelper.GetNBTFile().RootTag["servers"]).Cast<NbtCompound>())
      ServerInfos.Add(new ServerInfo(item));
    SelectDirectory.Instance.PathChanged += GamePathChanged;
  }
  private void GamePathChanged(object sender, GamePathChangedEventArgs args)
  {
    Update();
  }

  public ObservableCollection<ServerInfo> ServerInfos { get; set; } = [];

  public void Update()
  {
    ServerInfos.Clear();
    foreach (var item in ((NbtList)NBTHelper.GetNBTFile().RootTag["servers"]).Cast<NbtCompound>())
      ServerInfos.Add(new ServerInfo(item));
  }

  private void AddServer(string name, string ip, byte hidden)
  {
    var nbtCompoundTag = new NbtCompound
    {
      new NbtString(nameof(name), name),
      new NbtString(nameof(ip), ip),
      new NbtByte(nameof(hidden), hidden)
    };
    ((NbtList)NBTHelper.GetNBTFile().RootTag["servers"]).Add(nbtCompoundTag);
    ServerInfos.Add(new ServerInfo(nbtCompoundTag));
  }

  [RelayCommand]
  private void SaveNBT()
  {
    NBTHelper.SaveNBTFile();
  }

  [RelayCommand]
  private async Task SynchronizateServer()
  {
    if (string.IsNullOrEmpty(Settings.Default.GamePath))
    {
      MessageBox.Show("请先选择游戏文件夹");
      return;
    }

    var jsonStr = await App.Current.webHelper.GetAsync(Settings.Default.Address + "/GetServerList");
    if (string.IsNullOrEmpty(jsonStr))
    {
      MessageBox.Show("连接服务器失败!");
      return;
    }

    //由于serverInfo有额外的操作，因此无法使用该模型类
    var serverList = JsonSerializer.Deserialize<List<JsonObject>>(jsonStr);
    if (serverList == null)
    {
      MessageBox.Show("服务器返回了空服务器列表!");
      return;
    }

    foreach (JsonObject item in serverList)
    {
      if (ServerInfos.Any(s => s.ip == item["ip"].ToString()))
        continue;
      AddServer(item["name"].ToString(), item["ip"].ToString(), item["hidden"].GetValue<byte>());
    }
  }

  [RelayCommand]
  private void Remove(ServerInfo serverInfo)
  {
    var index = ServerInfos.IndexOf(serverInfo);
    ServerInfos.RemoveAt(index);
    ((NbtList)NBTHelper.GetNBTFile().RootTag["servers"]).RemoveAt(index);
  }
}