using System;
using System.Globalization;
using Xamarin.Forms;

namespace TQM
{
    public class TextFormatterMinMaxLimit : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            foreach (var value in values)
            {
                if (value is null)
                {
                    return "";
                    // set a default value when unset
                }

            }

            var minLimit = (int)values[0];
            var maxLimit = (int)values[1];



            return minLimit.ToString() + " & " + maxLimit.ToString();
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        
    }
}
