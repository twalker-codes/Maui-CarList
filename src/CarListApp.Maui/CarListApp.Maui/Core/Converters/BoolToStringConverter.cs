using System.Globalization;

namespace CarListApp.Maui.Core.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string strParameter)
            {
                var options = strParameter.Split('|');
                if (options.Length == 2)
                {
                    return boolValue ? options[0] : options[1];
                }
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue && parameter is string strParameter)
            {
                var options = strParameter.Split('|');
                if (options.Length == 2)
                {
                    return stringValue.Equals(options[0], StringComparison.OrdinalIgnoreCase);
                }
            }
            return false;
        }
    }
} 