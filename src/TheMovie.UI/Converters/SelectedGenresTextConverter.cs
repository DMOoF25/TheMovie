using System.Globalization;
using System.Windows.Data;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Converters;

public sealed class SelectedGenresTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable<GenreOptionViewModel> opts)
        {
            var count = opts.Count(o => o.IsSelected);
            if (count == 0) return "Vælg genrer...";
            return count == 1 ? "1 valgt" : $"{count} valgt";
        }
        return "Vælg genrer...";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Binding.DoNothing;
}