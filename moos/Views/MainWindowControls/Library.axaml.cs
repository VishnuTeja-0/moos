using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using moos.Models;
using moos.ViewModels;
using System.Linq;
using System.Runtime.CompilerServices;

namespace moos.Views.MainWindowControls;

public partial class Library : UserControl
{
    public Library()
    {
        InitializeComponent();
    }

    public void SetTrackSelection(object source, SelectionChangedEventArgs args)
    {
        var localLibraryGrid = (DataGrid)source;
        if (localLibraryGrid is not null && localLibraryGrid.SelectedItems.Count > 0)
        {
            var vm = (MainWindowViewModel)DataContext!;
            var _selectedTracks = new System.Collections.ObjectModel.ObservableCollection<Track>(
                localLibraryGrid.SelectedItems
                .Cast<object>()
                .OfType<Track>());
            vm.SelectedTracks = _selectedTracks;
        }
    }

    public void PlaySingleTrack(object source, TappedEventArgs args)
    {
        var localLibraryGrid = (DataGrid)source;
        var eventSource = args.Source as Interactive;
        var eventSourceName = eventSource!.Name;
        if (localLibraryGrid is not null && localLibraryGrid.SelectedItems.Count > 0 && 
            eventSourceName is "CellBorder" or "CellTextBlock")
        {
            var vm = (MainWindowViewModel)DataContext!;
            vm.PlaySingleTrackCommand.Execute(null);
        }
    }
}