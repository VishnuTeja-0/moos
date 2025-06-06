using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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

    private ObservableCollection<Bitmap> __AlbumArtPreCrops = [];
    public ObservableCollection<Bitmap> AlbumArtPreChops
    {
        get => __AlbumArtPreCrops;
        set => this.RaiseAndSetIfChanged(ref __AlbumArtPreCrops, value);
    }

    private bool? _IsLoadingResults = false;
    public bool? IsLoadingResults
    {
        get => _IsLoadingResults;
        set => this.RaiseAndSetIfChanged(ref _IsLoadingResults, value);
    }
    
    private AlbumArtFetchService _AlbumArtService = new();
    private IImageEditor _ImageService;
    
    public ICommand SearchAlbumArtCommand { get; }
    private async void SearchAlbumArt()
    {
        if (AlbumArtSearchResults is not null)
        {
            AlbumArtSearchResults.Clear();
            AlbumArtSearchResults = null;
        }
        IsLoadingResults = true;
        try
        {
            (bool isSuccess, string resultMessage, AlbumArtSearchResults)
                = await _AlbumArtService.SearchAlbumArt(TrackTitle, TrackArtists, TrackAlbum);
            if (!isSuccess)
            {
                // Pop Up
                Debug.WriteLine(resultMessage);
            }
        }
        catch (Exception ex)
        {
            // Error Display
            Debug.WriteLine(ex);
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
            Debug.WriteLine(ex);
        }
        IsLoadingResults = false;
    }

    

    private bool _IsEditMode = false;
    public bool IsEditMode
    {
        get => _IsEditMode;
        set => this.RaiseAndSetIfChanged(ref _IsEditMode, value);
    }

    private double _FrameSide;
    public double FrameSide
    {
        get => _FrameSide;
        set => this.RaiseAndSetIfChanged(ref _FrameSide, value);
    }

    private double _CropSide = Constants.DefaultCropSide;
    public double CropSide
    {
        get => _CropSide;
        set => this.RaiseAndSetIfChanged(ref _CropSide, value);
    }
    
    private double _CropX = Constants.DefaultCropPosition;
    public double CropX
    {
        get => _CropX;
        set => this.RaiseAndSetIfChanged(ref _CropX, value);
    }
    
    private double _CropY = Constants.DefaultCropPosition;
    public double CropY
    {
        get => _CropY;
        set => this.RaiseAndSetIfChanged(ref _CropY, value);
    }

    public ICommand ToggleEditModeCommand { get; }
    private Bitmap _temp;
    public ICommand SaveCropCommand { get; }
    private async Task SaveCrop()
    {
        if (IsEditMode)
        {
            
            AlbumArtPreChops.Add(SelectedAlbumArt!);
            Bitmap? cropped = null;
            await Task.Run(() => { 
                cropped = _ImageService.CropBitmap(SelectedAlbumArt!, (int)CropX, (int)CropY, (int)CropSide); 
            });
            if(cropped is not null)
            {
                SelectedAlbumArt = null;
                SelectedAlbumArt = cropped;
            }
            IsEditMode = false;
        }
    }

    public ICommand UndoCropCommand { get; }
    private void UndoCrop()
    {
        int undoCount = AlbumArtPreChops.Count();
        if (undoCount > 0)
        {
            int index = undoCount - 1;
            SelectedAlbumArt = AlbumArtPreChops.ElementAt(index);
            AlbumArtPreChops.RemoveAt(index);
        }
    }

    public ICommand SaveAlbumArtCommand { get; }
    private async Task SaveAlbumArt()
    {
        Bitmap scaled = SelectedAlbumArt!;
        await Task.Run(() => { scaled = _ImageService.ResizeSelectedImage(SelectedAlbumArt!, null, null); });
        SelectedAlbumArt = scaled;
    }
    
    public AlbumArtSelectionWindowViewModel(IImageEditor imageService, string? dialogTrackTitle, string? dialogTrackArtists, string? dialogTrackAlbum, Bitmap? dialogTrackAlbumArt)
    {
        _ImageService = imageService;
        TrackTitle = dialogTrackTitle;
        TrackArtists = dialogTrackArtists;
        TrackAlbum = dialogTrackAlbum;
        SelectedAlbumArt = dialogTrackAlbumArt ?? new Bitmap(AssetLoader.Open(new Uri(Constants.DefaultAlbumArtPath)));
        
        SearchAlbumArtCommand = ReactiveCommand.Create(() =>
        {
            SearchAlbumArt();
        });

        SelectLocalAlbumArtCommand = ReactiveCommand.Create((Window window) =>
        {
            SelectLocalAlbumArt(window);
        });

        ToggleEditModeCommand = ReactiveCommand.Create(() =>
        {
            IsEditMode = !IsEditMode;
            if (!IsEditMode)
            {
                CropSide = Constants.DefaultCropSide;
                CropX = Constants.DefaultCropPosition;
                CropY = Constants.DefaultCropPosition;
            }
        });

        SaveAlbumArtCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await SaveAlbumArt();
        });

        SaveCropCommand = ReactiveCommand.CreateFromTask(async () => 
        {
            await SaveCrop();
        });

        UndoCropCommand = ReactiveCommand.Create(() => 
        { 
            UndoCrop(); 
        });
    }
}