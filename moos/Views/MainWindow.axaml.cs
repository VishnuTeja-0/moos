using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.Remote.Protocol.Input;
using DialogHostAvalonia;
using moos.ViewModels;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace moos.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

    }

    public void SetTrackSelection(object source, TappedEventArgs args)
    {
        var localLibraryGrid = (DataGrid)source;
        if (localLibraryGrid != null && localLibraryGrid.SelectedItems.Count > 0)
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
        if (localLibraryGrid != null && localLibraryGrid.SelectedItems.Count > 0 && 
            (eventSourceName == "CellBorder" || eventSourceName == "CellTextBlock"))
        {
            var vm = (MainWindowViewModel)DataContext!;
            vm.PlaySingleTrackCommand.Execute(null);
        }
    }

    public void EnterNewDialogArtist(object source, KeyEventArgs args)
    {
        var vm = (MainWindowViewModel)DataContext!;
        if (args.Key == Avalonia.Input.Key.Enter && vm.NewArtist != null)
        {
            
            vm.EnterNewDialogArtistCommand.Execute(null);
        }

    }

    public void RemoveDialogArtist(object source, TappedEventArgs args)
    {
        var vm = (MainWindowViewModel)DataContext!;
        var iconSource = (PathIcon)source;
        string? selectedArtist = iconSource.DataContext as string;
        if (selectedArtist != null)
        {
            vm.RemoveDialogArtistCommand.Execute(selectedArtist);
        }
        
    }

    public void CheckForDialogChanges(object source, KeyEventArgs args)
    {
        var vm = (MainWindowViewModel)DataContext!;
        vm.SetMetadataFormActionsCommand.Execute(null);
    }
}