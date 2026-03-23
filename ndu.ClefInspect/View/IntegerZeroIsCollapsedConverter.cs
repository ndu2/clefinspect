using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ndu.ClefInspect.View
{
    public class IntegerZeroIsCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int b && b != 0)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v && v == Visibility.Visible)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
