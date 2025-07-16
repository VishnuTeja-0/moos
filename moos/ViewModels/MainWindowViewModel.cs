using moos.Models;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using moos.Services;
using System.Linq;
using Avalonia.Controls;
using moos.Interfaces;
using Avalonia.Media.Imaging;
using moos.Views;
using System.Diagnostics;
using moos.Views.MainWindowControls;


namespace moos.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{


    #region Library Commands
    private Models.Library _LocalLibrary = new();
    public Models.Library LocalLibrary
    {
        get => _LocalLibrary;
        set => this.RaiseAndSetIfChanged(ref _LocalLibrary, value);
    }

    private ObservableCollection<Track> _libraryDataGridSource;
    public ObservableCollection<Track> LibraryDataGridSource
    {
        get => _libraryDataGridSource;
        set => this.RaiseAndSetIfChanged(ref _libraryDataGridSource, value);
    }

    private bool? _IsLibraryLoading = false;
    public bool? IsLibraryLoading
    {
        get => _IsLibraryLoading;
        set => this.RaiseAndSetIfChanged(ref _IsLibraryLoading, value);
    }

    public ICommand ReloadLibrary { get; }
    private void LoadLibrary()
    {
        IsLibraryLoading = true;
        try
        {
            LibraryDataGridSource = LocalLibrary.LoadLocalCollection(Constants.LibraryFolder);
        }
        catch (Exception ex)
        {
            //Logging and Error Display - Critical
            Debug.WriteLine(ex.Message);
        }
        IsLibraryLoading = false;
    }

    private string? _YtUrl;
    public string? YtUrl
    {
        get => _YtUrl;
        set => this.RaiseAndSetIfChanged(ref _YtUrl, value);
    }

    private YTDownloaderService? _DownloadService;

    private double? _DownloadProgress = 0;
    public double? DownloadProgress
    {
        get => _DownloadProgress;
        set => this.RaiseAndSetIfChanged(ref _DownloadProgress, value);
    }

    public ICommand DownloadYoutubeMp3DirectCommand { get; }
    public ICommand CancelCurrentDownloadCommand { get; }
    private IDisposable? _downloadTimerSubscription;

    private async Task<bool> GetYoutubeAudio()
    {
        IsLibraryLoading = true;
        bool isDownloadSuccess = false, isMetadataSuccess = false;
        string downloadResult = "";
        try
        {
            if (YtUrl is not null)
            {
                _DownloadService = new YTDownloaderService();
                StartDownloadPolling();
                (isDownloadSuccess, downloadResult) =
                    await _DownloadService.DownloadSong(YtUrl, Constants.LibraryFolder, Constants.ProjectDirectory);

                if (isDownloadSuccess)
                {
                    DownloadProgress = 90;
                    LoadLibrary();
                    Track newTrack = LibraryDataGridSource.First(track => track.FilePath == downloadResult);
                    DownloadProgress = 100;
                    (isMetadataSuccess, newTrack) = await _DownloadService.FetchVideoMetadata(YtUrl, newTrack);
                }
            }
        }
        catch (Exception ex)
        {
            //Logging and Error Display
            Debug.WriteLine(ex.Message);
        }

        if (!isDownloadSuccess)
        {
            // Logging and Error Display
            Debug.WriteLine("There was an error in downloading audio from youtube: {0}", downloadResult);
        }
        else
        {
            YtUrl = null;
        }

        if (!isMetadataSuccess)
        {
            // Logging
            Debug.WriteLine("There was an error in fetching youtube metadata: {0}", downloadResult);
        }

        IsLibraryLoading = false;
        DownloadProgress = 0;
        return isDownloadSuccess;
    }

    private void StartDownloadPolling()
    {
        _downloadTimerSubscription = Observable
            .Interval(TimeSpan.FromMilliseconds(100))
            .SubscribeOn(TaskPoolScheduler.Default)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                DownloadProgress = _DownloadService.GetProgressPercentage() * 0.8;

                if (DownloadProgress >= 80)
                {
                    _downloadTimerSubscription?.Dispose();
                    _downloadTimerSubscription = null;
                }
            });
    }

    private ObservableCollection<Track>? _SelectedTracks;
    public ObservableCollection<Track>? SelectedTracks
    {
        get => _SelectedTracks;
        set
        {
            this.RaiseAndSetIfChanged(ref _SelectedTracks, value);
            this.RaisePropertyChanged(nameof(IsMetadataEditEnabled));
        }
    }
    #endregion

    #region Metadata Commands 
    private IImageEditor _ImageEditor = new ImageEditorService();

    public bool IsMetadataEditEnabled
    {
        get
        {
            return SelectedTracks is not null &&
                    SelectedTracks.Count == 1 &&
                    SelectedTracks[0].FilePath != PlayingTrack?.FilePath;
        }
    }

    private Track? _DialogTrack;
    public Track? DialogTrack
    {
        get => _DialogTrack;
        set => this.RaiseAndSetIfChanged(ref _DialogTrack, value);
    }

    public ICommand OpenMetadataDialogCommand { get; }
    public ICommand ResetMetadataDialogCommand { get; }

    private bool _IsMetadataDialogOpen = false;
    public bool IsMetadataDialogOpen
    {
        get => _IsMetadataDialogOpen;
        set => this.RaiseAndSetIfChanged(ref _IsMetadataDialogOpen, value);
    }

    private string? _NewArtist;
    public string? NewArtist
    {
        get => _NewArtist;
        set => this.RaiseAndSetIfChanged(ref _NewArtist, value);
    }

    public ICommand OpenAlbumArtWindowCommand { get; }

    private async void OpenAlbumArtWindow(Window mainWindow)
    {
        var albumArtWindow = new AlbumArtSelectionWindow
        {
            DataContext = new AlbumArtSelectionWindowViewModel(
                _ImageEditor,
                DialogTrack?.Title ?? "",
                DialogTrack?.DisplayArtists ?? "",
                DialogTrack?.Album ?? "",
                DialogTrack?.AlbumArt),
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var selectedBitmap = await albumArtWindow.ShowDialogWithResult(mainWindow);
        if (selectedBitmap is not null)
        {
            IsMetadataDialogOpen = false;
            DialogTrack!.AlbumArt = selectedBitmap;
            IsMetadataOptionEnabled = true;
            IsMetadataDialogOpen = true;
        }
    }

    public ICommand SubmitMetadataChangesCommand { get; }

    private async void SubmitMetadataChanges()
    {
        if (!IsDialogYearWarningVisible)
        {
            try
            {
                await Task.Run(() => { LocalLibrary.EditTrackMetadata(DialogTrack!, Constants.LibraryFolder, _ImageEditor); });
                await Task.Run(() => { LoadLibrary(); });
                SelectedTracks = [];
                SelectedTracks.Add(DialogTrack);
                IsMetadataDialogOpen = false;
            }
            catch (Exception ex)
            {
                //Logging and Error Display
                Debug.WriteLine(ex.Message);
            }
        }
    }

    public ICommand EnterNewDialogArtistCommand { get; }
    public ICommand RemoveDialogArtistCommand { get; }

    private bool _IsMetadataOptionEnabled = false;
    public bool IsMetadataOptionEnabled
    {
        get => _IsMetadataOptionEnabled;
        set => this.RaiseAndSetIfChanged(ref _IsMetadataOptionEnabled, value);
    }

    private bool _IsDialogYearWarningVisible = false;
    public bool IsDialogYearWarningVisible
    {
        get => _IsDialogYearWarningVisible;
        set => this.RaiseAndSetIfChanged(ref _IsDialogYearWarningVisible, value);
    }

    public ICommand SetMetadataFormActionsCommand { get; }
    public ICommand AddToPlaylistCommand { get; }
    #endregion

    #region Player Commands
    private IAudioPlayer _Player;

    private Track? _PlayingTrack;
    public Track? PlayingTrack
    {
        get => _PlayingTrack;
        set => this.RaiseAndSetIfChanged(ref _PlayingTrack, value);
    }

    private Bitmap? _PlayingTrackAlbumArt;
    public Bitmap? PlayingTrackAlbumArt
    {
        get => _PlayingTrackAlbumArt;
        set => this.RaiseAndSetIfChanged(ref _PlayingTrackAlbumArt, value);
    }

    private bool _IsPlaying = false;
    public bool IsPlaying
    {
        get => _IsPlaying;
        set => this.RaiseAndSetIfChanged(ref _IsPlaying, value);
    }

    private Models.Playlist? _Playlist;
    public Models.Playlist? Playlist
    {
        get => _Playlist;
        set => this.RaiseAndSetIfChanged(ref _Playlist, value);
    }

    private ObservableCollection<PlaylistItem> _CurrentTrackList = [];
    public ObservableCollection<PlaylistItem> CurrentTrackList
    {
        get => _CurrentTrackList;
        set => this.RaiseAndSetIfChanged(ref _CurrentTrackList, value);
    }

    public ICommand PlaySingleTrackCommand { get; }

    private IDisposable? _playbackTimerSubscription;

    private void SetAndPlayTrack(Track track, float? speed, float? pitch)
    {
        ResetPlayback();
        InitializeTrack(track);

        if (speed is null && Playlist is null)
        {
            Playlist = new Models.Playlist();
            CurrentTrackList = Playlist.AddTrack(track, Constants.DefaultPlayingSpeed, Constants.DefaultPlayingPitch);
        }

        if (speed is not null)
        {
            PlayingTrackSpeed = speed.Value;
        }
        if (pitch is not null)
        {
            PlayingTrackPitch = pitch.Value;
        }

        _Player = AudioPlayerFactory.CreatePlayer();
        _Player.PlayTrack(track.FilePath);
        _Player!.SetVolume(PlayerVolume);
        IsPlaying = true;
        
        StartPlaybackTimer();
    }

    private void ResetPlayback()
    {
        if (PlayingTrack is not null)
        {
            _playbackTimerSubscription?.Dispose();
            _playbackTimerSubscription = null;
            _Player.StopTrack();
            PlayingTrackPosition = 0;
            PlayingTrackSpeed = Constants.DefaultPlayingSpeed;
            PlayingTrackPitch = Constants.DefaultPlayingPitch;
        }
    }

    private void InitializeTrack(Track track)
    {
        // UI update workaround
        if (PlayingTrack is not null && track.FilePath == PlayingTrack.FilePath) Task.Run(() => LoadLibrary());

        PlayingTrack = (Track)track!.Clone();
        this.RaisePropertyChanged(nameof(IsMetadataEditEnabled));
        if (PlayingTrack.AlbumArt is null)
        {
            PlayingTrack.SetAlbumArt(Constants.DefaultAlbumArtPath);
        }

        PlayingTrackAlbumArt = PlayingTrack.AlbumArt;
    }

    private void StartPlaybackTimer()
    {
        _playbackTimerSubscription = Observable
            .Interval(TimeSpan.FromMilliseconds(300))
            .SubscribeOn(TaskPoolScheduler.Default)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                PlayingTrackPosition = _Player.GetPosition();
                if (Math.Floor(PlayingTrackPosition) >= Math.Floor(PlayingTrack!.Duration.TotalSeconds))
                {
                    IsPlaying = false;
                    PlayNextTrack();
                }
            });
    }

    public ICommand PlayNextCommand { get; }
    private void PlayNextTrack()
    {
        if (Playlist is null)
        {
            return;
        }

        PlaylistItem? trackItem = Playlist.ReturnTrack();
        if (trackItem is null)
        {
            PlayingTrackPosition += 5;
            return;
        }
        else
        {
            // UI update workaround
            CurrentTrackList = [];
            CurrentTrackList = Playlist.CurrentPlaylist;
        }

        SetAndPlayTrack(trackItem.Track, trackItem.Speed, trackItem.Pitch);
    }

    public ICommand PlayPreviousCommand { get; }
    private void PlayPreviousTrack()
    {
        if (Playlist is null || PlayingTrackPosition > 5)
        {
            PlayingTrackPosition = 0;
            return;
        }

        PlaylistItem? trackItem = Playlist.ReturnTrack(-1);
        if (trackItem is null)
        {
            PlayingTrackPosition = 0;
            return;
        }
        else
        {
            // UI update workaround
            CurrentTrackList = [];
            CurrentTrackList = Playlist.CurrentPlaylist;
        }

        SetAndPlayTrack(trackItem.Track, trackItem.Speed, trackItem.Pitch);
    }

    public ICommand PlayPauseActiveTrackCommand { get; }
    private void PlayPauseActiveTrack(bool IsMuted)
    {
        if (!IsMuted)
        {
            _Player!.PauseTrack();
            IsPlaying = false;
        }
        else
        {
            if (Math.Floor(PlayingTrackPosition) >= Math.Floor(PlayingTrack!.Duration.TotalSeconds))
            {
                PlayingTrackPosition = 0;
                _Player!.SeekToPosition(0);
            }
            _Player!.ResumeTrack();
            IsPlaying = true;
        }
    }

    private float _PlayerVolume = Constants.DefaultPlayingVolume;
    public float PlayerVolume
    {
        get => _PlayerVolume;
        set => this.RaiseAndSetIfChanged(ref _PlayerVolume, value);
    }
    private float tempVolume = Constants.DefaultPlayingVolume;

    public ICommand MuteButtonCommand { get; }

    private double _PlayingTrackPosition = 0;
    public double PlayingTrackPosition
    {
        get => _PlayingTrackPosition;
        set
        {
            this.RaiseAndSetIfChanged(ref _PlayingTrackPosition, value);
            this.RaisePropertyChanged(nameof(DisplayPlayingPosition));
        }
    }
    public string DisplayPlayingPosition
    {
        get { return TimeSpan.FromSeconds(PlayingTrackPosition).ToString("mm\\:ss"); }
    }

    public ICommand ChangeTrackSpeedCommand { get; }
    public ICommand ChangeTrackPitchCommand { get; }

    private float _PlayingTrackSpeed = Constants.DefaultPlayingSpeed;
    public float PlayingTrackSpeed
    {
        get => _PlayingTrackSpeed;
        set
        {
            this.RaiseAndSetIfChanged(ref _PlayingTrackSpeed, value);
            this.RaisePropertyChanged(nameof(DisplayPlayingSpeed));
        }
    }

    public string DisplayPlayingSpeed
    {
        get { return PlayingTrackSpeed.ToString() + "%"; }
    }

    private float _PlayingTrackPitch = Constants.DefaultPlayingPitch;
    public float PlayingTrackPitch
    {
        get => _PlayingTrackPitch;
        set
        {
            this.RaiseAndSetIfChanged(ref _PlayingTrackPitch, value);
            this.RaisePropertyChanged(nameof(DisplayPlayingPitch));
        }
    }
    public string DisplayPlayingPitch
    {
        get { return PlayingTrackPitch.ToString("0.0"); }
    }
    #endregion

    #region Playlist Commands
    private PlaylistService _playlistService = new();

    private void AddTracksToPlaylist(bool isPlayingTrack)
    {
        if (Playlist is null)
        {
            Playlist = new Models.Playlist();
        }
        if (isPlayingTrack)
        {
            CurrentTrackList = Playlist.AddTrack(PlayingTrack, PlayingTrackSpeed, PlayingTrackPitch);
        }
        else
        {
            CurrentTrackList = Playlist.AddTracks(SelectedTracks);
            if(PlayingTrack is null)
            {
                SetAndPlayTrack(SelectedTracks[0], null, null); 
            }
        }
    }

    private ObservableCollection<int>? _SelectedPlaylistIndexes;
    public ObservableCollection<int>? SelectedPlaylistIndexes
    {
        get => _SelectedPlaylistIndexes;
        set => this.RaiseAndSetIfChanged(ref _SelectedPlaylistIndexes, value);
    }

    private ObservableCollection<SavedPlaylist> _SavedPlaylistNames;
    public ObservableCollection<SavedPlaylist> SavedPlaylistNames
    {
        get => _SavedPlaylistNames;
        set => this.RaiseAndSetIfChanged(ref _SavedPlaylistNames, value);
    }

    private void LoadSavedPlaylists()
    {
        SavedPlaylistNames = _playlistService.GetPlaylistsWithPaths();
    }

    private SavedPlaylist? _SelectedSavedPlayist;
    public SavedPlaylist? SelectedSavedPlayist
    {
        get => _SelectedSavedPlayist;
        set => this.RaiseAndSetIfChanged(ref _SelectedSavedPlayist, value);
    }

    public ICommand SavePlaylistCommand { get; }
    private async void SavePlaylist()
    {
        if(Playlist is not null || CurrentTrackList.Count == 0)
        {
            // Pop up
            return;
        }
        if(SavedPlaylistNames.Any(savedList => savedList.Name == Playlist.Name))
        {
            // Pop up confirmation - same file path
            await Task.Run(() => { _playlistService.SavePlaylist(Playlist!); });
        }
    }
    #endregion

    public MainWindowViewModel()
    {
        RxApp.MainThreadScheduler.Schedule(() => { LoadLibrary(); LoadSavedPlaylists(); });
        
        ReloadLibrary = ReactiveCommand.Create(() =>
        {
            LoadLibrary();
        });

        DownloadYoutubeMp3DirectCommand = ReactiveCommand.Create(async () => 
        {
            await GetYoutubeAudio();
        });

        CancelCurrentDownloadCommand = ReactiveCommand.Create(() => 
        {
           if(_DownloadService is not null)
           {
                _DownloadService.cancelCurrentDownload();
           } 
        });

        AddToPlaylistCommand = ReactiveCommand.Create((bool isPlayingTrack) => 
        {
            AddTracksToPlaylist(isPlayingTrack);
            this.RaisePropertyChanged(nameof(Playlist));
            this.RaisePropertyChanged(nameof(CurrentTrackList)) ;
        });

        OpenMetadataDialogCommand = ReactiveCommand.Create(() =>
        {
            var selectedTrack = SelectedTracks[0];
            DialogTrack = (Track) selectedTrack!.Clone();
            if (selectedTrack.AlbumArt is null)
            {
                DialogTrack.SetAlbumArt(Constants.DefaultAlbumArtPath);
            }
            IsMetadataOptionEnabled = false;
            IsMetadataDialogOpen = true;
        });

        ResetMetadataDialogCommand = ReactiveCommand.Create(() =>
        {
            var selectedTrack = SelectedTracks[0];
            DialogTrack = (Track) selectedTrack!.Clone();
            if (selectedTrack.AlbumArt is null)
            {
                DialogTrack.SetAlbumArt(Constants.DefaultAlbumArtPath);
            }
            IsMetadataOptionEnabled = false;
        });

        OpenAlbumArtWindowCommand = ReactiveCommand.Create((Window parentWindow) =>
        {
            OpenAlbumArtWindow(parentWindow);
        });

        SubmitMetadataChangesCommand = ReactiveCommand.Create(() =>
        {
            SubmitMetadataChanges();
        });

        EnterNewDialogArtistCommand = ReactiveCommand.Create(() =>
        {
            if(DialogTrack!.Artists is null)
            {
                DialogTrack.Artists = [NewArtist!];
                NewArtist = null;
            }
            else
            {
                DialogTrack.Artists.Add(NewArtist!);
                NewArtist = null;
            }
        });

        RemoveDialogArtistCommand = ReactiveCommand.Create((string selectedArtist) =>
        {
            DialogTrack!.Artists!.Remove(selectedArtist);
            IsMetadataOptionEnabled = !(SelectedTracks[0]!.Equals(DialogTrack!));
        });

        SetMetadataFormActionsCommand = ReactiveCommand.Create(() =>
        {
            IsMetadataOptionEnabled = !(SelectedTracks[0]!.Equals(DialogTrack!));

            if (DialogTrack!.Year != "" && !uint.TryParse(DialogTrack!.Year, out uint _))
            {
                IsDialogYearWarningVisible = true;
            }
            else
            {
                IsDialogYearWarningVisible = false;
            }
        });

        PlaySingleTrackCommand = ReactiveCommand.Create( (Track? selectedTrack) =>
        {
            Track track = selectedTrack ?? SelectedTracks[0];
            SetAndPlayTrack(track, null, null);
        });

        PlayPauseActiveTrackCommand = ReactiveCommand.Create((bool isPlayButtonChecked) =>
        {
            PlayPauseActiveTrack(isPlayButtonChecked);
        });

        // Subscription for volume change
        this.WhenAnyValue(x => x.PlayerVolume)
            .Where(_ => PlayingTrack is not null && _Player is not null)
            .Subscribe(_ =>
                {
                    _Player!.SetVolume(PlayerVolume);
                    if(PlayerVolume != 0)
                    {
                        tempVolume = PlayerVolume;
                    }
                }
            );

        MuteButtonCommand = ReactiveCommand.Create((bool isMuteButtonChecked) =>
        {
            if (isMuteButtonChecked)
            {
                PlayerVolume = 0;
            }
            else
            {
                PlayerVolume = tempVolume;
            }
        });

        // Subscription for track seekbar change
        this.WhenAnyValue(x => x.PlayingTrackPosition)
            .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler) 
            .Where(newPosition => PlayingTrack is not null && _Player is not null &&
                   Math.Abs(newPosition - _Player.GetPosition()) > 1)
            .Subscribe(_ =>
            {
                _Player!.SeekToPosition(PlayingTrackPosition);
            });

        PlayNextCommand = ReactiveCommand.Create(() =>
        {
            PlayNextTrack();
        });

        PlayPreviousCommand = ReactiveCommand.Create(() =>
        {
            PlayPreviousTrack();
        });

        ChangeTrackSpeedCommand = ReactiveCommand.Create((bool? isIncrease) =>
        {
            if (isIncrease is null)
            {
                PlayingTrackSpeed = Constants.DefaultPlayingSpeed;
            }
            else if (isIncrease.Value)
            {
                PlayingTrackSpeed = Math.Min(150, PlayingTrackSpeed + 5);
            }
            else
            {
                PlayingTrackSpeed = Math.Max(50, PlayingTrackSpeed - 5);
            }
        });
        ChangeTrackPitchCommand = ReactiveCommand.Create((bool? isIncrease) =>
        {
            if (isIncrease is null)
            {
                PlayingTrackPitch = Constants.DefaultPlayingPitch;
            }
            else if (isIncrease.Value)
            {
                PlayingTrackPitch = Math.Min(5, PlayingTrackPitch + 0.5f);
            }
            else
            {
                PlayingTrackPitch = Math.Max(-5, PlayingTrackPitch - 0.5f);
            }
        });

        // Subscriptions for speed and pitch change
        this.WhenAnyValue(x => x.PlayingTrackSpeed)
            .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
            .Where(_ => PlayingTrack is not null && _Player is not null)
            .Subscribe(_ =>
            {
                _Player!.SetSpeed(PlayingTrackSpeed);
                if(Playlist is not null && PlayingTrack is not null && CurrentTrackList is not null)
                {
                    CurrentTrackList = [];
                    CurrentTrackList = _Playlist!.UpdatePlaylistTrack(PlayingTrack.FilePath, PlayingTrackSpeed, PlayingTrackPitch);
                }
            });

        this.WhenAnyValue(x => x.PlayingTrackPitch)
            .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
            .Where(_ => PlayingTrack is not null && _Player is not null)
            .Subscribe(_ =>
            {
                _Player!.SetPitch(PlayingTrackPitch);
                if (Playlist is not null && PlayingTrack is not null && CurrentTrackList is not null)
                {
                    CurrentTrackList = [];
                    CurrentTrackList = _Playlist!.UpdatePlaylistTrack(PlayingTrack.FilePath, PlayingTrackSpeed, PlayingTrackPitch);
                }
            });

        SavePlaylistCommand = ReactiveCommand.Create(() =>
        {
            SavePlaylist();
        });
    }
}
