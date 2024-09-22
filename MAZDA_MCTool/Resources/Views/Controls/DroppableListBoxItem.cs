using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MAZDA_MCTool.Resources.Views.Controls;
  public class DroppableListBoxItem : ListBoxItem
  {
    //private DropableListbox ParentSelector
    //{
    //  get { return (DropableListbox)ItemsControl.ItemsControlFromItemContainer(this); }
    //}
    static DroppableListBoxItem()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(DroppableListBoxItem), new FrameworkPropertyMetadata(typeof(DroppableListBoxItem)));
    }
    protected override void OnMouseEnter(MouseEventArgs e)
    {

    }
    //protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    //{
    //  base.OnMouseLeftButtonDown(e);
    //  if (!ParentSelector.InDroping)
    //  {
    //    ParentSelector?.ReleaseMouseCapture();
    //  }
    //}
  }
