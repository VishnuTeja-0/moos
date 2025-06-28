using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using moos.ViewModels;
using moos.Views.MainWindowControls;

namespace moos.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private Border? _playerBorder;
    public MainWindow()
    {
        InitializeComponent();
        this.GetObservable(HeightProperty).Subscribe(height => OnWindowHeightChanged(height));
        
        #if DEBUG
        this.AttachDevTools();
        #endif
    }

    private void OnWindowHeightChanged(double newHeight)
    {
        var library = this.FindControl<Library>("ViewLibrary");
        var libraryGrid = library.FindControl<DataGrid>("GridLibrary");
        var librarySection = this.FindControl<StackPanel>("SectionLibrary");
        var libraryHeightValue = newHeight * 0.5;
        libraryGrid.Height = libraryHeightValue;
        librarySection.MinHeight = libraryHeightValue;

        var playlist = this.FindControl<Playlist>("ViewPlaylist");
        var playlistGrid = playlist.FindControl<DataGrid>("GridPlaylist");
        var playlistSection = this.FindControl<Panel>("SectionPlaylist");
        var playlistHeightValue = newHeight * 0.3;
        playlistGrid.Height = playlistHeightValue;
        playlistSection.MinHeight = playlistHeightValue;
    }
}