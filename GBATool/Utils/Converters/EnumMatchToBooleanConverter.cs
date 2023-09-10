using System;
using System.Globalization;
using System.Windows.Data;

namespace GBATool.Utils.Converters
{
    public class EnumMatchToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? checkValue = value.ToString();
            string? targetValue = parameter.ToString();

            if (checkValue != null)
            {
                return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool useValue = (bool)value;
            string? targetValue = parameter.ToString();

            if (useValue && targetValue != null)
            {
                return Enum.Parse(targetType, targetValue);
            }

            throw new NotImplementedException();
        }
    }
}
