using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomInstaller.Resources.Models
{
  public class InstallSetting : INotifyPropertyChanged
  {
    private string intallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MAZDA MC Tool");
    public string IntallPath
    {
      get { return intallPath; }
      set
      {
        intallPath = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IntallPath)));
      }
    }
    public bool AutoLaunch { get; set; } = true;
    public bool CreateDeskTopShortcuts { get; set; } = true;
    public bool CreateStartMenuShortcuts { get; set; } = true;

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
