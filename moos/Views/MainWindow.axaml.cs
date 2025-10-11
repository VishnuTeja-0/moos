using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
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

        #if DEBUG
        this.AttachDevTools();
        #endif
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        this.GetObservable(HeightProperty).Subscribe(height => OnWindowHeightChanged(height));
    }

    private void OnWindowHeightChanged(double newHeight)
    {
        var librarySection = this.FindControl<StackPanel>("SectionLibrary");
        var libraryView = librarySection.FindControl<Library>("ViewLibrary");
        var libraryGrid = libraryView.FindControl<DataGrid>("GridLibrary");
        var libraryHeightValue = newHeight * 0.5;
        librarySection.MinHeight = libraryHeightValue;
        libraryGrid.Height =  libraryHeightValue;

        var playlistSection = this.FindControl<Panel>("SectionPlaylist");
        var playlistView = playlistSection.FindControl<Playlist>("ViewPlaylist");
        var playlistGrid = playlistView.FindControl<DataGrid>("GridPlaylist");
        var playlistHeightValue = newHeight * 0.3;
        playlistSection.MinHeight = playlistHeightValue;
        playlistGrid.Height = playlistHeightValue;
        
    }

    public void HandleDialogClosing(object? sender, RoutedEventArgs e)
    {
        var vm = (MainWindowViewModel)DataContext!;
        vm.ResetDialogHostCommand.Execute(null);
    }
}