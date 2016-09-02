using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoPrintr.Converters
{
    internal sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            bool isVisible = System.Convert.ToBoolean(value);

            if (parameter != null && !string.IsNullOrEmpty(parameter.ToString()))
            {
                bool parmBool = System.Convert.ToBoolean(parameter);
                isVisible = isVisible == parmBool;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}