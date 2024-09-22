using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MAZDA_MCTool.Resources.Views.Controls;

/// <summary>
///   Card.xaml 的交互逻辑
/// </summary>
[ObservableObject]
public partial class Expander : ButtonBase
{
  public static readonly DependencyProperty HeaderControlProperty =
    DependencyProperty.Register(nameof(HeaderControl), typeof(FrameworkElement), typeof(Expander),
      new PropertyMetadata(null));

  public static readonly DependencyProperty MDL2IconStrProperty =
    DependencyProperty.Register(nameof(MDL2IconStr), typeof(string), typeof(Expander), new PropertyMetadata(null));

  public static readonly DependencyProperty CornerRadiusProperty =
    DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Expander),
      new PropertyMetadata(new CornerRadius()));

  [ObservableProperty] private bool isExpanded;

  public FrameworkElement HeaderControl
  {
    get => (FrameworkElement)GetValue(HeaderControlProperty);
    set => SetValue(HeaderControlProperty, value);
  }


  public string MDL2IconStr
  {
    get => (string)GetValue(MDL2IconStrProperty);
    set => SetValue(MDL2IconStrProperty, value);
  }

  public CornerRadius CornerRadius
  {
    get => (CornerRadius)GetValue(CornerRadiusProperty);
    set => SetValue(CornerRadiusProperty, value);
  }

  //private void ResponsivePanelClicks(object sender, RoutedEventArgs e)
  //{
  //  Control control = (Control)e.OriginalSource;
  //  if (Content != null && control.Name == "panel")
  //  {
  //    IsExpanded = !IsExpanded;
  //    e.Handled = true;
  //  }
  //}

  public override void OnApplyTemplate()
  {
    base.OnApplyTemplate();

    var panelButton = (Button)GetTemplateChild("panel")!;
    panelButton.Click += (sender, args) =>
    {
      if (Content != null && ((Button)args.OriginalSource).Name == "panel")
        IsExpanded = !IsExpanded;
    };
  }
}