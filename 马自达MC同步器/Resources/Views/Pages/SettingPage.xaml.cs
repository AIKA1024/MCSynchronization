﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Velopack;
using Velopack.Sources;
using 马自达MC同步器.Resources.ViewModels;

namespace 马自达MC同步器.Resources.Pages;

/// <summary>
///   SettingPage.xaml 的交互逻辑
/// </summary>
public partial class SettingPage : Page
{
  private readonly SettingPageViewModel settingPageViewModel = new();

  public SettingPage()
  {
    InitializeComponent();
    DataContext = settingPageViewModel;
  }

  private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
  {
    Process.Start(new ProcessStartInfo("explorer.exe", e.Uri.AbsoluteUri));
    e.Handled = true;
  }

  private void Expander_Click(object sender, RoutedEventArgs e)
  {

  }
}