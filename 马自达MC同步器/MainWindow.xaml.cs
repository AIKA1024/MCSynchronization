using System.ComponentModel;
using System.Windows;
using 马自达MC同步器.Resources.Pages;
using 马自达MC同步器.Resources.Views.Pages;

namespace 马自达MC同步器;

/// <summary>
///   MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
  public ModPage modPage;
  public ServerPage serverPage;
  public SettingPage settingPage;

  public MainWindow()
  {
    InitializeComponent();
    modPage = new ModPage();
    settingPage = new SettingPage();
    serverPage = new ServerPage();
    MainFrame.Navigate(modPage);
  }

  private void ModPageCheck(object sender, RoutedEventArgs e)
  {
    MainFrame.Navigate(modPage);
  }

  private void SettingPageCheck(object sender, RoutedEventArgs e)
  {
    MainFrame.Navigate(settingPage);
  }

  private void ServerPageCheck(object sender, RoutedEventArgs e)
  {
    MainFrame.Navigate(serverPage);
  }

  private void Window_Closing(object sender, CancelEventArgs e)
  {
    settingPage.SaveSettingToFile();
  }
}