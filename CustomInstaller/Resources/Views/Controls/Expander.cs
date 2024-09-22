using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CustomInstaller.Resources.Views.Controls
{
  public partial class Expander : ButtonBase, INotifyPropertyChanged
  {
    public static readonly DependencyProperty HeaderControlProperty =
      DependencyProperty.Register(nameof(HeaderControl), typeof(FrameworkElement), typeof(Expander),
        new PropertyMetadata(null));

    public static readonly DependencyProperty MDL2IconStrProperty =
      DependencyProperty.Register(nameof(MDL2IconStr), typeof(string), typeof(Expander), new PropertyMetadata(null));

    public static readonly DependencyProperty CornerRadiusProperty =
      DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Expander),
        new PropertyMetadata(new CornerRadius()));

    private bool isExpress;
    public bool IsExpress
    {
      get
      {
        return isExpress;
      }
      set
      {
        isExpress = value;
        PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(IsExpress)));
      }
    }

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

    public event PropertyChangedEventHandler PropertyChanged;

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      var panelButton = (Button)GetTemplateChild("panel");
      panelButton.Click += (sender, args) =>
      {
        if (Content != null && ((Button)args.OriginalSource).Name == "panel")
          IsExpress = !IsExpress;
      };
    }
  }
}


