using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using 马自达MC同步器.Resources.Enums;

namespace 马自达MC同步器.Resources.Converters;

internal class StausToMDL2Convert : MarkupExtension, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    var status = (SynchronizationStatus)value;
    switch (status)
    {
      case SynchronizationStatus.未同步:
        return SegoeMDL2.问号;
      case SynchronizationStatus.已同步:
        return SegoeMDL2.黑框对勾;
      case SynchronizationStatus.缺少:
        return SegoeMDL2.叉;
      case SynchronizationStatus.额外:
        return SegoeMDL2.三角黑框感叹号;
      case SynchronizationStatus.下载中:
        return SegoeMDL2.下载;
      default:
        return SegoeMDL2.问号;
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