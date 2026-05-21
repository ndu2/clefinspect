using System.Globalization;
using System.Windows.Data;

namespace ndu.ClefInspect.View
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string s)
            {
                return String.Format(s, value);
            }
            throw new ArgumentException("unsupported parameter", nameof(parameter));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("unsupported");
        }
    }
}
