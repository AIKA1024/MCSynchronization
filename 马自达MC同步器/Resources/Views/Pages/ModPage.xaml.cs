using System.Windows.Controls;
using 马自达MC同步器.Resources.ViewModels;

namespace 马自达MC同步器.Resources.Pages;

/// <summary>
///   MainPage.xaml 的交互逻辑
/// </summary>
public partial class ModPage : Page
{
  private readonly ModPageViewModel modViewModel;

  public ModPage()
  {
    modViewModel = new ModPageViewModel();
    DataContext = modViewModel;
    InitializeComponent();
  }

  // private async void Button_Click(object sender, RoutedEventArgs e)
  // {
  //   if (mainPageViewModel.FolderBrowserDialog.ShowDialog() != true)
  //     return;
  //   if (!CheckPath(mainPageViewModel.FolderBrowserDialog.FolderName))
  //   {
  //     SyButton.IsEnabled = false;
  //     MessageBox.Show("路径不正确");
  //     return;
  //   }
  //
  //   SFButton.IsEnabled = false;
  //   SetGamePath(mainPageViewModel.FolderBrowserDialog.FolderName);
  //   await TraverseMod();
  //   SyButton.IsEnabled = true;
  //   SFButton.IsEnabled = true;
  // }
}