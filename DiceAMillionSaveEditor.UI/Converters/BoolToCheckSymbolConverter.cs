using System;
using System.Globalization;
using System.Windows.Data;

namespace DiceAMillionSaveEditor.UI.Converters;

public class BoolToCheckSymbolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool unlocked && unlocked ? "✔" : "✖";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
