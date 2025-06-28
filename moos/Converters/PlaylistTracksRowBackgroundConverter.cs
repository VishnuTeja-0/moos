using System;
using System.Drawing;
using System.Globalization;
using Avalonia.Data.Converters;

namespace moos.Converters
{
    internal class PlaylistTracksRowBackgroundConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not null) {
                if ((bool)value)
                {
                    return 0.7;
                }
            }

            return 1;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(value);
        }
    }
}
