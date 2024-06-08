using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using WebSDK;
using 马自达MC同步器.Resources.Pages;

namespace 马自达MC同步器
{
  /// <summary>
  /// MainWindow.xaml 的交互逻辑
  /// </summary>
  public partial class MainWindow : Window
  {
    public ModPage modPage;
    public SettingPage settingPage;
    public ServerPage serverPage;
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

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      settingPage.SaveSettingToFile();
    }
  }
}
