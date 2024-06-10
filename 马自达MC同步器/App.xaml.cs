using System.Windows;
using WebSDK;

namespace 马自达MC同步器;

/// <summary>
///   Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
  public static WebHelper webHelper = new(
    new Uri(string.IsNullOrEmpty(Settings.Default.Address)
      ? "https://frp-bid.top:28996"
      : Settings.Default.Address));

  public App()
  {
    if (Settings.Default.MaxDownloadCount < 1)
      Settings.Default.MaxDownloadCount = 1;
    Settings.Default.Save();
  }
}