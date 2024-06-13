using System.Windows;
using System.Windows.Controls;
using 马自达MC同步器.Resources.Helper;
using 马自达MC同步器.Resources.ViewModels;

namespace 马自达MC同步器.Resources.Views.Pages;

/// <summary>
///   ServerAddressPage.xaml 的交互逻辑
/// </summary>
public partial class ServerPage : Page
{
  private readonly ServerPageViewModel viewModel;

  public ServerPage()
  {
    viewModel = new ServerPageViewModel(NBTHelper.GetNBTFile());
    DataContext = viewModel;
    InitializeComponent();
  }

  private void ClickNewServer(object sender, RoutedEventArgs e)
  {
    if (string.IsNullOrEmpty(Settings.Default.GamePath))
    {
      MessageBox.Show("请先选择游戏文件夹");
      return;
    }

    AddWindow.Visibility = Visibility.Visible;
  }

  private void CancelAddServer(object sender, RoutedEventArgs e)
  {
    AddWindow.Visibility = Visibility.Hidden;
  }
}