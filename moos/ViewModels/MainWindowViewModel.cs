
using Avalonia.Threading;
using moos.Models;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reactive.Concurrency;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
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


namespace moos.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private string libraryFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\" + ConfigurationManager.AppSettings["libraryFolder"];
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

    public FlatTreeDataGridSource<Track> LibrarySource { get; }

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

    public ICommand SetTrackSelectionCommand { get; }
    public void SetTrackSelection()
    {
        IsLibraryButtonEnabled = true;
        SelectedTrack = LibrarySource.RowSelection?.SelectedItem;
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

    public ICommand SubmitMetadataChangesCommand { get; }
    public ICommand EnterNewDialogArtistCommand { get; }
    public ICommand RemoveDialogArtistCommand { get; }

    public MainWindowViewModel()
    {
        RxApp.MainThreadScheduler.Schedule(LoadLibrary);

        LibrarySource = new FlatTreeDataGridSource<Track>(LibraryDataGridSource)
        {
            Columns =
                {
                    new TextColumn<Track, string>("Title", Track => Track.Title),
                    new TextColumn<Track, string>("Artist", song => song.DisplayArtists),
                    new TextColumn<Track, string>("Album", song => song.Album),
                    new TextColumn<Track, uint>("Year", song => song.Year),
                    new TextColumn<Track, string>("Duration", song => song.DisplayDuration)

                }
        };

        DownloadYoutubeMp3DirectCommand = ReactiveCommand.Create(() => 
        {
            GetYoutubeVideo();
        } );

        SetTrackSelectionCommand = ReactiveCommand.Create(() =>
        {
            SetTrackSelection();
        });

        OpenMetadataDialogCommand = ReactiveCommand.Create(() =>
        {
            DialogTrack = (Track) SelectedTrack!.Clone();
            IsMetadataDialogOpen = true;
        });

        ResetMetadataDialogCommand = ReactiveCommand.Create(() =>
        {
            DialogTrack = (Track)SelectedTrack!.Clone();
        });

        SubmitMetadataChangesCommand = ReactiveCommand.Create(() =>
        {
            // Add syncing
            try
            {
                LocalLibrary.EditTrackMetadata(DialogTrack!, libraryFolder);
                LoadLibrary();
                SelectedTrack = DialogTrack;
                IsMetadataDialogOpen = false;
            }
            catch(Exception ex)
            {
                //Logging
                Console.WriteLine(ex.Message);
            }
        });

        EnterNewDialogArtistCommand = ReactiveCommand.Create(() =>
        {
            if(DialogTrack!.Artists == null)
            {
                DialogTrack.Artists = new ObservableCollection<string> { NewArtist! };
            }
            else
            {
                DialogTrack.Artists.Add(NewArtist!);
            }
        });

        RemoveDialogArtistCommand = ReactiveCommand.Create((string selectedArtist) =>
        {
            DialogTrack!.Artists!.Remove(selectedArtist);
        });

    }
}
