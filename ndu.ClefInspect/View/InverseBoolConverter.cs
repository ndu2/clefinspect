using System.Globalization;
using System.Windows.Data;

namespace ndu.ClefInspect.View
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }
            throw new ArgumentException("unsupported argument type");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }
            throw new ArgumentException("unsupported argument type");
        }
    }
}
