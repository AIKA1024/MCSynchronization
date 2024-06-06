using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace 马自达MC同步器.Resources.Converters
{
  internal class ByteToBoolConvert : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      byte hidden = (byte)value;
      if (hidden > 0)
        return true;
      return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      bool isCheck = (bool)value;
      return isCheck ? 1 : 0;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }
  }
}
