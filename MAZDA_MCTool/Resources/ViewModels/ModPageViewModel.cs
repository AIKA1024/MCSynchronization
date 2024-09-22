using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualBasic.FileIO;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Media.Imaging;
using Tomlyn;
using Tomlyn.Model;
using MAZDA_MCTool.Resources.Commands;
using MAZDA_MCTool.Resources.Enums;
using MAZDA_MCTool.Resources.Helper;
using MAZDA_MCTool.Resources.Messages;
using MAZDA_MCTool.Resources.Models;
using MAZDA_MCTool.Resources.MyEventArgs;

namespace MAZDA_MCTool.Resources.ViewModels;

public partial class ModPageViewModel : ObservableObject
{
  private FileSystemWatcher fileSystemWatcher;
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
    var modInfo = await LoadModZip(e.FullPath);

    if (modInfo == null)
      return;
    int index = 0;
    for (int i = 0; i < ModInfos.Count; i++)
    {
      if (string.Compare(modInfo.DisplayName, ModInfos[i].DisplayName) > 0)
      {
        index = i;
        break;
      }
    }

    App.Current.Dispatcher.Invoke(() =>
    {
      string logoPath = $"{Path.Combine(App.Current.LogoPath, modInfo.Sha1Hash)}.png";
      if (File.Exists(logoPath))
        modInfo.Logo = new BitmapImage(new Uri(logoPath));
      ModInfos.Insert(index, modInfo);
      WeakReferenceMessenger.Default.Send(new ModInfoMessage(modInfo));
    });
  }

  private async void OnChanged(object sender, FileSystemEventArgs e)
  {
    var modInfo = ModInfos.FirstOrDefault(m => m.FileName == e.Name);
    if (modInfo == null) return;
    CopyPropertiesTo(await LoadModZip(e.FullPath), modInfo);
    if (!modInfo.LoadedCacheOrRemote)
      await LoadModCache(modInfo);
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
    var selectedItems = items.ToList();
    if (!selectedItems.Any())
      return;

    var paths = selectedItems.Select(i => Path.GetFileName(((ModInfo)i).FullFileName)).ToList();
    var process = new Process();
    process.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpenFolderAndSelect.exe");
    process.StartInfo.Arguments =
      $"\"{Path.GetDirectoryName(((ModInfo)selectedItems[0]).FullFileName)}\" \"{string.Join("\" \"", paths)}\""; // 指定要执行的命令和参数（/c 选项表示执行完命令后自动关闭 cmd 窗口）
    process.Start();
  }

  [RelayCommand]
  private void DeleteItem(IEnumerable<object> items)
  {
    var enumerable = items as List<object> ?? items.ToList();
    if (!enumerable.Any())
      return;
    var selectedItems = enumerable.Where(i => !string.IsNullOrEmpty(((ModInfo)i).DisplayName)).ToList();
    var count = selectedItems.Count;
    for (var i = 0; i < count; i++)
    {
      var modInfo = (ModInfo)selectedItems[i];
      //ModInfos.Remove(modInfo); //统一用fileSystemWatcher的删除事件
      FileSystem.DeleteFile(modInfo.FullFileName, UIOption.OnlyErrorDialogs,
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

#if !OFFLINEDEBUG
    TaskInfoHelper.Instance.TaskInfo = "联网查询mod信息..";
    await GetModInfoFromModrinth(ModInfos.Where(
       m => !m.LoadedCacheOrRemote).ToList(),
       new SemaphoreSlim(10));
    TaskInfoHelper.Instance.TaskInfo = "遍历完成";
#endif
  }

  private async Task GetModInfoFromModrinth(List<ModInfo> modList, SemaphoreSlim semaphore)
  {
    var modrinthModFileInfos = await App.Current.webHelper.GetVersionListFromHashasync(
      modList.Select(m => m.Sha1Hash.ToLower()).ToList());

    var modrinthProjectsArray = await App.Current.webHelper.GetProjectListFromID(
      modrinthModFileInfos?.Select(m => m.Value["project_id"].ToString()).ToList());

    if (modrinthModFileInfos == null || modrinthProjectsArray == null)
    {
      TaskInfoHelper.Instance.TaskInfo = $"共有{modList.Count}个mod联网查询失败,可能modrinth还没收录安装的部分mod";
      await Task.Delay(1000);
      return;
    }

    Dictionary<string, JsonObject> modrinthProjectsDir = modrinthProjectsArray
      .OfType<JsonObject>()
      .ToDictionary(
        m => m["id"].ToString(),
        m => m
      );

    List<Task> loadTasks = [];

    foreach (var modInfo in modList)
    {
      if (!modrinthModFileInfos.ContainsKey(modInfo.Sha1Hash))
        continue;

      await semaphore.WaitAsync();
      loadTasks.Add(Task.Run(async () =>
      {
        try
        {
          var modVersionJObj = modrinthModFileInfos[modInfo.Sha1Hash];
          var modProjectId = modVersionJObj["project_id"].ToString();
          var projectJObj = modrinthProjectsDir[modProjectId];

          var cacheFilePath = Path.Combine(App.Current.CachePath, modInfo.Sha1Hash + ".json");
          var cacheHelper = new CacheHelper();

          await File.Create(cacheFilePath).DisposeAsync();
          var logoUrl = projectJObj["icon_url"]?.ToString();
          if (logoUrl != null)
          {
            var logoFilePath = Path.Combine(App.Current.LogoPath, modInfo.Sha1Hash + ".png");
            await App.Current.webHelper.DownloadImageAsync(projectJObj["icon_url"].ToString(), logoFilePath, 3);
            projectJObj["icon_url"] = logoFilePath;
            projectJObj["body"] = null; //不需要的信息
            modVersionJObj["changelog"] = null;

            var modLogo = new BitmapImage();
            modLogo.BeginInit();
            modLogo.CacheOption = BitmapCacheOption.OnLoad; // 确保图像被完全加载到内存中
            modLogo.UriSource = new Uri(logoFilePath);
            modLogo.DecodePixelWidth = 34;
            modLogo.DecodePixelHeight = 34;
            modLogo.EndInit();
            modLogo.Freeze();
            modInfo.Logo = modLogo;
            modInfo.ProjectId = modProjectId;
            modInfo.LoadedCacheOrRemote = true;
          }

          using (var writer = new StreamWriter(cacheFilePath))
          {
            var versionAndProjectFrom = new
            {
              modVersionJObj,
              projectJObj
            };
            await writer.WriteAsync(JsonSerializer.Serialize(versionAndProjectFrom));
          }
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.Message);
          throw;
        }
        finally
        {
          semaphore.Release();
        }
      }));
    }
    await Task.WhenAll(loadTasks);
  }

  private async Task<ModInfo> LoadModZip(string modFullFileName)
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
      var modDisplayName = modFileName; //初始值，获取不到时使用文件名
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
              modDescription = GetValue<string>(mod[0], "description")?.TrimStart(' ', '\n');
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
      JsonObject? versionAndProjectJObj = null;
      var cacheFilePath = Path.Combine(App.Current.CachePath, modInfo.Sha1Hash + ".json");
      var cacheHelper = new CacheHelper();
      var loadedCacheOrRemote = false;
      if (cacheHelper.HasModCache(modInfo.Sha1Hash))
      {
        versionAndProjectJObj = cacheHelper.GetModCache(modInfo.Sha1Hash);
        loadedCacheOrRemote = true;
      }

      if (string.IsNullOrEmpty(modInfo.Description))
      {
        var remoteDescription = versionAndProjectJObj?["projectJObj"]["description"].ToString();
        modDescription = string.IsNullOrEmpty(remoteDescription) ? "该mod没有提供任何描述..." : remoteDescription;
      }

      if (string.IsNullOrEmpty(modInfo.Version))
      {
        var remoteVersion = versionAndProjectJObj?["modVersionJObj"]["version_number"].ToString();
        modVersion = string.IsNullOrEmpty(remoteVersion) ? "无法获取版本号..." : remoteVersion;
      }

      BitmapImage? modLogo = null;
      var url = versionAndProjectJObj?["projectJObj"]["icon_url"]?.ToString();
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

      modInfo.Description = modDescription ?? modInfo.Description;
      modInfo.Version = modVersion ?? modInfo.Version;
      modInfo.Logo = modLogo;
      modInfo.ProjectId = versionAndProjectJObj?["projectJObj"]["id"]?.ToString();
      modInfo.LoadedCacheOrRemote = loadedCacheOrRemote;
    });
  }

  T GetValue<T>(TomlTable table, string key)
  {
    if (table.TryGetValue(key, out var value) && value is T typedValue) return typedValue;
    return default; // Return the default value of T (e.g., null for reference types, 0 for int, false for bool)
  }

  string ReadDescriptionByManifest(ZipArchive archive)
  {
    var manifestInfoPath = "META-INF/MANIFEST.MF";
    var manifestEntry = archive.GetEntry(manifestInfoPath);
    if (manifestEntry != null)
      // 读取文件内容
      using (var reader = new StreamReader(manifestEntry.Open()))
      {
        while (reader.ReadLine() is { } line)
        {
          string[] parts = line.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
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

    var remoteModList = JsonSerializer.Deserialize<List<ModInfo>>(jsonStr);

    if (remoteModList == null || remoteModList.Count == 0)
    {
      MessageBox.Show("服务器返回了空的模组列表");
      Tip = "服务器返回了空的模组列表";
      return;
    }

    foreach (var remoteModInfo in remoteModList)
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
        WeakReferenceMessenger.Default.Send(new ModInfoMessage(modInfo));
      }
    }

    await DownloadMissingMods(missList);
  }

  private async Task DownloadMissingMods(List<ModInfo> missList)
  {
    // 设置最大并发下载任务数量
    SemaphoreSlim friendsSemaphore = new(Settings.Default.MaxDownloadCount);
    SemaphoreSlim modrinthSemaphore = new(6);
    Tip = "下载所需mod中";
    List<Task> downloadTasks = [];
    //var missModHashList = missList.Select(m => m.Sha1Hash).ToList();
    var downLoadedList = new List<ModInfo>();
    var leftModList = new List<ModInfo>();
    foreach (var item in missList)
    {
      item.Status = SynchronizationStatus.下载中;
      downloadTasks.Add(Task.Run(async () =>
      {
        await modrinthSemaphore.WaitAsync();
        if (await App.Current.webHelper.DownLoadModFromModrinth(item.Sha1Hash, App.Current.ModPath))
          downLoadedList.Add(item);
        else
          leftModList.Add(item);
        modrinthSemaphore.Release();
      }));
    }
    await Task.WhenAll(downloadTasks);
    downloadTasks.Clear();
    //await App.Current.webHelper.DownLoadModFromModrinth(missModHashList,App.Current.ModPath);

    for (int i = 0; i < leftModList.Count; i++)
    {
      int index = i;
      if (leftModList[index].Status != SynchronizationStatus.已同步)
      {
        await friendsSemaphore.WaitAsync(); // 等待信号量，限制并发数量
        downloadTasks.Add(Task.Run(async () =>
        {
          try
          {
            leftModList[index].Status = SynchronizationStatus.下载中;
            await App.Current.webHelper.DownloadModFromFriends($"{Settings.Default.Address}/Download",
              leftModList[index].Sha1Hash,
              App.Current.ModPath);
            leftModList[index].Status = SynchronizationStatus.已同步;
          }
          catch (Exception ex)
          {
            leftModList[index].Status = SynchronizationStatus.下载失败;
            Debug.WriteLine($"下载模组失败：{ex.Message}");
          }
          finally
          {
            friendsSemaphore.Release(); // 释放信号量
          }
        }));
      }
    }


    await Task.WhenAll(downloadTasks);
    //加载缓存信息或网络信息
    var cacheHelper = new CacheHelper();
    var noCacheModList = new List<ModInfo>();
    var hasCacheModList = new List<ModInfo>();
    for (int i = 0; i < downLoadedList.Count; i++)
    {
      if (cacheHelper.HasModCache(downLoadedList[i].Sha1Hash))
        hasCacheModList.Add(downLoadedList[i]);
      else
        noCacheModList.Add(downLoadedList[i]);
    }

    if (noCacheModList.Count > 0)
      await GetModInfoFromModrinth(noCacheModList, new SemaphoreSlim(10));
    foreach (var modInfo in hasCacheModList)
    {
      await LoadModCache(modInfo);
    }
    ModInfos.OrderBy(mod => mod.DisplayName);
    Tip = "完成";
  }

  public void CopyPropertiesTo<T>(T source, T dest)
  {
    var properties = typeof(T).GetProperties();

    foreach (var property in properties)
    {
      if (property is { CanRead: true, CanWrite: true })
      {
        var value = property.GetValue(source);
        property.SetValue(dest, value);
      }
    }
  }
}