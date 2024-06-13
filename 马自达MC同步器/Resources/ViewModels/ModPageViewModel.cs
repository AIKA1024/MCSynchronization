using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic.FileIO;
using System.IO.Compression;
using Tomlyn;
using Tomlyn.Model;
using 马自达MC同步器.Resources.Enums;
using 马自达MC同步器.Resources.Models;
using System.Windows.Media.Imaging;
using System;
using Microsoft.VisualBasic;
using 马自达MC同步器.Resources.Helper;

namespace 马自达MC同步器.Resources.ViewModels;

public partial class ModPageViewModel : ObservableObject
{
  [ObservableProperty] private string tip = "";

  public ObservableCollection<ModInfo>? ModInfos { get; set; } = [];

  [RelayCommand]
  private void OpenItemToExplorer(IEnumerable<object> items)
  {
    if (items.Count() == 0)
      return;
    var selectedItems = items.ToList();
    var paths = items.Select(i => ((ModInfo)i).Name).ToList();
    var command = "dir";
    // 创建一个进程对象并设置参数
    var process = new Process();
    process.StartInfo.FileName = $"{AppDomain.CurrentDomain.BaseDirectory}\\OpenFolderAndSelect.exe"; // 指定要执行的程序（cmd）
    process.StartInfo.Arguments =
      $"{Path.GetDirectoryName(((ModInfo)selectedItems[0]).FullName)} \"{string.Join("\" \"", paths)}\""; // 指定要执行的命令和参数（/c 选项表示执行完命令后自动关闭 cmd 窗口）
    // 启动进程
    process.Start();
  }

  [RelayCommand]
  private void DeleteItem(IEnumerable<object> items)
  {
    if (items.Count() == 0)
      return;
    var selectedItems = items.ToList();
    var count = selectedItems.Count();
    for (var i = 0; i < count; i++)
    {
      var modInfo = (ModInfo)selectedItems[i];
      ModInfos.Remove(modInfo);
      Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(modInfo.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
    }
  }

  public async Task LoadModInfo()
  {

    T GetValue<T>(TomlTable table, string key)
    {
      if (table.TryGetValue(key, out var value) && value is T typedValue)
      {
        return typedValue;
      }
      return default(T); // Return the default value of T (e.g., null for reference types, 0 for int, false for bool)
    }

    BitmapImage LoadBitmapImage(Stream stream)
    {
      BitmapImage bitmap = new BitmapImage();
      bitmap.BeginInit();
      bitmap.CacheOption = BitmapCacheOption.OnLoad;
      bitmap.StreamSource = stream;
      bitmap.EndInit();
      //bitmap.Freeze(); // 使得BitmapImage在不同线程中可用
      return bitmap;
    }

    string ReadDescriptionByManifest(ZipArchive archive)
    {
      string manifestInfoPath = "META-INF/MANIFEST.MF";
      ZipArchiveEntry manifestEntry = archive.GetEntry(manifestInfoPath);
      if (manifestEntry != null)
      {
        // 读取文件内容
        using (StreamReader reader = new StreamReader(manifestEntry.Open()))
        {
          string line;
          while ((line = reader.ReadLine()) != null)
          {
            string[] parts = line.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
              string key = parts[0].Trim();
              if (key == "Implementation-Version")
              {
                return parts[1].Trim();
              }
            }
          }
        }
      }
      return "";
    }

    //var modMD5LogoDir = JsonSerializer.Deserialize<Dictionary<string,string>>(Settings.Default.ModMD5LogoDir);
    TaskInfoHelper.Instance.TaskInfo = "开始遍历mod文件";
    //ModInfos = null;
    ModInfos.Clear();
    var mods = Directory.GetFiles(Path.Combine(Settings.Default.GamePath, "mods"));
    //List<ModInfo> temp = [];
    foreach (var modFullName in mods)
    {
      string modName = Path.GetFileName(modFullName);
      string modVersion = "";
      string modDescription = "";
      string modLogoPath = "";
      BitmapImage? modLogo = null;
      var md5 = await CalculateFileMD5(modFullName);
      string metaInfoPath = "META-INF/mods.toml";
      
      using (ZipArchive archive = ZipFile.OpenRead(modFullName))
      {
        // 查找指定文件
        ZipArchiveEntry metaEntry = archive.GetEntry(metaInfoPath);
        if (metaEntry != null)
        {
          // 读取文件内容
          using (StreamReader reader = new StreamReader(metaEntry.Open()))
          {
            string fileContent = reader.ReadToEnd();
            TomlTable tomlTable = Toml.Parse(fileContent).ToModel();
            TomlTableArray mod = (TomlTableArray)tomlTable["mods"];
            if (mod.Count() > 0)
            {
              modName = GetValue<string>(mod[0], "displayName");
              modVersion = GetValue<string>(mod[0], "version");
              modDescription = GetValue<string>(mod[0], "description")?.Replace("  ","");
              modLogoPath = GetValue<string>(mod[0], "logoFile");
              TaskInfoHelper.Instance.TaskInfo = $"读取：{modFullName}";
            }
          }

          //可能没有version
          if (modVersion == "${file.jarVersion}")
            modVersion = ReadDescriptionByManifest(archive);

          //读图片会内存错误 不知道咋整
          //if (modLogoPath != null)
          //{
          //  ZipArchiveEntry logoEntry = archive.GetEntry(modLogoPath);
          //  if (logoEntry != null)
          //    using (var stream = logoEntry.Open())
          //    {
          //      modLogo = LoadBitmapImage(stream);
          //    }
          //}
        }
      }
      if (modName == "读取失败")
        TaskInfoHelper.Instance.TaskInfo = $"读取失败:{modFullName}";

      if (modDescription=="")
        modDescription = "该mod没有提供任何描述...";

      var modInfo = new ModInfo()
      {
        Name = modName,
        MD5 = await CalculateFileMD5(modFullName),
        Version = modVersion,
        Description = modDescription,
        Logo = modLogo,
        FullName = modFullName,
      };
      ModInfos.Add(modInfo);
    }
    TaskInfoHelper.Instance.TaskInfo = "遍历完成";
  }

  private async Task<string> CalculateFileMD5(string filename)
  {
    var result = "";
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
    //await LoadModInfo();
    foreach (var item in ModInfos!) item.Status = SynchronizationStatus.额外;

    Tip = "与服务器对比mod文件中";
    var missList = new List<ModInfo>();
    var jsonStr = await App.Current.webHelper.GetRemoteModList();
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
        var modInfo = new ModInfo()
        {
          Name = remoteModInfo.Name,
          FullName = Path.Combine(Settings.Default.GamePath, "mods", remoteModInfo.Name),
          MD5 = remoteModInfo.MD5,
          Status = SynchronizationStatus.缺少
        };
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
            await App.Current.webHelper.DownloadMod(modInfo.MD5,
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