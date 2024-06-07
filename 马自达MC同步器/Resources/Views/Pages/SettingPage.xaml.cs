using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
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
using Velopack.Sources;
using Velopack;
using 马自达MC同步器.Resources.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace 马自达MC同步器.Resources.Pages
{
  /// <summary>
  /// SettingPage.xaml 的交互逻辑
  /// </summary>
  public partial class SettingPage : Page
  {
    SettingPageViewModel settingPageViewModel = new SettingPageViewModel()
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
      return Uri.TryCreate(urlString, UriKind.Absolute, out Uri uriResult)
          && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
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

      Settings.Default.Address = settingPageViewModel.Address;
      Settings.Default.MaxDownloadCount = settingPageViewModel.MaxDownloadCount;
      Settings.Default.Save();
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
        MessageBox.Show(ex.Message,"更新失败");
      }
    }
  }
}
