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
using NAudio.Wave;
using Avalonia.Threading;
using System.ComponentModel;
using Avalonia.Controls;


namespace moos.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly string libraryFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\" + ConfigurationManager.AppSettings["libraryFolder"];
    private readonly string defaultAlbumArtPath = ConfigurationManager.AppSettings["defaultAlbumArtPath"];

    public ICommand DownloadYoutubeMp3DirectCommand { get; }

    private int GetYoutubeVideo()
    {
        return 0;
    }

    private void LoadLibrary()
    {
        try
        {
            LibraryDataGridSource = LocalLibrary.LoadLocalCollection(libraryFolder);
        }
        catch(Exception ex)
        {
            //Logging
            Console.WriteLine(ex.Message);
        }
        
    }

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

    private string? _YtUrl;
    public string? YtUrl
    {
        get => _YtUrl;
        set => this.RaiseAndSetIfChanged(ref _YtUrl, value);
    }

    private Track? _SelectedTrack;
    public Track? SelectedTrack
    {
        get => _SelectedTrack;
        set => this.RaiseAndSetIfChanged(ref _SelectedTrack, value);
    }

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
                await Task.Run(() => { LocalLibrary.EditTrackMetadata(DialogTrack!, libraryFolder); });
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
    private PlayerService _Player;

    private Track? _PlayingTrack;
    public Track? PlayingTrack
    {
        get => _PlayingTrack;
        set => this.RaiseAndSetIfChanged(ref _PlayingTrack, value);
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

        _Player = new PlayerService();
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
        }
    }

    private void InitializeTrack(Track track)
    {
        PlayingTrack = (Track)track!.Clone();
        if (PlayingTrack.AlbumArt is null)
        {
            PlayingTrack.SetAlbumArt(defaultAlbumArtPath);
        }
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
                if (PlayingTrackPosition > PlayingTrack?.Duration.TotalSeconds)
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
            if (Math.Floor(PlayingTrackPosition) == Math.Floor(PlayingTrack!.Duration.TotalSeconds))
            {
                PlayingTrackPosition = 0;
                _Player.PlayTrack(PlayingTrack.FilePath);
            }
            else
            {
                _Player!.ResumeTrack();
            }

            IsPlaying = true;
        }
    }

    private float _PlayerVolume = 25;
    public float PlayerVolume
    {
        get => _PlayerVolume;
        set => this.RaiseAndSetIfChanged(ref _PlayerVolume, value);
    }
    private float tempVolume = 40;

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
        get
        {
            return TimeSpan.FromSeconds(PlayingTrackPosition).ToString("mm\\:ss");
        }
    }
    #endregion

    public MainWindowViewModel()
    {
        RxApp.MainThreadScheduler.Schedule(LoadLibrary);

        DownloadYoutubeMp3DirectCommand = ReactiveCommand.Create(() => 
        {
            GetYoutubeVideo();
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

        PlaySingleTrackCommand = ReactiveCommand.Create(() =>
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
    }
}
