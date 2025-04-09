using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using moos.ViewModels;

namespace moos.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        //TrackNameBounceEffect();

        this.GetObservable(HeightProperty).Subscribe(height =>
        {
            OnWindowHeightChanged(height);
        });
    }

    public void SetTrackSelection(object source, TappedEventArgs args)
    {
        var localLibraryGrid = (DataGrid)source;
        if (localLibraryGrid is not null && localLibraryGrid.SelectedItems.Count > 0)
        {
            var vm = (MainWindowViewModel)DataContext!;
            vm.EnableMetadataOptionsCommand.Execute(null);
        }
    }

    public void PlaySingleTrack(object source, TappedEventArgs args)
    {
        var localLibraryGrid = (DataGrid)source;
        var eventSource = args.Source as Interactive;
        var eventSourceName = eventSource!.Name;
        if (localLibraryGrid is not null && localLibraryGrid.SelectedItems.Count > 0 && 
            (eventSourceName == "CellBorder" || eventSourceName == "CellTextBlock"))
        {
            var vm = (MainWindowViewModel)DataContext!;
            vm.PlaySingleTrackCommand.Execute(null);
        }
    }

    public void EnterNewDialogArtist(object source, KeyEventArgs args)
    {
        var vm = (MainWindowViewModel)DataContext!;
        if (args.Key == Avalonia.Input.Key.Enter && vm.NewArtist is not null)
        {
            vm.EnterNewDialogArtistCommand.Execute(null);
        }

    }

    public void RemoveDialogArtist(object source, TappedEventArgs args)
    {
        var vm = (MainWindowViewModel)DataContext!;
        var iconSource = (PathIcon)source;
        string? selectedArtist = iconSource.DataContext as string;
        if (selectedArtist is not null)
        {
            vm.RemoveDialogArtistCommand.Execute(selectedArtist);
        }
        
    }

    public void CheckForDialogChanges(object source, KeyEventArgs args)
    {
        var vm = (MainWindowViewModel)DataContext!;
        vm.SetMetadataFormActionsCommand.Execute(null);
    }

    private void OnWindowHeightChanged(double newHeight)
    {
        DataGrid libraryGrid = this.FindControl<DataGrid>("GridLibrary");
        StackPanel librarySection = this.FindControl<StackPanel>("SectionLibrary");
        var libraryHeightValue = newHeight * 0.5;
        libraryGrid.Height = libraryHeightValue;
        librarySection.MinHeight = libraryHeightValue;
        
        DataGrid playlistGrid = this.FindControl<DataGrid>("GridPlaylist");
        StackPanel playlistSection = this.FindControl<StackPanel>("SectionLibrary");
        var playlistHeightValue = newHeight * 0.3;
        playlistGrid.Height = playlistHeightValue;
        playlistSection.MinHeight = playlistHeightValue;
    }
    
}