using System.ComponentModel.DataAnnotations;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Velopack;
using Velopack.Sources;
using 马自达MC同步器.Resources.Attributes;

namespace 马自达MC同步器.Resources.ViewModels;

public partial class SettingPageViewModel : ObservableValidator
{
  public SettingPageViewModel()
  {
    ValidateAllProperties();
  }

  [ObservableProperty] private Visibility addressWarningVisblity = Visibility.Collapsed;

  private string? address = Settings.Default.Address;

  [Attributes.Url(nameof(Address))]
  public string? Address
  {
    get => address;
    set
    {
      SetProperty(ref address, value, true);
      ValidateProperty(value);
      AddressWarningVisblity = Visibility.Visible;
    }
  }

  [Range(1, 16)] [ObservableProperty] public int maxDownloadCount = Settings.Default.MaxDownloadCount;

  [ObservableProperty] public string? version = Application.ResourceAssembly.GetName().Version?.ToString();

  public void SaveSettingToFile()
  {
    Settings.Default.Address = Address;
    Settings.Default.MaxDownloadCount = MaxDownloadCount;
    Settings.Default.Save();
  }

  [RelayCommand]
  private async Task Update()
  {
    try
    {
      var mgr = new UpdateManager(new GithubSource("https://github.com/AIKA1024/MCSynchronization", null, false));

      // check for new version
      var newVersion = await mgr.CheckForUpdatesAsync();
      if (newVersion == null)
      {
        MessageBox.Show("当前已经是最新版");
        return; // no update available
      }

      // download new version
      await mgr.DownloadUpdatesAsync(newVersion);

      // install new version and restart app
      mgr.ApplyUpdatesAndRestart(newVersion);
    }
    catch (Exception ex)
    {
      MessageBox.Show(ex.Message, "更新失败");
    }
  }
}