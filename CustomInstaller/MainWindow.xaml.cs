using CustomInstaller.Resources.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Microsoft.Win32;
using CustomInstaller.Resources.Views.Pages;

namespace CustomInstaller
{
  public partial class MainWindow : Window
  {
    const string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MAZDAMCTools";
    public MainWindow()
    {
      InitializeComponent();
    }
    

    private RegistryKey GetInstallInfo()
    {
      RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(registryPath);
      return registryKey;
    }
    

    private void Window_Initialized(object sender, EventArgs e)
    {
      var RegistryKey = GetInstallInfo();
      if (RegistryKey == null)
      {
        MainFrame.Navigate(new InstallPage());
      }
      else
      {
        MainFrame.Navigate(new UnInstallPage()
        {
          DataContext = new InstallInfo()
          {
            DisplayName = RegistryKey.GetValue("DisplayName").ToString(),
            DisplayVersion = RegistryKey.GetValue("DisplayVersion").ToString(),
            InstallLocation = RegistryKey.GetValue("InstallLocation").ToString(),
            UninstallString = RegistryKey.GetValue("UninstallString").ToString()
          }
        });
      }
    }
  }
}
