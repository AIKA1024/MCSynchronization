using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using fNbt;
using WebSDK;
using 马自达MC同步器.Resources.Models;
using 马自达MC同步器.Resources.ViewModels;

namespace 马自达MC同步器.Resources.Pages
{
  /// <summary>
  /// ServerAddressPage.xaml 的交互逻辑
  /// </summary>
  public partial class ServerPage : Page
  {
    private ServerPageViewModel viewModel;
    public ServerPage()
    {
      InitializeComponent();
    }

    private void Page_Initialized(object sender, EventArgs e)
    {
      viewModel = new ServerPageViewModel(NBTHelper.GetNBTFile());
      DataContext = viewModel;
    }

    private void ClickSaveNBT(object sender, RoutedEventArgs e)
    {
      NBTHelper.SaveNBTFile();
    }

    private void AddServerWithView(string name, string ip, byte hidden)
    {
      var nbtFile = NBTHelper.GetNBTFile();
      var nbtCompoundTag = new NbtCompound()
      {
        new NbtString(nameof(name),name),
        new NbtString(nameof(ip), ip),
        new NbtByte(nameof(hidden), hidden)
      };
      ((NbtList)nbtFile.RootTag["servers"]).Add(nbtCompoundTag);
      viewModel.ServerUrl.Add(new ServerInfo(nbtCompoundTag));
    }

    private async void ClickSynchronizationServer(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(Settings.Default.GamePath))
      {
        MessageBox.Show("请先选择游戏文件夹");
        return;
      }
      SyServerButton.IsEnabled = false;
      string jsonStr = await App.webHelper.GetRemoteServerList();
      SyServerButton.IsEnabled = true;
      if (string.IsNullOrEmpty(jsonStr))
      {
        MessageBox.Show("连接服务器失败!");
        return;
      }

      var serverList = JsonSerializer.Deserialize<List<ServerInfoOnlyData>>(jsonStr);
      if (serverList == null)
      {
        MessageBox.Show("服务器返回了空服务器列表!");
        return;
      }
      foreach (var server in serverList)
      {
        if (viewModel.ServerUrl.Any(s => s.ip == server.ip))
          continue;

        AddServerWithView(server.name, server.ip, server.hidden);
      }
    }

    private void ClickRemove(object sender, RoutedEventArgs e)
    {
      Button button = (Button)e.Source;
      int index = viewModel.ServerUrl.IndexOf((ServerInfo)button.DataContext);
      viewModel.ServerUrl.RemoveAt(index);
      ((NbtList)NBTHelper.GetNBTFile().RootTag["servers"]).RemoveAt(index);
    }

    private void ClickNewServer(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(Settings.Default.GamePath))
      {
        MessageBox.Show("请先选择游戏文件夹");
        return;
      }
      AddWindow.Visibility = Visibility.Visible;
    }

    private void CancelAddServer(object sender, RoutedEventArgs e)
    {
      AddWindow.Visibility = Visibility.Hidden;
    }
  }
}
