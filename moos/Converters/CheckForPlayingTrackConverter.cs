using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using moos.Models;
using moos.ViewModels;

namespace moos.Converters
{
    internal class CheckForPlayingTrackConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isPlaying = false;
            if(value is not null && value.Count == 4)
            {
                bool isLibraryCheck = value[2] as bool? ?? true;
                Track? checkTrack = isLibraryCheck ? value[0] as Track : (value[0] as PlaylistItem)?.Track;
                Track? playingTrack = value[1] as Track;
                isPlaying = value[3] as bool? ?? false;
                if (checkTrack?.FilePath == playingTrack?.FilePath)
                {
                    return true && (isLibraryCheck || isPlaying);
                }
            }

            return false && isPlaying;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
