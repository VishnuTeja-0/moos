using Avalonia.Controls;
using Avalonia.Input;
using moos.Models;
using moos.ViewModels;
using SoundFlow.Components;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace moos.Views.MainWindowControls;

public partial class Playlist : UserControl
{
    ObservableCollection<int> _selectedIndexes = new();

    public Playlist()
    {
        InitializeComponent();
    }

    public void SetPlaylistIndexSelection(object? source, SelectionChangedEventArgs e)
    {
        var playlist = (DataGrid)source;
        if (playlist is not null)
        {
            var vm = (MainWindowViewModel)DataContext!;
            if (playlist.SelectedItems.Count > 0)
            {
                _selectedIndexes = new ObservableCollection<int>(
                    playlist.SelectedItems
                    .Cast<object>()
                    .OfType<PlaylistItem>()
                    .Select(item => item.Id));
                
            }
            vm.SelectedPlaylistIndexes = _selectedIndexes;
        }
    }
}