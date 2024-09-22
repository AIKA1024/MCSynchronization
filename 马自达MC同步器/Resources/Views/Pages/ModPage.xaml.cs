using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
}