using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using moos.Models;

namespace moos.Converters
{
    internal class CheckForPlayingTrackConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is not null && value.Count == 2)
            {
                Track listTrack = value[0] as Track;
                Track playingTrack = value[1] as Track;
                if(listTrack?.FilePath == playingTrack?.FilePath)
                {
                    return true;
                }
            }

            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
