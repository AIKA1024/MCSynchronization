using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 马自达MC同步器.Resources.Helper
{
  public static class Extensions
  {
    public static void Sort<TSource, TKey>(this ObservableCollection<TSource> observableCollection, Func<TSource, TKey> keySelector)
    {
      var a = observableCollection.OrderBy(keySelector).ToList();
      observableCollection.Clear();
      foreach (var b in a)
      {
        observableCollection.Add(b);
      }
    }
  }
}
