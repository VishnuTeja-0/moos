using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Xaml.Interactions.Custom;
using moos.Models;

namespace moos.Converters
{
    internal class PlaylistTracksRowBackgroundConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not null && value.Count == 1) {
                PlaylistItem item = value[0] as PlaylistItem;
                if(item is not null && item.IsPlaying)
                {
                    return new SolidColorBrush(Colors.DarkCyan, 1);
                }
                else if (item is not null && !item.IsActive)
                {
                    return new SolidColorBrush(Color.FromRgb(36, 36, 36), 0.4);
                }
            }
            return new SolidColorBrush(Color.FromRgb(36, 36, 36), 1);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(value);
        }
    }
}
