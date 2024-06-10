using fNbt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using 马自达MC同步器.Resources.Models;

namespace 马自达MC同步器;

public static class NBTHelper
{
  private static NbtFile? nbtFile;

  public static NbtFile? GetNBTFile()
  {
    if (nbtFile != null)
      return nbtFile;

    if (string.IsNullOrEmpty(Settings.Default.GamePath))
      return null;

    var gamePath = Path.Combine(Settings.Default.GamePath, "servers.dat");
    if (!string.IsNullOrEmpty(gamePath))
      nbtFile = new NbtFile(gamePath);
    else
      MessageBox.Show("未找到服务器列表文件");
    return nbtFile;
  }

  public static void SaveNBTFile()
  {
    nbtFile.SaveToFile(nbtFile.FileName, NbtCompression.None);
  }
}