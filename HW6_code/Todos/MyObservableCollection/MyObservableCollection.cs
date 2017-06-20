using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todos.MyObservableCollection
{
    class MyObservableCollection<T> : ObservableCollection<T>
    {
        public void MySetItem(int index, T item)
        {
            SetItem(index, item);
        }
    }
}
