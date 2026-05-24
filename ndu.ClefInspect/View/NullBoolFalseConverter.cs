using System.Globalization;
using System.Windows.Data;

namespace ndu.ClefInspect.View
{
    public class NullBoolFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("unsupported");
        }
    }
}
