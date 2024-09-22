using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;
using 马自达MC同步器.Resources.Enums;

namespace 马自达MC同步器.Resources.Models;

public partial class ModInfo : ObservableObject
{
  [ObservableProperty] private string? projectId;
  [ObservableProperty] private string fileName = "";
  [ObservableProperty] private string fullFileName = "";

  [ObservableProperty] private string sha1Hash = "";

  [ObservableProperty] private string displayName = "";

  [JsonIgnore]
  [ObservableProperty] private SynchronizationStatus status;

  [ObservableProperty] private string version = "";

  [ObservableProperty] private BitmapImage? logo;

  [ObservableProperty] private string description = "";

  [JsonIgnore]
  [ObservableProperty] private bool loadedCacheOrRemote;
}