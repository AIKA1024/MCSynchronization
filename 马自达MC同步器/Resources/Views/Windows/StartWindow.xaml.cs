using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using 马自达MC同步器.Resources.Commands;
using 马自达MC同步器.Resources.Pages;
using 马自达MC同步器.Resources.ViewModels;
using 马自达MC同步器.Resources.Views.Pages;

namespace 马自达MC同步器.Resources.Views.Windows;

/// <summary>
/// StartWindow.xaml 的交互逻辑
/// </summary>
public partial class StartWindow : Window
{
  public StartWindow()
  {
    InitializeComponent();
  }
  public MainWindow? MainWindow { get; set; }
  private async Task InitialMainWindow()
  {
    var viewModel = new MainWindowViewModel()
    {
      ModPage = new ModPage(),
      SettingPage = new SettingPage(),
      ServerPage = new ServerPage()
    };
    await((ModPageViewModel)viewModel.ModPage.DataContext).LoadAllModInfo();
    MainWindow = new MainWindow(viewModel);
    Close();
  }
  private async void Window_Initialized(object sender, EventArgs e)
  {
    Directory.CreateDirectory(App.Current.LogoPath);

    if (string.IsNullOrEmpty(Settings.Default.GamePath))
    {
      Mask.Visibility = Visibility.Visible;
      return;
    }

    await InitialMainWindow();
  }

  private async void Button_Click(object sender, RoutedEventArgs e)
  {
    SelectDirectory.Instance.Execute(null);

    if (!string.IsNullOrEmpty(Settings.Default.GamePath))
    {
      Mask.Visibility = Visibility.Collapsed;
      await InitialMainWindow();
    }
  }

  private void ExitBT_Click(object sender, RoutedEventArgs e)
  {
      App.Current.Shutdown();
  }
}