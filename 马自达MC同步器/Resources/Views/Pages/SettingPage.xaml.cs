using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Velopack;
using Velopack.Sources;
using 马自达MC同步器.Resources.ViewModels;

namespace 马自达MC同步器.Resources.Pages;

/// <summary>
///   SettingPage.xaml 的交互逻辑
/// </summary>
public partial class SettingPage : Page
{
  private readonly SettingPageViewModel settingPageViewModel = new()
  {
    Version = Application.ResourceAssembly.GetName().Version?.ToString(),
    Address = Settings.Default.Address,
    MaxDownloadCount = Settings.Default.MaxDownloadCount,
    GamePath = Settings.Default.GamePath
  };

  public SettingPage()
  {
    InitializeComponent();
    DataContext = settingPageViewModel;
  }

  private bool IsValidUrl(string urlString)
  {
    return Uri.TryCreate(urlString, UriKind.Absolute, out var uriResult)
           && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
  }

  public void SaveSettingToFile()
  {
    Settings.Default.Address = settingPageViewModel.Address;
    Settings.Default.MaxDownloadCount = settingPageViewModel.MaxDownloadCount;
    Settings.Default.Save();
  }

  private void Button_Click(object sender, RoutedEventArgs e)
  {
    if (!IsValidUrl(AddressTextBox.Text))
    {
      MessageBox.Show("无效的地址");
      return;
    }

    //if (!int.TryParse(MaxDounloadTextBox.Text, out int result) || result < 1)
    //{
    //  MessageBox.Show("无效的线程限制");
    //  return;
    //}

    SaveSettingToFile();
  }

  private async void UpdateBT_Click(object sender, RoutedEventArgs e)
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

  private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
  {
    Process.Start(new ProcessStartInfo("explorer.exe", e.Uri.AbsoluteUri));
    e.Handled = true;
  }


  private void AddressTextBox_LostFocus(object sender, RoutedEventArgs e)
  {
    if (AddressTextBox.Text == settingPageViewModel.Address)
      return;

    var textBox = (TextBox)sender;
    if (!IsValidUrl(AddressTextBox.Text))
    {
      MessageBox.Show("无效的URL");
      AddressTextBox.Text = settingPageViewModel.Address;
      return;
    }

    Settings.Default.Address = textBox.Text; //绑定的更新比这个慢
    //App.webHelper.ChangeBaseAddress(new Uri(Settings.Default.Address));
    WarningTextBlock.Visibility = Visibility.Visible;
    MessageBox.Show("重启后生效");
  }
}