using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic.FileIO;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
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
    var modinfo = await LoadMod(e.FullPath);
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
    CopyPropertiesTo(await LoadMod(e.FullPath), modinfo);
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
      ModInfo? modInfo = await LoadMod(modFullName);
      if (modInfo != null)
        ModInfos.Add(modInfo);
    }

    TaskInfoHelper.Instance.TaskInfo = "遍历完成";
  }

  private async Task<ModInfo?> LoadMod(string modFullFileName)
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
      var modLogoPath = "";
      BitmapImage? modLogo = null;
      var md5 = await FileHashHelper.ComputeMd5HashAsync(modFullFileName);
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
              modLogoPath = GetValue<string>(mod[0], "logoFile");
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

      if (string.IsNullOrWhiteSpace(modDescription) || modDescription == "")
        modDescription = "该mod没有提供任何描述...";

      if (string.IsNullOrWhiteSpace(modVersion) || modVersion == "")
        modVersion = "无法获取版本号";

      var modInfo = new ModInfo()
      {
        DisplayName = modDisplayName,
        FileName = Path.GetFileName(modFullFileName),
        Sha1Hash = await FileHashHelper.ComputeSha1HashForFileAsync(modFullFileName),
        Version = modVersion,
        Description = modDescription,
        Logo = modLogo,
        FullFileName = modFullFileName
      };
      return modInfo;
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
      return null;
    }
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
    var jsonStr = await App.Current.webHelper.GetRemoteModList(uri);
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