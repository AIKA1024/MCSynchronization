using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 马自达MC同步器.Resources.Pages
{
  /// <summary>
  /// SettingPage.xaml 的交互逻辑
  /// </summary>
  public partial class SettingPage : Page
  {
    public SettingPage()
    {
      InitializeComponent();
      DataContext = Settings.Default;
    }
    private bool IsValidUrl(string urlString)
    {
      return Uri.TryCreate(urlString, UriKind.Absolute, out Uri uriResult)
          && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
    private void Button_Click(object sender, RoutedEventArgs e)
    {
      if (!IsValidUrl(AddressTextBox.Text))
      {
        MessageBox.Show("无效的地址");
        return;
      }

      if (!int.TryParse(MaxDounloadTextBox.Text, out int result)||result<1)
      {
        MessageBox.Show("无效的线程限制");
        return;
      }

      Settings.Default.Address = AddressTextBox.Text;
      Settings.Default.MaxDownloadCount = int.Parse(MaxDounloadTextBox.Text);
      Settings.Default.Save();
    }
  }
}
