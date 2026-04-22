using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DiceAMillionSaveEditor.UI.ViewModels;

namespace DiceAMillionSaveEditor.UI.Converters;

public class LogLevelToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is LogLevel level)
        {
            return level switch
            {
                LogLevel.Info => new SolidColorBrush(Color.FromRgb(210, 215, 225)),     // Soft white
                LogLevel.Success => new SolidColorBrush(Color.FromRgb(74, 222, 128)),   // Green
                LogLevel.Warning => new SolidColorBrush(Color.FromRgb(251, 191, 36)),   // Amber/orange-yellow
                LogLevel.Error => new SolidColorBrush(Color.FromRgb(248, 113, 113)),    // Red
                _ => new SolidColorBrush(Color.FromRgb(210, 215, 225))
            };
        }
        return new SolidColorBrush(Color.FromRgb(210, 215, 225));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
