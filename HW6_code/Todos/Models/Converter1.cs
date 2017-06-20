using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Todos.Models
{
   class Converter1 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? ischecked = value as bool?;
            if (ischecked == null || ischecked == false)
            {
                return 0;
            }
            else
            {
                return 2;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}