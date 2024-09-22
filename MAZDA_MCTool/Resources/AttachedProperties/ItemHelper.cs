using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MAZDA_MCTool.Resources.AttachedProperties;

public class ItemHelper
{
  public static RoutedEvent CreatedEvent =
    EventManager.RegisterRoutedEvent("Created", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ItemHelper));

  public static void AddCleanHandler(DependencyObject dependencyObject, RoutedEventHandler handler)
  {
    if (dependencyObject is not UIElement uiElement)
      return;

    uiElement.AddHandler(CreatedEvent, handler);
  }

  public static void RemoveCleanHandler(DependencyObject dependencyObject, RoutedEventHandler handler)
  {
    if (dependencyObject is not UIElement uiElement)
      return;

    uiElement.RemoveHandler(CreatedEvent, handler);
  }

}
