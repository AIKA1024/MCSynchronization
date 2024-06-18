using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic.FileIO;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Policy;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Media.Imaging;
using Tomlyn;
using Tomlyn.Model;
using 马自达MC同步器.Resources.Commands;
using 马自达MC同步器.Resources.Enums;
using 马自达MC同步器.Resources.Helper;
using 马自达MC同步器.Resources.Models;
using 马自达MC同步器.Resources.MyEventArgs;

namespace 马自达MC同步器.Resources.ViewModels;

public partial class ModPageViewModel : ObservableObject
{
  private static FileSystemWatcher fileSystemWatcher;
  [ObservableProperty] private string tip = "";

  public ObservableCollection<ModInfo> ModInfos { get; set; } = [];

  public ModPageViewModel()
  {
    if (fileSystemWatcher == null)
    {
      fileSystemWatcher = new FileSystemWatcher
      {
        Path = Path.Combine(Settings.Default.GamePath, "mods"),
        NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
        Filter = "*.*",
        IncludeSubdirectories = false,
        EnableRaisingEvents = true
      };
      fileSystemWatcher.Created += OnCreated;
      fileSystemWatcher.Changed += OnChanged;
      fileSystemWatcher.Deleted += OnDeleted;
      App.Current.Exit += (sender, e) => fileSystemWatcher.Dispose();
    }
    SelectDirectory.Instance.PathChanged += GamePathChanged;
  }
  private async void GamePathChanged(object sender, GamePathChangedEventArgs e)
  {
    ModInfos.Clear();
    fileSystemWatcher.Path = e.NewPath;
    await LoadAllModInfo();
  }

  #region ModFileChangeHandler

  private async void OnCreated(object sender, FileSystemEventArgs e)
  {
    FileInfo file = new FileInfo(e.FullPath);
    if (file.Length < 1024)
      return;
    var modinfo = await LoadModZip(e.FullPath);
    if (modinfo == null)
      return;
    int index = 0;
    for (int i = 0; i < ModInfos.Count; i++)
    {
      if (string.Compare(modinfo.DisplayName, ModInfos[i].DisplayName) > 0)
      {
        index = i;
        break;
      }
    }
    App.Current.Dispatcher.Invoke(() =>
    {
      ModInfos.Insert(index, modinfo);
    });
  }

  private async void OnChanged(object sender, FileSystemEventArgs e)
  {
    var modinfo = ModInfos.FirstOrDefault(m => m.FileName == e.Name);
    if (modinfo == null) return;
    CopyPropertiesTo(await LoadModZip(e.FullPath), modinfo);
  }

  private void OnDeleted(object sender, FileSystemEventArgs e)
  {
    for (int i = 0; i < ModInfos.Count; i++)
    {
      if (ModInfos[i].FullFileName == e.FullPath)
      {
        App.Current.Dispatcher.Invoke(() => ModInfos.RemoveAt(i));
        return;
      }
    }
  }

  #endregion

  [RelayCommand]
  private void OpenItemToExplorer(IEnumerable<object> items)
  {
    if (!items.Any())
      return;
    var selectedItems = items.ToList();
    var paths = items.Where(i => !string.IsNullOrEmpty(((ModInfo)i).DisplayName))
      .Select(i => Path.GetFileName(((ModInfo)i).FullFileName)).ToList();
    // 创建一个进程对象并设置参数
    var process = new Process();
    process.StartInfo.FileName = $"{AppDomain.CurrentDomain.BaseDirectory}\\OpenFolderAndSelect.exe";
    process.StartInfo.Arguments =
      $"{Path.GetDirectoryName(((ModInfo)selectedItems[0]).FullFileName)} \"{string.Join("\" \"", paths)}\""; // 指定要执行的命令和参数（/c 选项表示执行完命令后自动关闭 cmd 窗口）
    // 启动进程
    process.Start();
  }

  [RelayCommand]
  private void DeleteItem(IEnumerable<object> items)
  {
    if (!items.Any())
      return;
    var selectedItems = items.Where(i => !string.IsNullOrEmpty(((ModInfo)i).DisplayName)).ToList();
    var count = selectedItems.Count;
    for (var i = 0; i < count; i++)
    {
      var modInfo = (ModInfo)selectedItems[i];
      //ModInfos.Remove(modInfo); //统一用fileSystemWatcher的删除事件
      Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(modInfo.FullFileName, UIOption.OnlyErrorDialogs,
        RecycleOption.SendToRecycleBin);
    }
  }



  public async Task LoadAllModInfo()
  {
    TaskInfoHelper.Instance.TaskInfo = "开始遍历mod文件";
    ModInfos.Clear();
    var mods = Directory.GetFiles(Path.Combine(Settings.Default.GamePath, "mods"));
    foreach (var modFullName in mods)
    {
      ModInfo? modInfo = await LoadModZip(modFullName);
      if (modInfo != null)
      {
        await LoadModCache(modInfo);
        ModInfos.Add(modInfo);
      }
    }
    TaskInfoHelper.Instance.TaskInfo = "联网查询mod信息..";
    await GetModInfoFromModrinth(ModInfos.Where(m => !m.LoadedCacheOrRemote).ToList());
    TaskInfoHelper.Instance.TaskInfo = "遍历完成";
  }

  private async Task GetModInfoFromModrinth(List<ModInfo> ModList)
  {
    var modrinthModFileInfos = (await App.Current.webHelper.GetVersionListFromHashasync(
      ModList.Select(m => m.Sha1Hash.ToLower()).ToList()));

    var modrinthProjectsArray = await App.Current.webHelper.GetProjectListFromID(
      modrinthModFileInfos?.Select(m => m.Value["project_id"].ToString()).ToList());

    if (modrinthModFileInfos == null || modrinthProjectsArray == null)
    {
      TaskInfoHelper.Instance.TaskInfo = "联网查询失败,可能modrinth还没收录安装的部分mod";
      await Task.Delay(1000);
      return;
    }

    Dictionary<string, JsonObject> modrinthProjectsDir = modrinthProjectsArray
      .OfType<JsonObject>()
      .ToDictionary(
          m => m["id"].ToString(),
          m => m
      );

    //todo 多线程加载

    foreach (var modinfo in ModList)
    {
      if (!modrinthModFileInfos.ContainsKey(modinfo.Sha1Hash))
        continue;

      var modVersionJObj = modrinthModFileInfos[modinfo.Sha1Hash];
      var modProjectId = modVersionJObj["project_id"].ToString();
      var projectJObj = modrinthProjectsDir[modProjectId];

      var cacheFilePath = Path.Combine(App.Current.CachePath, modinfo.Sha1Hash + ".json");
      var cacheHelper = new CacheHelper();
      
      File.Create(cacheFilePath).Dispose();
      var logoUrl = projectJObj["icon_url"]?.ToString();
      if (logoUrl != null)
      {
        var logoFilePath = Path.Combine(App.Current.LogoPath, modinfo.Sha1Hash + ".png");
        await App.Current.webHelper.DownloadImageAsync(projectJObj["icon_url"].ToString(), logoFilePath);
        projectJObj["icon_url"] = logoFilePath;
        projectJObj["body"] = null;//不需要的信息
        modVersionJObj["changelog"] = null;

        var modLogo = new BitmapImage();
        modLogo.BeginInit();
        modLogo.CacheOption = BitmapCacheOption.OnLoad; // 确保图像被完全加载到内存中
        modLogo.UriSource = new Uri(logoFilePath);
        modLogo.DecodePixelWidth = 34;
        modLogo.DecodePixelHeight = 34;
        modLogo.EndInit();
        modLogo.Freeze();
        modinfo.Logo = modLogo;
      }
      using (var writer = new StreamWriter(cacheFilePath))
      {
        JsonObject versionAndProjectFrom = new()
        {
          { nameof(modVersionJObj), modVersionJObj.DeepClone() },
          { nameof(projectJObj), projectJObj.DeepClone() }
        };
        await writer.WriteAsync(JsonSerializer.Serialize(versionAndProjectFrom));
      }
    }
  }

  private async Task<ModInfo?> LoadModZip(string modFullFileName)
  {
    try
    {
      byte[] header = new byte[4];
      using (FileStream fs = new FileStream(modFullFileName, FileMode.Open, FileAccess.Read))
      {
        fs.Read(header, 0, header.Length);
      }
      if (!(header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04))
        return null;
      var modFileName = Path.GetFileName(modFullFileName);
      var modDisplayName = modFileName;//初始值，获取不到时使用文件名
      var modVersion = "";
      var modDescription = "";
      var metaInfoPath = "META-INF/mods.toml";

      using (var archive = ZipFile.OpenRead(modFullFileName))
      {
        // 查找指定文件
        var metaEntry = archive.GetEntry(metaInfoPath);
        if (metaEntry != null)
        {
          // 读取文件内容
          using (var reader = new StreamReader(metaEntry.Open()))
          {
            var fileContent = reader.ReadToEnd();
            var tomlTable = Toml.Parse(fileContent).ToModel();
            var mod = (TomlTableArray)tomlTable["mods"];
            if (mod.Count > 0)
            {
              modDisplayName = GetValue<string>(mod[0], "displayName");
              modVersion = GetValue<string>(mod[0], "version");
              modDescription = GetValue<string>(mod[0], "description")?.Replace("  ", "");
              TaskInfoHelper.Instance.TaskInfo = $"读取：{modFileName}";
            }
          }
          //可能没有version
          if (modVersion == "${file.jarVersion}")
            modVersion = ReadDescriptionByManifest(archive);
        }
      }

      if (modDisplayName == modFileName)
      {
        modDisplayName = Path.GetFileNameWithoutExtension(modDisplayName);
        TaskInfoHelper.Instance.TaskInfo = $"读取配置失败:{modFileName}";
      }

      var modInfo = new ModInfo()
      {
        DisplayName = modDisplayName,
        FileName = Path.GetFileName(modFullFileName),
        Sha1Hash = (await FileHashHelper.ComputeSha1HashForFileAsync(modFullFileName)).ToLower(),
        Version = modVersion,
        Description = modDescription,
        FullFileName = modFullFileName,
      };

      return modInfo;
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
      return null;
    }
  }

  public async Task LoadModCache(ModInfo modInfo)
  {
    await Task.Run(() =>
    {
      string? modVersion = null;
      string? modDescription = null;
      JsonObject? VersionAndProjectJObj = null;
      var cacheFilePath = Path.Combine(App.Current.CachePath, modInfo.Sha1Hash + ".json");
      var cacheHelper = new CacheHelper();
      var loadedCacheOrRemote = false;
      if (cacheHelper.HasModCache(modInfo.Sha1Hash))
      {
        VersionAndProjectJObj = cacheHelper.GetModCache(modInfo.Sha1Hash);
        loadedCacheOrRemote = true;
      }

      if (string.IsNullOrEmpty(modInfo.Description))
      {
        var remoteDiscription = VersionAndProjectJObj?["projectJObj"]["description"].ToString();
        modDescription = string.IsNullOrEmpty(remoteDiscription) ? "该mod没有提供任何描述..." : remoteDiscription;
      }
      if (string.IsNullOrEmpty(modInfo.Version))
      {
        var remoteVersion = VersionAndProjectJObj?["modVersionJObj"]["version_number"].ToString();
        modVersion = string.IsNullOrEmpty(remoteVersion) ? "无法获取版本号..." : remoteVersion;
      }

      BitmapImage? modLogo = null;
      var url = VersionAndProjectJObj?["projectJObj"]["icon_url"]?.ToString();
      if (!string.IsNullOrEmpty(url))
      {
        modLogo = new BitmapImage();
        modLogo.BeginInit();
        modLogo.CacheOption = BitmapCacheOption.OnLoad; // 确保图像被完全加载到内存中
        modLogo.UriSource = new Uri(url);
        modLogo.DecodePixelWidth = 34;
        modLogo.DecodePixelHeight = 34;
        modLogo.EndInit();
        modLogo.Freeze();
      }
      modInfo.Description = modDescription?? modInfo.Description;
      modInfo.Version = modVersion ?? modInfo.Version;
      modInfo.Logo = modLogo;
      modInfo.LoadedCacheOrRemote = loadedCacheOrRemote;
    });
  }

  T GetValue<T>(TomlTable table, string key)
  {
    if (table.TryGetValue(key, out var value) && value is T typedValue) return typedValue;
    return default; // Return the default value of T (e.g., null for reference types, 0 for int, false for bool)
  }

  BitmapImage LoadBitmapImage(Stream stream)
  {
    var bitmap = new BitmapImage();
    bitmap.BeginInit();
    bitmap.CacheOption = BitmapCacheOption.OnLoad;
    bitmap.StreamSource = stream;
    bitmap.EndInit();
    //bitmap.Freeze(); // 使得BitmapImage在不同线程中可用
    return bitmap;
  }

  string ReadDescriptionByManifest(ZipArchive archive)
  {
    var manifestInfoPath = "META-INF/MANIFEST.MF";
    var manifestEntry = archive.GetEntry(manifestInfoPath);
    if (manifestEntry != null)
      // 读取文件内容
      using (var reader = new StreamReader(manifestEntry.Open()))
      {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
          string[] parts = line.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
          if (parts.Length == 2)
          {
            var key = parts[0].Trim();
            if (key == "Implementation-Version") return parts[1].Trim();
          }
        }
      }

    return "";
  }

  [RelayCommand]
  public async Task Synchronization()
  {
    var uri = Settings.Default.Address + "/GetModList";
    //await LoadModInfo();
    foreach (var item in ModInfos!) item.Status = SynchronizationStatus.额外;

    Tip = "与服务器对比mod文件中";
    var missList = new List<ModInfo>();
    var jsonStr = await App.Current.webHelper.GetAsync(uri);
    if (string.IsNullOrEmpty(jsonStr))
    {
      MessageBox.Show("与服务器链接失败");
      Tip = "与服务器链接失败";
      return;
    }

    var RemoteModList = JsonSerializer.Deserialize<List<ModInfo>>(jsonStr);

    if (RemoteModList == null || RemoteModList.Count == 0)
    {
      MessageBox.Show("服务器返回了空的模组列表");
      Tip = "服务器返回了空的模组列表";
      return;
    }


    // 设置最大并发下载任务数量
    SemaphoreSlim semaphore = new(Settings.Default.MaxDownloadCount);

    foreach (var remoteModInfo in RemoteModList)
    {
      var found = false;
      foreach (var localModInfo in ModInfos)
        if (remoteModInfo.Sha1Hash == localModInfo.Sha1Hash)
        {
          localModInfo.Status = SynchronizationStatus.已同步;
          found = true;
          break;
        }

      if (!found)
      {
        var modInfo = new ModInfo()
        {
          FileName = remoteModInfo.FileName,
          FullFileName = Path.Combine(App.Current.ModPath, remoteModInfo.FileName),
          Sha1Hash = remoteModInfo.Sha1Hash,
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

    for (int i = 0; i < missList.Count; i++)
    {
      int index = i;
      if (missList[index].Status != SynchronizationStatus.已同步)
      {
        await semaphore.WaitAsync(); // 等待信号量，限制并发数量
        downloadTasks.Add(Task.Run(async () =>
        {
          try
          {
            missList[index].Status = SynchronizationStatus.下载中;
            await App.Current.webHelper.DownloadMod($"{Settings.Default.Address}/Download", missList[index].Sha1Hash,
              App.Current.ModPath);
            //CopyPropertiesTo(await LoadMod(missList[index].FullFileName), missList[index]);
            missList[index].Status = SynchronizationStatus.已同步;
          }
          catch (Exception ex)
          {
            missList[index].Status = SynchronizationStatus.下载失败;
            Debug.WriteLine($"下载模组失败：{ex.Message}");
          }
          finally
          {
            semaphore.Release(); // 释放信号量
          }
        }));
      }
    }


    await Task.WhenAll(downloadTasks);
    //ModInfos.OrderBy(mod => mod.DisplayName);
    Tip = "完成";
  }
  private void SortByDisPlayName(ObservableCollection<ModInfo> modInfos)
  {
    for (int i = 0; i < modInfos.Count; i++)
    {

    }
  }
  public void CopyPropertiesTo<T>(T source, T dest)
  {
    var properties = typeof(T).GetProperties();

    foreach (var property in properties)
    {
      if (property.CanRead && property.CanWrite)
      {
        var value = property.GetValue(source);
        property.SetValue(dest, value);
      }
    }
  }
}