using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Xaml.Interactions.Custom;
using moos.Interfaces;
using moos.Services;
using ReactiveUI;

namespace moos.ViewModels;

public partial class AlbumArtSelectionWindowViewModel : ViewModelBase
{
    private string? _TrackTitle;
    public string? TrackTitle
    {
        get => _TrackTitle;
        set => this.RaiseAndSetIfChanged(ref _TrackTitle, value);
    }
    
    private string? _TrackArtists;
    public string? TrackArtists
    {
        get => _TrackArtists;
        set => this.RaiseAndSetIfChanged(ref _TrackArtists, value);
    }
    
    private string? _TrackAlbum;
    public string? TrackAlbum
    {
        get => _TrackAlbum;
        set => this.RaiseAndSetIfChanged(ref _TrackAlbum, value);
    }

    private ObservableCollection<Bitmap>? _AlbumArtSearchResults;
    public ObservableCollection<Bitmap>? AlbumArtSearchResults
    {
        get => _AlbumArtSearchResults;
        set => this.RaiseAndSetIfChanged(ref _AlbumArtSearchResults, value);
    }
    
    private Bitmap? _SelectedAlbumArt;
    public Bitmap? SelectedAlbumArt
    {
        get => _SelectedAlbumArt;
        set => this.RaiseAndSetIfChanged(ref _SelectedAlbumArt, value);
    }

    private bool? _IsLoadingResults = false;
    public bool? IsLoadingResults
    {
        get => _IsLoadingResults;
        set => this.RaiseAndSetIfChanged(ref _IsLoadingResults, value);
    }
    
    private AlbumArtFetchService _AlbumArtService = new();
    private IImageEditor _ImageEditor;
    
    public ICommand SearchAlbumArtCommand { get; }
    private async void SearchAlbumArt()
    {
        if (AlbumArtSearchResults is not null)
        {
            AlbumArtSearchResults.Clear();
        }
        IsLoadingResults = true;
        try
        {
            (bool isSuccess, string resultMessage, AlbumArtSearchResults)
                = await _AlbumArtService.SearchAlbumArt(TrackTitle, TrackArtists, TrackAlbum);
            if (!isSuccess)
            {
                // Pop Up
                Console.WriteLine(resultMessage);
            }
        }
        catch (Exception ex)
        {
            // Error Display
            Console.WriteLine(ex);
        }
        IsLoadingResults = false;
    }

    public ICommand SelectLocalAlbumArtCommand { get; }
    public async void SelectLocalAlbumArt(Window window)
    {
        IsLoadingResults = true;
        try
        {
            var bitmap = await _AlbumArtService.LoadLocalImage(window);
            SelectedAlbumArt = bitmap ?? throw new FileLoadException();
        }
        catch (Exception ex)
        {
            // Logging and Error display
            Console.WriteLine(ex);
        }
        IsLoadingResults = false;
    }

    private IImageEditor _ImageService = new ImageEditorService();

    private bool _IsEditMode = false;
    public bool IsEditMode
    {
        get => _IsEditMode;
        set => this.RaiseAndSetIfChanged(ref _IsEditMode, value);
    }

    private double? _CropSide = 150;
    public double? CropSide
    {
        get => _CropSide;
        set => this.RaiseAndSetIfChanged(ref _CropSide, value);
    }
    
    private double? _CropX = 25;
    public double? CropX
    {
        get => _CropX;
        set => this.RaiseAndSetIfChanged(ref _CropX, value);
    }
    
    private double? _CropY = 25;
    public double? CropY
    {
        get => _CropY;
        set => this.RaiseAndSetIfChanged(ref _CropY, value);
    }
    
    public ICommand SaveAlbumArtCommand { get; }
    private void SaveAlbumArt()
    {
        var scaled = _ImageService.ResizeSelectedImage(SelectedAlbumArt);
        
    }
    
    public AlbumArtSelectionWindowViewModel(string? dialogTrackTitle, string? dialogTrackArtists, string? dialogTrackAlbum, string? dialogTrackAlbumArtUri)
    {
        TrackTitle = dialogTrackTitle;
        TrackArtists = dialogTrackArtists;
        TrackAlbum = dialogTrackAlbum;
        SelectedAlbumArt = new Bitmap(AssetLoader.Open(new Uri(dialogTrackAlbumArtUri)));
        
        SearchAlbumArtCommand = ReactiveCommand.Create(() =>
        {
            SearchAlbumArt();
        });

        SelectLocalAlbumArtCommand = ReactiveCommand.Create((Window window) =>
        {
            SelectLocalAlbumArt(window);
        });

        SaveAlbumArtCommand = ReactiveCommand.Create(() =>
        {
            SaveAlbumArt();
        });
    }
}