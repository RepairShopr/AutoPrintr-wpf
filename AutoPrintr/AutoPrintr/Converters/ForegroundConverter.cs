using AutoPrintr.Core.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoPrintr.Converters
{
    internal sealed class ForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = Colors.Black;

            if (value is LogType)
            {
                var logType = (LogType)value;
                switch (logType)
                {
                    case LogType.Error: color = Colors.Red; break;
                    case LogType.Warning: color = Colors.Orange; break;
                    default: break;
                }
            }

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}