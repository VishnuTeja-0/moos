using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.ReactiveUI;
using moos.ViewModels;
using moos.Views.MainWindowControls;
using moos.Views.MainWindowControls.PlaylistControls;

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
        var librarySection = this.FindControl<StackPanel>("SectionLibrary");
        var libraryView = this.FindControl<Library>("ViewLibrary");
        var libraryGrid = libraryView.FindControl<DataGrid>("GridLibrary");
        var libraryHeightValue = newHeight * 0.5;
        libraryGrid.Height =  libraryHeightValue;
        librarySection.MinHeight = libraryHeightValue;

        var playlistSection = this.FindControl<Panel>("SectionPlaylist");
        var playlistView = this.FindControl<Playlist>("ViewPlaylist");
        var nowPlayingView = playlistView.FindControl<NowPlaying>("ViewPlaying");
        var playlistGrid = nowPlayingView.FindControl<DataGrid>("GridPlaylist");
        var playlistHeightValue = newHeight * 0.3;
        playlistGrid.Height = playlistHeightValue;
        playlistSection.MinHeight = playlistHeightValue;
    }

    public void HandleDialogClosing(object? sender, RoutedEventArgs e)
    {
        var vm = (MainWindowViewModel)DataContext!;
        vm.ResetDialogHostCommand.Execute(null);
    }
}