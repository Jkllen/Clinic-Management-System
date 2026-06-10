using System;
using System.Globalization;
using System.Windows.Data;

/// <summary>
/// Converter that converts a numeric value to a formatted Peso currency string and vice versa.
namespace CruzNeryClinic.Converters
{
    public class PesoCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
                return $"₱ {decimalValue:N2}";

            if (value is double doubleValue)
                return $"₱ {doubleValue:N2}";

            if (value is int intValue)
                return $"₱ {intValue:N2}";

            return "₱ 0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value?.ToString() ?? "0";

            text = text
                .Replace("₱", "")
                .Replace(",", "")
                .Trim();

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;

            return 0m;
        }
    }
}