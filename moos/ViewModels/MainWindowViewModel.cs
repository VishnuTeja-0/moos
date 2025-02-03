
using Avalonia.Threading;
using moos.Models;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reactive.Concurrency;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
//using Avalonia.Controls.Models.DataGrid;
using System.Linq.Expressions;
using System.Diagnostics;
using Avalonia.Interactivity;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;
using DynamicData;
using System.Collections.ObjectModel;
using System.Configuration;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using System.ComponentModel;
using System.IO;


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

    private void SetAndPlayTrack(Track track)
    {
        PlayingTrack = (Track)track!.Clone();
        if (PlayingTrack.AlbumArt == null)
        {
            PlayingTrack.SetAlbumArt(defaultAlbumArtPath);
        }

        if(Playlist != null)
        {
            Playlist.StopTrack();
        }

        Playlist = new Playlist();
        Playlist.AddTrack(track);
        Playlist.PlayThrough(0);
        IsPlaying = true;
    }

    public ICommand PlayPauseActiveTrackCommand { get; }
    
    private void PlayPauseActiveTrack(bool IsMuted)
    {
        if (!IsMuted)
        {
            Playlist!.PauseTrack();
            IsPlaying = false;
        }
        else
        {
            Playlist!.ResumeTrack();
            IsPlaying = true;
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
            if(DialogTrack!.Artists == null)
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

            if(DialogTrack!.Year != "" && !uint.TryParse(DialogTrack!.Year, out uint _))
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
    }
}
