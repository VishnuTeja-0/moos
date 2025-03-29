using moos.Models;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Configuration;
using moos.Services;
using System.IO;
using System.Diagnostics;
using System.Linq;
using moos.Interfaces;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;


namespace moos.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    
    
    #region Library Commands
    private Library _LocalLibrary = new();
    public Library LocalLibrary
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
    
    private void LoadLibrary()
    {
        try
        {
            LibraryDataGridSource = LocalLibrary.LoadLocalCollection(Constants.LibraryFolder);
        }
        catch(Exception ex)
        {
            //Logging and Error Display - Critical
            Console.WriteLine(ex.Message);
        }
        
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
                    LoadLibrary();
                    Track newTrack = LibraryDataGridSource.First(track => track.FilePath == downloadResult);
                    (isMetadataSuccess, newTrack) = await _DownloadService.FetchVideoMetadata(YtUrl, newTrack);
                }
            }
        }
        catch(Exception ex)
        {
            //Logging and Error Display
            Console.WriteLine(ex.Message);
        }

        if (!isDownloadSuccess)
        {
            // Logging and Error Display
            Console.WriteLine("There was an error in downloading audio from youtube: {0}", downloadResult);
        }
        if (!isMetadataSuccess)
        {
            // Logging and Error Display
            Console.WriteLine("There was an error in fetching youtube metadata: {0}", downloadResult);
        }
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
                DownloadProgress = _DownloadService.GetProgressPercentage();

                if (DownloadProgress == 0)
                {
                    _downloadTimerSubscription?.Dispose();
                    _downloadTimerSubscription = null;
                }
            });
    }

    private Track? _SelectedTrack;
    public Track? SelectedTrack
    {
        get => _SelectedTrack;
        set => this.RaiseAndSetIfChanged(ref _SelectedTrack, value);
    }
    #endregion
    
    #region Metadata Commands 
    private Track? _DialogTrack;
    public Track? DialogTrack
    {
        get => _DialogTrack;
        set => this.RaiseAndSetIfChanged(ref _DialogTrack, value);
    }

    private bool _IsLibraryButtonEnabled = false;
    public bool IsLibraryButtonEnabled
    {
        get => _IsLibraryButtonEnabled;
        set => this.RaiseAndSetIfChanged(ref _IsLibraryButtonEnabled, value); 
    }

    public ICommand EnableMetadataOptionsCommand { get; }
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

    public ICommand SubmitMetadataChangesCommand { get; }

    private async void SubmitMetadataChanges()
    {
        // Add syncing
        if (!IsDialogYearWarningVisible)
        {
            try
            {
                await Task.Run(() => { LocalLibrary.EditTrackMetadata(DialogTrack!, Constants.LibraryFolder); });
                await Task.Run(() => { LoadLibrary(); });
                SelectedTrack = DialogTrack;
                IsMetadataDialogOpen = false;
            }
            catch (Exception ex)
            {
                //Logging and Error Display
                Console.WriteLine(ex.Message);
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

    private Playlist? _Playlist;
    public Playlist? Playlist
    {
        get => _Playlist;
        set => this.RaiseAndSetIfChanged(ref _Playlist, value);
    }

    public ICommand PlaySingleTrackCommand {  get; }
    
    private IDisposable? _playbackTimerSubscription;

    private void SetAndPlayTrack(Track track)
    {
        ResetPlayback();
        InitializeTrack(track);

        if(Playlist is null || Playlist.CurrentPlaylist is null || Playlist.CurrentPlaylist.Count == 1)
        {
            Playlist = new Playlist();
            Playlist.AddTrack(track);
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
        PlayingTrack = (Track)track!.Clone();
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

        Track? track = Playlist.ReturnTrack();
        if (track is null)
        {
            return;
        }

        SetAndPlayTrack(track);
    }

    public ICommand PlayPreviousCommand { get; }

    private void PlayPreviousTrack()
    {
        if (Playlist is null || PlayingTrackPosition > 5)
        {
            PlayingTrackPosition = 0;
            return;
        }

        Track? track = Playlist.ReturnTrack(-1);
        if (track is null)
        {
            PlayingTrackPosition = 0;
            return;
        }

        SetAndPlayTrack(track);
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
        set => this.RaiseAndSetIfChanged(ref _PlayingTrackSpeed, value);
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
        get {return PlayingTrackPitch.ToString("0.0");}
    }
    #endregion

    public MainWindowViewModel()
    {
        RxApp.MainThreadScheduler.Schedule(LoadLibrary);

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

        EnableMetadataOptionsCommand = ReactiveCommand.Create(() =>
        {
            IsLibraryButtonEnabled = true;
        });

        OpenMetadataDialogCommand = ReactiveCommand.Create(() =>
        {
            DialogTrack = (Track) SelectedTrack!.Clone();
            IsMetadataOptionEnabled = false;
            IsMetadataDialogOpen = true;
        });

        ResetMetadataDialogCommand = ReactiveCommand.Create(() =>
        {
            DialogTrack = (Track) SelectedTrack!.Clone();
            IsMetadataOptionEnabled = false;
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
            IsMetadataOptionEnabled = !(SelectedTrack!.Equals(DialogTrack!));
        });

        SetMetadataFormActionsCommand = ReactiveCommand.Create(() =>
        {
            IsMetadataOptionEnabled = !(SelectedTrack!.Equals(DialogTrack!));

            if (DialogTrack!.Year != "" && !uint.TryParse(DialogTrack!.Year, out uint _))
            {
                IsDialogYearWarningVisible = true;
            }
            else
            {
                IsDialogYearWarningVisible = false;
            }
        });

        PlaySingleTrackCommand = ReactiveCommand.Create( () =>
        {
            SetAndPlayTrack(SelectedTrack!);
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
            });

        this.WhenAnyValue(x => x.PlayingTrackPitch)
            .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
            .Where(_ => PlayingTrack is not null && _Player is not null)
            .Subscribe(_ =>
            {
                _Player!.SetPitch(PlayingTrackPitch);
            });
    }
}
