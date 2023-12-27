using System;
using System.Globalization;
using Xamarin.Forms;

namespace TQM
{
    public class TextFormatterDrumViewReport : IMultiValueConverter
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

            //var machineCat = (string)values[0];
            var drumNo = (string)values[0];
            var Strength = (string)values[1];

            //if (machineCat != "Spinning" && machineCat != "Winding")
            //{
            //    return formatDecimal(act, 4).ToString() + " [Std Hank:" + formatDecimal(exp, 4).ToString() + " " + deviation.ToString() + "]";
            //}

            //if (machineCat == "Spinning" || machineCat == "Winding")
            //{
            //    return formatDecimal(act, 2).ToString() + " [Std Count:" + formatDecimal(exp, 2).ToString() + " " + deviation.ToString() + "]";
            //}

            return drumNo + "\n" + Strength;

            //return "";
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
                if (afterDecimalCount == 4)
                {
                    return decimal.Parse(inputString + ".0000");
                }
                else { return decimal.Parse(inputString + ".00"); }
            }
        }
    }
}
