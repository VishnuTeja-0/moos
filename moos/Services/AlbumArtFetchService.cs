using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Xaml.Interactions.Core;
using HarfBuzzSharp;

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
        
        string albumIdQuery = QueryStringBuilder(title, artists, album);
        string[] mbids = await GetMBReleaseIds(albumIdQuery);
        if (mbids.Length != 0)
        {
            var tasks = mbids.Select(async mbid =>
            {
                var bitmap = await GetCoverArt(mbid);
                return bitmap;
            });
            var results = await Task.WhenAll(tasks);
            foreach (var bmp in results) if(bmp is not null) searchResults.Add(bmp);
        }

        if (searchResults.Count > 0)
        {
            isSuccess = true;
            resultMessage = "Success";
        }
        
        return (isSuccess, resultMessage, searchResults);
    }

    private string QueryStringBuilder(string title, string artists, string album)
    {
        
        string artist = string.IsNullOrEmpty(artists) ? string.Empty : $"artist:\"{artists}\"";
        string release = string.IsNullOrEmpty(album) ? string.Empty : $"release:\"{album}\"";
        string recording = string.IsNullOrEmpty(title) ? string.Empty : $"recording:\"{title}\"";
        
        string query = $"({release} OR {recording}) AND {artist}";
        return Uri.EscapeDataString(query);
    }
    
    private async Task<string[]> GetMBReleaseIds(string query)
    {
        string url = $"{Constants.AlbumIdSearchApi}?query={query}&limit=10&fmt=json";
        string[] result = [];
        
        var response = await _client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            using JsonDocument json = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);
            var root = json.RootElement;
            if (root.TryGetProperty("releases", out JsonElement releases) && releases.GetArrayLength() > 0)
            {
                result = releases.EnumerateArray().Select(release => release.GetProperty("id").GetString()).ToArray();
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
        string url = $"{Constants.CoverArtApi}{mbid}/front";
        Bitmap imageResult = null;

        var response = await _client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            using var stream = await response.Content.ReadAsStreamAsync();
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