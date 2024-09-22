using System.ComponentModel;
using fNbt;

namespace MAZDA_MCTool.Resources.Models;

public class ServerInfo(NbtCompound compound) : INotifyPropertyChanged
{
  private readonly NbtCompound compound = compound;

  public string ip
  {
    get => compound["ip"].StringValue;
    set
    {
      compound["ip"] = new NbtString("ip", value);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ip"));
    }
  }

  public string name
  {
    get => compound["name"].StringValue;
    set
    {
      compound["name"] = new NbtString("name", value);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("name"));
    }
  }

  public byte hidden
  {
    get => compound["hidden"].ByteValue;
    set
    {
      compound["hidden"] = new NbtByte("hidden", value);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("hidden"));
    }
  }

  public event PropertyChangedEventHandler? PropertyChanged;
}