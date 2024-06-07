using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace 马自达MC同步器.Resources.Converters
{
  class NullToVisiblityConverter : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return Visibility.Collapsed;
      return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }
  }
}
