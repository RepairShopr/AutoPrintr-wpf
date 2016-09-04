using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoPrintr.Converters
{
    internal sealed class ObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = value != null && !Equals(value, null);

            if (parameter != null)
            {
                if (parameter is string)
                    isVisible = string.Compare(value.ToString(), (string)parameter, true) == 0;
                else
                    isVisible = value == parameter;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}