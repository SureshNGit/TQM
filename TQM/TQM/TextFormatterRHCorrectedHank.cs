using System;
using System.Globalization;
using Xamarin.Forms;

namespace TQM
{
    public class TextFormatterRHCorrectedHank : IMultiValueConverter
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

            var rhcorrectedhank = (decimal)values[0];
            var rhcorrection = (int)values[1];

            return formatDecimal(rhcorrectedhank, 4).ToString() + " [RH% : " + rhcorrection.ToString() + "]";
         }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private decimal formatDecimal(decimal inputVal, int afterDecimalCount = 4)
        {

            inputVal = Math.Round(inputVal, afterDecimalCount);
            string inputString = inputVal.ToString();
            string[] ipStringArray = inputString.Split('.');
            if (ipStringArray.Length > 1)
            {
                string beforeDecimal = ipStringArray[0];
                string afterDecimal = ipStringArray[1];
                for (int i = ipStringArray[1].Length; i < afterDecimalCount; i++)
                {
                    afterDecimal = afterDecimal + "0";
                }
                return decimal.Parse(beforeDecimal + "." + afterDecimal);
            }
            else
            {
                return decimal.Parse(inputString + ".0000");
            }
        }
    }
}
