using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using 马自达MC同步器.Resources.Enums;
using 马自达MC同步器.Resources.Models;

namespace 马自达MC同步器.Resources.ViewModels;

public partial class ModPageViewModel : ObservableObject
{

  [ObservableProperty] private string tip = "";

  public ObservableCollection<ModInfo>? ModInfos { get; set; } = [];

  [RelayCommand]
  private async void OpenItemToExplorer(IEnumerable<Object> items)
  {
    var selectedItems = items.ToList();
    var paths = items.Select(i => ((ModInfo)i).Name).ToList();
    //await Task.Run(async () =>
    //{
    //  Process.Start("explorer.exe", Path.GetDirectoryName(((ModInfo)selectedItems[0]).FullName)).WaitForInputIdle();
    //  await Task.Delay(2000);
    //});
    //SelectFiles(selectedItems.Select(e => ((ModInfo)e).FullName).ToList());

    string command = "dir";
    // 创建一个进程对象并设置参数
    Process process = new Process();
    process.StartInfo.FileName = $"{AppDomain.CurrentDomain.BaseDirectory}\\OpenFolderAndSelect.exe"; // 指定要执行的程序（cmd）
    process.StartInfo.Arguments = $"{Path.GetDirectoryName(((ModInfo)selectedItems[0]).FullName)} \"{string.Join("\" \"", paths)}\""; // 指定要执行的命令和参数（/c 选项表示执行完命令后自动关闭 cmd 窗口）

    // 启动进程
    process.Start();


    //Process.Start("OpenFolderAndSelect", $"{Path.GetDirectoryName(((ModInfo)selectedItems[0]).FullName)} {string.Join(" ", paths)}");
  }

  [RelayCommand]
  private void DeleteItem(IEnumerable<Object> items)
  {
    var selectedItems = items.ToList();
    var count = selectedItems.Count();
    for (int i = 0; i < count; i++)
    {
      var modInfo = (ModInfo)selectedItems[i];
      ModInfos.Remove(modInfo);
      FileSystem.DeleteFile(modInfo.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
    }
  }

  private void SelectFiles(List<string> filesToSelect)
  {
    var shellAppType = Type.GetTypeFromProgID("Shell.Application");
    dynamic shellApp = Activator.CreateInstance(shellAppType);
    foreach (var window in shellApp.Windows())
    {
      foreach (var folderItem in window.Document.Folder.Items())
      {
        if (Enumerable.Contains(filesToSelect, folderItem.Path))
        {
          window.Document.SelectItem(folderItem, 1 + 8);
          filesToSelect.Remove(folderItem.Path);
        }
      }
    }
  }

  public async Task TraverseMod()
  {
    Tip = "开始遍历mod文件";
    //ModInfos = null;
    ModInfos.Clear();
    var mods = Directory.GetFiles(Path.Combine(Settings.Default.GamePath, "mods"));
    //List<ModInfo> temp = [];
    foreach (var modFullName in mods)
      ModInfos.Add(new ModInfo(Path.GetFileName(modFullName), await CalculateFileMD5(modFullName), modFullName));
    //ModInfos = new ObservableCollection<ModInfo>(temp);
    // Application.Current.Dispatcher.Invoke(() => { ModDataGrid.ItemsSource = ModInfos; });
    Tip = "遍历完成";
  }

  private async Task<string> CalculateFileMD5(string filename)
  {
    string result = "";
    await Task.Run(() =>
    {
      using var md5 = MD5.Create();
      using var stream = File.OpenRead(filename);
      var hash = md5.ComputeHash(stream);
      result = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    });
    return result;
  }

  [RelayCommand]
  public async Task Synchronization()
  {
    await TraverseMod();
    foreach (var item in ModInfos!) item.Status = SynchronizationStatus.额外;

    Tip = "与服务器对比mod文件中";
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
      Tip = "完成";
      return;
    }


    // 设置最大并发下载任务数量
    SemaphoreSlim semaphore = new(Settings.Default.MaxDownloadCount);

    foreach (var remoteModInfo in modList)
    {
      var found = false;
      foreach (var localModInfo in ModInfos)
        if (remoteModInfo.MD5 == localModInfo.MD5)
        {
          localModInfo.Status = SynchronizationStatus.已同步;
          found = true;
          break;
        }

      if (!found)
      {
        ModInfo modInfo = new(remoteModInfo.Name, remoteModInfo.MD5, Path.Combine(Settings.Default.GamePath, "mods"), SynchronizationStatus.缺少);
        missList.Add(modInfo);
        ModInfos.Insert(0, modInfo);
      }
    }

    // 下载缺失的模组，使用 SemaphoreSlim 限制并发数量
    await DownloadMissingMods(missList, semaphore);
  }

  private async Task DownloadMissingMods(List<ModInfo> missList, SemaphoreSlim semaphore)
  {
    Tip = "下载所需mod中";
    List<Task> downloadTasks = [];

    foreach (var modInfo in missList)
      if (modInfo.Status != SynchronizationStatus.已同步)
      {
        await semaphore.WaitAsync(); // 等待信号量，限制并发数量
        downloadTasks.Add(Task.Run(async () =>
        {
          try
          {
            modInfo.Status = SynchronizationStatus.下载中;
            await App.webHelper.DownloadMod(modInfo.MD5,
              Path.Combine(Settings.Default.GamePath, "mods"));
            modInfo.Status = SynchronizationStatus.已同步;
          }
          catch (Exception ex)
          {
            modInfo.Status = SynchronizationStatus.缺少;
            Debug.WriteLine($"下载模组失败：{ex.Message}");
          }
          finally
          {
            semaphore.Release(); // 释放信号量
          }
        }));
      }

    await Task.WhenAll(downloadTasks);
    Tip = "完成";
  }
}