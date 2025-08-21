using System.Globalization;
using System.Windows.Data;

namespace TheMovie.UI.Converters;

public sealed class DateOnlyToDateTimeConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateOnly d)
            return new DateTime(d.Year, d.Month, d.Day);
        return null;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
            return DateOnly.FromDateTime(dt);
        return default(DateOnly);
    }
}