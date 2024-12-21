using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace clef_inspect.View
{
    public class BooleanVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool v)
            {
                return v ? Visibility.Visible : Visibility.Collapsed;
            }
            throw new ArgumentException("unsupported type", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
            {
                if (v == Visibility.Visible)
                {
                    return true;
                }
                if (v == Visibility.Collapsed)
                {
                    return false;
                }
                throw new ArgumentException("unsupported value "+v, nameof(value));
            }
            throw new ArgumentException("unsupported type", nameof(value));
        }
    }
}
