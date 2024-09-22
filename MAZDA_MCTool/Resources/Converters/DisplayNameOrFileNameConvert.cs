using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace MAZDA_MCTool.Resources.Converters
{
  internal class DisplayNameOrFileNameConvert : MarkupExtension, IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (!string.IsNullOrEmpty(values[0]?.ToString()) && values[0] != DependencyProperty.UnsetValue)
      {
        return values[0];
      }
      return "正在下载：" + values[1].ToString();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }
  }
}
