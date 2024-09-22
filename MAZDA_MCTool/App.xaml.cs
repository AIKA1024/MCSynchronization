using System.IO;
using System.Windows;
using WebSDK;

namespace MAZDA_MCTool;

/// <summary>
///   Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
  //public WebHelper webHelper = new(
  //  new Uri(string.IsNullOrEmpty(Settings.Default.Address)
  //    ? "https://frp-bid.top:28996"
  //    : Settings.Default.Address));

  public WebHelper webHelper = new();

#if DEBUG
  public readonly string CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
  public readonly string LogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache","Logo");
  public readonly string DownloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache", "Download");
#else
  public readonly string CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\Cache");
  public readonly string LogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\Cache","Logo");
  public readonly string DownloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\Cache", "Download");
#endif
  public string ModPath => Path.Combine(Settings.Default.GamePath, "mods");
  public new static App Current => (App)Application.Current;

  public App()
  {
    if (Settings.Default.MaxDownloadCount < 1)
      Settings.Default.MaxDownloadCount = 1;
    Settings.Default.Save();
  }
}
