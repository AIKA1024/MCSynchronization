using System.IO;
using System.Windows;
using WebSDK;

namespace 马自达MC同步器;

/// <summary>
///   Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
  public WebHelper webHelper = new(
    new Uri(string.IsNullOrEmpty(Settings.Default.Address)
      ? "https://frp-bid.top:28996"
      : Settings.Default.Address));

  public readonly string CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");

  public new static App Current => (App)Application.Current;

  public App()
  {
    if (Settings.Default.MaxDownloadCount < 1)
      Settings.Default.MaxDownloadCount = 1;
    Settings.Default.Save();
  }
}