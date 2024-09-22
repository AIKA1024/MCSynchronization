using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace MAZDA_MCTool.Resources.Views.Controls
{
  internal class IconButton : ButtonBase
  {


    public CornerRadius CornerRadius
    {
      get => (CornerRadius)GetValue(CornerRadiusProperty);
      set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
    DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(IconButton),
      new PropertyMetadata(new CornerRadius()));

  }
}
