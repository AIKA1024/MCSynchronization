﻿using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace MAZDA_MCTool.Resources.Converters;

internal class ByteToBoolConvert : MarkupExtension, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    var hidden = (byte)value;
    if (hidden > 0)
      return true;
    return false;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    var isCheck = (bool)value;
    return isCheck ? 1 : 0;
  }

  public override object ProvideValue(IServiceProvider serviceProvider)
  {
    return this;
  }
}