using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MAZDA_MCTool.Adorners;

namespace MAZDA_MCTool.Resources.Views.Controls;
public class DropableListbox : ListBox, INotifyPropertyChanged
{
  private DroppableListBoxItem? firstSelected;
  private double defalutOpacity = 1;
  private bool inDroping = false;
  public bool InDroping
  {
    get => inDroping;
    private set
    {
      inDroping = value;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InDroping)));
    }
  }

  public Animatable Animation
  {
    get { return (Animatable)GetValue(AnimationProperty); }
    set { SetValue(AnimationProperty, value); }
  }
  public static readonly DependencyProperty AnimationProperty =
      DependencyProperty.Register("Animation", typeof(Animatable), typeof(DropableListbox), new PropertyMetadata(null));

  public event PropertyChangedEventHandler? PropertyChanged;

  static DropableListbox()
  {
    DefaultStyleKeyProperty.OverrideMetadata(typeof(DropableListbox), new FrameworkPropertyMetadata(typeof(DropableListbox)));
  }

  public override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUp));
  }
  private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
  {
    if (SelectedItem == null || firstSelected == null)
      return;
    var adornerLayer = AdornerLayer.GetAdornerLayer(this);
    Adorner[] toRemoveArray = adornerLayer.GetAdorners(this);
    if (toRemoveArray != null)
    {
      for (int i = 0; i < toRemoveArray.Length; i++)
      {
        adornerLayer.Remove(toRemoveArray[i]);
      }
    }
    InDroping = false;
    firstSelected.Opacity = defalutOpacity;
    firstSelected = null;
  }

  private void OnMouseLeftButtonUpInItem(object sender, MouseButtonEventArgs e)
  {
    if (!InDroping)
      return;
    var remoteItem = (DroppableListBoxItem)sender;
    if (firstSelected == null || (firstSelected.Content == remoteItem.Content))
      return;
    var temp = remoteItem.Content;
    remoteItem.Content = firstSelected.Content;
    firstSelected.Content = temp;
    //_firstSelected.Opacity = _defalutOpacity;
  }
  //private void OnPreViewMouseMoveInItem(object sender, MouseEventArgs e)
  //{
  //  if (InDroping)
  //  {
  //    Debug.WriteLine("触发了动画");
  //  }
  //}
  private void OnMouseMoveInItem(object sender, MouseEventArgs e)
  {
    var item = sender as DroppableListBoxItem;
    if (item == null || SelectedItem == null) return;

    if (e.LeftButton == MouseButtonState.Pressed)
    {
      //DragDrop.DoDragDrop(item, item.DataContext ?? item.Content, DragDropEffects.Move);
      InDroping = true;
      var adornerLayer = AdornerLayer.GetAdornerLayer(this);
      if (adornerLayer.GetAdorners(this) != null)
        return;
      var selectedListBoxItem = (DroppableListBoxItem)ItemContainerGenerator.ContainerFromItem(SelectedItem);
      firstSelected = selectedListBoxItem;
      GeneralTransform transform = selectedListBoxItem.TransformToAncestor(this);
      Point position = transform.Transform(new Point(0, 0));
      adornerLayer.Add(new VisualAdorner(this, selectedListBoxItem, position, e.GetPosition(selectedListBoxItem).Y)
      { IsHitTestVisible = false });
      defalutOpacity = firstSelected.Opacity;
      firstSelected.Opacity = 0.4;
    }
  }
  protected override DependencyObject GetContainerForItemOverride()
  {
    var item = new DroppableListBoxItem();
    item.AddHandler(MouseMoveEvent, new MouseEventHandler(OnMouseMoveInItem));
    item.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUpInItem));
    //item.AddHandler(PreviewMouseMoveEvent, new MouseEventHandler(OnPreViewMouseMoveInItem));
    return item;
  }


  protected override bool IsItemItsOwnContainerOverride(object item)
  {
    return item is DroppableListBoxItem;
  }
}
