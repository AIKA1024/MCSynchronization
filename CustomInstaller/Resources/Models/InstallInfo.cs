using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomInstaller.Resources.Models
{
  public class InstallInfo
  {
    public string DisplayName { get; set; }
    public string DisplayVersion { get; set; }
    public string InstallLocation { get; set; }
    public string UninstallString { get; set; }
  }
}
