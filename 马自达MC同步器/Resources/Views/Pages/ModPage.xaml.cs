using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using 马自达MC同步器.Resources.ViewModels;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Net.Http;
using 马自达MC同步器.Resources.Models;
using WebSDK;
using System.Security.Policy;
using System.Threading;
using System.Diagnostics;


namespace 马自达MC同步器.Resources.Pages;

/// <summary>
/// MainPage.xaml 的交互逻辑
/// </summary>
public partial class ModPage : Page
{
  private MainPageViewModel mainPageViewModel;

  public ModPage()
  {
    mainPageViewModel = new MainPageViewModel();
    if (!string.IsNullOrEmpty(Settings.Default.GamePath))
      mainPageViewModel.GamePath = Settings.Default.GamePath;
    DataContext = mainPageViewModel;
    InitializeComponent();
  }

  private async void Button_Click(object sender, RoutedEventArgs e)
  {
    if (mainPageViewModel.FolderBrowserDialog.ShowDialog() != true)
      return;
    if (!CheckPath(mainPageViewModel.FolderBrowserDialog.FolderName))
    {
      SyButton.IsEnabled = false;
      MessageBox.Show("路径不正确");
      return;
    }

    SFButton.IsEnabled = false;
    SetGamePath(mainPageViewModel.FolderBrowserDialog.FolderName);
    await TraverseMod();
    SyButton.IsEnabled = true;
    SFButton.IsEnabled = true;
  }

  private bool CheckPath(string path)
  {
    if (string.IsNullOrEmpty(path) || !Directory.Exists(System.IO.Path.Combine(path, "mods")))
      return false;
    return true;
  }

  public async Task TraverseMod()
  {
    mainPageViewModel.Tip = "开始遍历mod文件";
    mainPageViewModel.ModInfos = null;
    var mods = Directory.GetFiles(System.IO.Path.Combine(mainPageViewModel.GamePath, "mods"));
    List<ModInfo> temp = [];
    foreach (var mod in mods)
      await Task.Run(() => { temp.Add(new ModInfo(System.IO.Path.GetFileName(mod), CalculateFileMD5(mod))); });
    mainPageViewModel.ModInfos = new ObservableCollection<ModInfo>(temp);
    Application.Current.Dispatcher.Invoke(() => { ModDataGrid.ItemsSource = mainPageViewModel.ModInfos; });
    mainPageViewModel.Tip = "遍历完成";
  }

  private void SetGamePath(string gamePath)
  {
    mainPageViewModel.GamePath = gamePath;
    Settings.Default.GamePath = gamePath;
    Settings.Default.Save();
  }

  private async void Page_Initialized(object sender, EventArgs e)
  {
    SyButton.IsEnabled = false;
    if (CheckPath(Settings.Default.GamePath))
    {
      await TraverseMod();
      SyButton.IsEnabled = true;
      SFButton.IsEnabled = true;
    }
  }

  public static string CalculateFileMD5(string filename)
  {
    using var md5 = MD5.Create();
    using var stream = File.OpenRead(filename);
    var hash = md5.ComputeHash(stream);
    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
  }

  private async void Synchronization(object sender, RoutedEventArgs e)
  {
    await TraverseMod();
    foreach (var item in mainPageViewModel.ModInfos!) item.Status = Enums.SynchronizationStatus.额外;

    mainPageViewModel.Tip = "与服务器对比mod文件中";
    var missList = new List<ModInfo>();
    var jsonStr = await App.webHelper.GetRemoteModList();
    if (string.IsNullOrEmpty(jsonStr))
    {
      MessageBox.Show("与服务器链接失败");
      return;
    }

    var modList = JsonSerializer.Deserialize<List<ModInfo>>(jsonStr);

    if (modList == null || modList.Count == 0)
    {
      MessageBox.Show("服务器返回了空的模组列表");
      mainPageViewModel.Tip = "完成";
      return;
    }


    // 设置最大并发下载任务数量
    SemaphoreSlim semaphore = new(Settings.Default.MaxDownloadCount);

    foreach (var remoteModInfo in modList)
    {
      var found = false;
      foreach (var localModInfo in mainPageViewModel.ModInfos)
        if (remoteModInfo.MD5 == localModInfo.MD5)
        {
          localModInfo.Status = Enums.SynchronizationStatus.已同步;
          found = true;
          break;
        }

      if (!found)
      {
        ModInfo modInfo = new(remoteModInfo.Name, remoteModInfo.MD5, Enums.SynchronizationStatus.缺少);
        missList.Add(modInfo);
        mainPageViewModel.ModInfos.Insert(0, modInfo);
      }
    }

    // 下载缺失的模组，使用 SemaphoreSlim 限制并发数量
    await DownloadMissingMods(missList, semaphore);
  }

  private async Task DownloadMissingMods(List<ModInfo> missList, SemaphoreSlim semaphore)
  {
    mainPageViewModel.Tip = "下载所需mod中";
    List<Task> downloadTasks = [];

    foreach (var modInfo in missList)
      if (modInfo.Status != Enums.SynchronizationStatus.已同步)
      {
        await semaphore.WaitAsync(); // 等待信号量，限制并发数量
        downloadTasks.Add(Task.Run(async () =>
        {
          try
          {
            modInfo.Status = Enums.SynchronizationStatus.下载中;
            await App.webHelper.DownloadMod(modInfo.MD5,
              System.IO.Path.Combine(Settings.Default.GamePath, "mods"));
            modInfo.Status = Enums.SynchronizationStatus.已同步;
          }
          catch (Exception ex)
          {
            modInfo.Status = Enums.SynchronizationStatus.缺少;
            Debug.WriteLine($"下载模组失败：{ex.Message}");
          }
          finally
          {
            semaphore.Release(); // 释放信号量
          }
        }));
      }

    await Task.WhenAll(downloadTasks);
    mainPageViewModel.Tip = "完成";
  }
}