﻿using System.Globalization;
using System.Windows.Data;

namespace ndu.ClefInspect.View
{
    public class PinSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as bool? ?? false ? "\uE141" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
