using Avalonia.Data.Converters;
using Avalonia.Media;
using moos.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace moos.Converters
{
    internal class PlaylistTracksRowOpacityConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not null && value.Count == 2)
            {
                Playlist playlist = value[0] as Playlist;
                PlaylistItem item = value[1] as PlaylistItem;
                if (item is not null && !item.IsActive)
                {
                    return 0.5;
                }
            }
            return 1.0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(value);
        }
    }
}
