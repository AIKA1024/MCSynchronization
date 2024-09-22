using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using MAZDA_MCTool.Resources.Enums;

namespace MAZDA_MCTool.Resources.Converters;

internal class StausToToolTipConvert : MarkupExtension, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    var status = (SynchronizationStatus)value;
    return status switch
    {
      SynchronizationStatus.未同步 => "未与服务器取得连接",
      SynchronizationStatus.已同步 => null,
      SynchronizationStatus.缺少 => "丢失mod文件",
      SynchronizationStatus.额外 => "服务器上并没有此mod",
      SynchronizationStatus.下载中 => null,
      _ => null
    };
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
