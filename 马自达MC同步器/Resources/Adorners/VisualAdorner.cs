using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace 马自达MC同步器.Adorners
{
  internal class VisualAdorner : Adorner
  {
    private VisualBrush visualBrush;
    private Point _mousePosition;
    private UIElement _visualElement;
    private double _offset;
    public VisualAdorner(UIElement adornedElement, UIElement VisualElement, Point mousePoint, double offset) : base(adornedElement)
    {
      SnapsToDevicePixels = true;
      _offset = offset;
      _mousePosition = mousePoint;
      _visualElement = VisualElement;
      //int width = (int)VisualElement.RenderSize.Width;
      //int height = (int)VisualElement.RenderSize.Height;

      visualBrush = new (VisualElement);
      if (visualBrush.CanFreeze)
        visualBrush.Freeze();
      adornedElement.MouseMove += AdornedElement_MouseMove;
    }


    private void AdornedElement_MouseMove(object sender, MouseEventArgs e)
    {
      _mousePosition.Y = e.GetPosition(this).Y - _offset;
      InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
      base.OnRender(drawingContext);

      var adornedElementRect = new Rect(_mousePosition, _visualElement.RenderSize);
      drawingContext.DrawRectangle(visualBrush, null, adornedElementRect);
    }
  }
}
