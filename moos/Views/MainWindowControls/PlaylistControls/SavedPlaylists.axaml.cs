using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using moos.Services;
using moos.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;

namespace moos.Views.MainWindowControls.PlaylistControls;

public partial class SavedPlaylists : UserControl
{
    ObservableCollection<SavedPlaylist> _selectedPlaylistItems = new();
    public SavedPlaylists()
    {
        InitializeComponent();
    }

    public void SetSavedPlaylistSelection(object? source, SelectionChangedEventArgs e)
    {
        var list = (ListBox)source;
        if (list is not null)
        {
            var vm = (MainWindowViewModel)DataContext!;
            _selectedPlaylistItems = new ObservableCollection<SavedPlaylist>(
                list.SelectedItems
                .Cast<object>()
                .OfType<SavedPlaylist>()
                .Select(item => item));
            vm.SelectedSavedPlaylists = _selectedPlaylistItems;
        }
    }
}