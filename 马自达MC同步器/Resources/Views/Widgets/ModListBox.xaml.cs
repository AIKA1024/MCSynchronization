using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using 马自达MC同步器.Resources.Models;

namespace 马自达MC同步器.Resources.Views.Widgets;

/// <summary>
/// ModListBox.xaml 的交互逻辑
/// </summary>
public partial class ModListBox : UserControl
{
  public ModListBox()
  {
    InitializeComponent();
  }


  public IEnumerable<ModInfo> ItemsSource
  {
    get => (IEnumerable<ModInfo>)GetValue(ItemsSourceProperty);
    set => SetValue(ItemsSourceProperty, value);
  }

  public static readonly DependencyProperty ItemsSourceProperty =
    DependencyProperty.Register("ItemsSource", typeof(IEnumerable<ModInfo>), typeof(ModListBox),
      new PropertyMetadata(null));

  private void ModList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
  {
    object dataConext = null;
    if (e.OriginalSource is FrameworkElement)
      dataConext = ((FrameworkElement)e.OriginalSource).DataContext;
    else
      dataConext = ((FrameworkContentElement)e.OriginalSource).DataContext;

    if (ModList.SelectedItems.Contains(dataConext))
      e.Handled = true;
  }
}