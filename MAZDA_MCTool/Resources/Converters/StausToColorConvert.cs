using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using MAZDA_MCTool.Resources.Enums;

namespace MAZDA_MCTool.Resources.Converters;

internal class StausToColorConvert : MarkupExtension, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    var status = (SynchronizationStatus)value;
    switch (status)
    {
      case SynchronizationStatus.未同步:
        return new SolidColorBrush(Color.FromRgb(0, 0, 0));
      case SynchronizationStatus.已同步:
        return new SolidColorBrush(Color.FromRgb(66, 185, 131));
      case SynchronizationStatus.缺少:
        return new SolidColorBrush(Color.FromRgb(220, 20, 60));
      case SynchronizationStatus.额外:
        return new SolidColorBrush(Color.FromRgb(143, 129, 94));
      case SynchronizationStatus.下载中:
        return new SolidColorBrush(Color.FromRgb(66, 185, 131));
      default:
        return new SolidColorBrush(Color.FromRgb(0, 0, 0));
    }
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
