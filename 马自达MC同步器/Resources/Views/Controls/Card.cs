using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using 马自达MC同步器.Resources.Enums;

namespace 马自达MC同步器.Resources.Views.Controls
{
  /// <summary>
  /// Card.xaml 的交互逻辑
  /// </summary>
  [ObservableObject]
  public partial class Card : ButtonBase
  {
    public Card()
    {

    }

    [ObservableProperty]
    public bool isExpress;
    public FrameworkElement HeaderControl
    {
      get { return (FrameworkElement)GetValue(HeaderControlProperty); }
      set { SetValue(HeaderControlProperty, value); }
    }

    public static readonly DependencyProperty HeaderControlProperty =
        DependencyProperty.Register("HeaderControl", typeof(FrameworkElement), typeof(Card), new PropertyMetadata(null));


    public string MDL2IconStr
    {
      get { return (string)GetValue(MDL2IconStrProperty); }
      set { SetValue(MDL2IconStrProperty, value); }
    }

    public static readonly DependencyProperty MDL2IconStrProperty =
        DependencyProperty.Register("MDL2IconStr", typeof(string), typeof(Card), new PropertyMetadata(null));

    public CornerRadius CornerRadius
    {
      get { return (CornerRadius)GetValue(CornerRadiusProperty); }
      set { SetValue(CornerRadiusProperty, value); }
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(Card), new PropertyMetadata(new CornerRadius()));

    //private void ResponsivePanelClicks(object sender, RoutedEventArgs e)
    //{
    //  Control control = (Control)e.OriginalSource;
    //  if (Content != null && control.Name == "panel")
    //  {
    //    IsExpress = !IsExpress;
    //    e.Handled = true;
    //  }
    //}


    protected override void OnClick()
    {
      base.OnClick();
      if (Content != null)
        IsExpress = !IsExpress;
      //创建事件携带信息（RoutedEventArgs类实例）,并和路由事件关联
      RoutedEventArgs args = new RoutedEventArgs(ClickEvent, this);
      //调用元素的RaiseEvent方法（继承自UIElement类），将事件发出去
      RaiseEvent(args);
    }

  }
}
