using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using 马自达MC同步器.Resources.AttachedProperties;
using 马自达MC同步器.Resources.Messages;
using 马自达MC同步器.Resources.Models;
using 马自达MC同步器.Resources.Views.Controls;

namespace 马自达MC同步器.Resources.Views.Widgets;

/// <summary>
/// ModListBox.xaml 的交互逻辑
/// </summary>
public partial class ModListBox : ListBox
{
  public ModListBox()
  {
    InitializeComponent();
    WeakReferenceMessenger.Default.Register<ModInfoMessage>(this, Receive);
  }

  private async void Receive(object recipient, ModInfoMessage message)
  {
    ListBoxItem item;
    while (true)
    {
      item = (ListBoxItem)ItemContainerGenerator.ContainerFromItem(message.modInfo);
      if (item == null)
        await Task.Delay(5);
      else
        break;
    }
    App.Current.Dispatcher.Invoke(() =>
      item.RaiseEvent(new RoutedEventArgs(ItemHelper.CreatedEvent, item))
    );
  }

  private void ModList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
  {
    object dataConext = null;
    if (e.OriginalSource is FrameworkElement)
      dataConext = ((FrameworkElement)e.OriginalSource).DataContext;
    else
      dataConext = ((FrameworkContentElement)e.OriginalSource).DataContext;

    if (SelectedItems.Contains(dataConext))
      e.Handled = true;
  }

  private void IconButton_Click(object sender, RoutedEventArgs e)
  {
    IconButton iconButton = (IconButton)sender;
    ModInfo modInfo = (ModInfo)iconButton.DataContext;
    Process.Start(new ProcessStartInfo("explorer.exe", $"https://modrinth.com/mod/{modInfo.ProjectId}"));
    e.Handled = true;
  }
}