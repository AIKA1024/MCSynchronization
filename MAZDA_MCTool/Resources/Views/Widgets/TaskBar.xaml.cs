using System.Windows.Controls;

namespace MAZDA_MCTool.Resources.Views.Widgets;

/// <summary>
///   TaskBar.xaml 的交互逻辑
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