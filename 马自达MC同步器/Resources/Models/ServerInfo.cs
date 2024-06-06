using fNbt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 马自达MC同步器.Resources.Models
{
  public class ServerInfo(NbtCompound compound) : INotifyPropertyChanged
  {
    private NbtCompound compound = compound;
    public string ip
    {
      get { return compound["ip"].StringValue; }
      set
      {
        compound["ip"] = new NbtString("ip", value);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ip"));
      }
    }

    public string name
    {
      get { return compound["name"].StringValue; }
      set
      {
        compound["name"] = new NbtString("name", value);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("name"));
      }
    }

    public byte hidden
    {
      get { return compound["hidden"].ByteValue; }
      set
      {
        compound["hidden"] = new NbtByte("hidden", value);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("hidden"));
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
  }
}
