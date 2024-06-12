using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using 马自达MC同步器.Resources.Pages;
using 马自达MC同步器.Resources.ViewModels;
using 马自达MC同步器.Resources.Views.Pages;

namespace 马自达MC同步器;

/// <summary>
///   MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
  private readonly MainWindowViewModel viewModel = new();

  public MainWindow()
  {
    viewModel.ModPage = new ModPage();
    viewModel.SettingPage = new SettingPage();
    viewModel.ServerPage = new ServerPage();
    DataContext = viewModel;
    InitializeComponent();
    MainFrame.Navigate(viewModel.ModPage);
  }

  public void SelectDirectory(object sender, ExecutedRoutedEventArgs e)
  {
    viewModel.SelectDirectory(sender, e);
  }

  private void ModPageCheck(object sender, RoutedEventArgs e)
  {
    MainFrame.Navigate(viewModel.ModPage);
  }

  private void SettingPageCheck(object sender, RoutedEventArgs e)
  {
    MainFrame.Navigate(viewModel.SettingPage);
  }

  private void ServerPageCheck(object sender, RoutedEventArgs e)
  {
    MainFrame.Navigate(viewModel.ServerPage);
  }

  private void Window_Closing(object sender, CancelEventArgs e)
  {
    ((SettingPageViewModel)viewModel.SettingPage.DataContext).SaveSettingToFile();
  }
}