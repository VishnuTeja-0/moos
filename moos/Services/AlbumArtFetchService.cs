using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace moos.Services;

public class AlbumArtFetchService
{
    private HttpClient _client;
    
    public async Task<(bool, string, ObservableCollection<Bitmap>)> SearchAlbumArt(string title, string artists, string album)
    {
        bool isSuccess = false;
        string resultMessage = "No cover art results found";
        ObservableCollection<Bitmap> searchResults = new();
        
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")    
        );
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; moos/1.0)");
        
        string query = string.Empty;
        bool isReleaseSearch = true;
        if ((string.IsNullOrEmpty(album) || string.IsNullOrEmpty(artists)) && !string.IsNullOrEmpty(title))
        {
            query = RecordingQueryStringBuilder(title, artists, album);
            isReleaseSearch = false;
        }
        else
        {
            query = ReleaseQueryStringBuilder(artists, album);
        }

        if (!string.IsNullOrEmpty(query))
        {
            string[] mbids = await GetMBReleaseIds(query, isReleaseSearch);
            if (mbids.Length != 0)
            {
                var tasks = mbids.Select(async mbid =>
                {
                    var bitmap = await GetCoverArt(mbid);
                    return bitmap;
                });
                var results = await Task.WhenAll(tasks);
                foreach (var bmp in results) if (bmp is not null) searchResults.Add(bmp);
            }
        }

        if (searchResults.Count > 0)
        {
            isSuccess = true;
            resultMessage = "Success";
        }
        
        return (isSuccess, resultMessage, searchResults);
    }

    private string ReleaseQueryStringBuilder(string artists, string album)
    {
        
        string artist = string.IsNullOrEmpty(artists) ? string.Empty : $"artist:\"{artists}\"";
        string release = string.IsNullOrEmpty(album) ? string.Empty : $"release:\"{album}\"";

        if(string.IsNullOrEmpty(artist) && string.IsNullOrEmpty(release))
        {
            return string.Empty;
        }
        
        var query = string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(release) 
            ? $"{release}{artist}" 
            : $"{release} AND {artist}";

        return Uri.EscapeDataString(query);
    }

    private string RecordingQueryStringBuilder(string title, string artists, string album)
    {
        var parameters = new List<string>();

        if(!string.IsNullOrEmpty(title)) parameters.Add($"recording:\"{title}\"");
        if (!string.IsNullOrEmpty(artists)) parameters.Add($"artist:\"{artists}\"");
        if (!string.IsNullOrEmpty(album)) parameters.Add($"release:\"{album}\"");

        if (parameters.Count == 0) return string.Empty;

        string query = parameters.Count == 1 ? parameters[0] : string.Join(" AND ", parameters);

        return Uri.EscapeDataString(query);
    }
    
    private async Task<string[]> GetMBReleaseIds(string query, bool isReleaseSearch)
    {
        string url = isReleaseSearch 
            ? $"{Constants.AlbumIdSearchApi}?query={query}&limit=10&fmt=json"
            : $"{Constants.TrackAlbumIdSearchApi}?query={query}&limit=10&fmt=json";

        string[] result = [];
        
        var response = await _client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            using JsonDocument json = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);
            JsonElement root = json.RootElement;
            if (!isReleaseSearch)
            {
                if (root.TryGetProperty("recordings", out JsonElement recordings) && recordings.GetArrayLength() > 0) 
                {

                    result = recordings.EnumerateArray()
                        .Where(recording => recording.TryGetProperty("releases", out _))
                        .SelectMany(recording => recording.GetProperty("releases").EnumerateArray())
                        .Select(release => release.GetProperty("id").GetString())
                        .Where(id => id is not null)
                        .ToArray()!;
                }
            }
            else if (root.TryGetProperty("releases", out JsonElement releases))
            {
                result = releases.EnumerateArray()
                    .Select(release => release.GetProperty("id").GetString())
                    .Where(id => id is not null)
                    .ToArray()!;
            }
        }
        else
        {
            // Logging
            Debug.WriteLine(response.StatusCode + " " + response.ReasonPhrase);
        }

        return result;
    }

    private async Task<Bitmap?> GetCoverArt(string mbid)
    {
        string url = $"{Constants.CoverArtApi}{mbid}/front-500";
        Bitmap imageResult = null;

        var response = await _client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            await using var stream = await response.Content.ReadAsStreamAsync();
            imageResult = new Bitmap(stream);
        }

        return imageResult;
    }

    public async Task<Bitmap?> LoadLocalImage(Window parentWindow)
    {
        var files = await parentWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select an image",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Image Files")
                {
                    Patterns = ["*.png", "*.jpg", "*.jpeg", "*.bmp"]
                }
            ]
        });
        var file = files.FirstOrDefault();
        if (file == null) return null;

        await using var stream = await file.OpenReadAsync();
        return await Task.Run(() => new Bitmap(stream));
    }
}