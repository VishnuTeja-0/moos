using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using moos.Models;
using moos.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;

namespace moos.Views.MainWindowControls.PlaylistControls;

public partial class NowPlaying : UserControl
{
    ObservableCollection<int> _selectedIndexes = new();

    public NowPlaying()
    {
        InitializeComponent();
    }

    public void SetPlaylistIndexSelection(object? source, SelectionChangedEventArgs e)
    {
        var playlist = (DataGrid)source;
        if (playlist is not null)
        {
            var vm = (MainWindowViewModel)DataContext!;
            _selectedIndexes = new ObservableCollection<int>(
                playlist.SelectedItems
                .Cast<object>()
                .OfType<PlaylistItem>()
                .Select(item => item.Id));
            vm.SelectedPlaylistIndexes = _selectedIndexes;
        }
    }
}