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

namespace 马自达MC同步器.Resources.Views.Widgets;

/// <summary>
/// TaskBar.xaml 的交互逻辑
/// </summary>
public partial class TaskBar : UserControl
{
  public TaskBar()
  {
    InitializeComponent();
  }

  public void UpdateText(string text) //先这样吧 懒
  {
    TipText.Text = text;
  }
}