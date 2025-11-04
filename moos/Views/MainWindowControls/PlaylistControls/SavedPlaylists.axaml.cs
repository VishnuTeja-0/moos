using Avalonia.Controls;
using Avalonia.Interactivity;
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
        var check = this.FindControl<CheckBox>("CheckSelectAllPlaylists");
        if (list is not null)
        {
            var vm = (MainWindowViewModel)DataContext!;
            _selectedPlaylistItems = new ObservableCollection<SavedPlaylist>(
                list.SelectedItems
                .Cast<object>()
                .OfType<SavedPlaylist>()
                .Select(item => item));
            vm.SelectedSavedPlaylists = _selectedPlaylistItems;
            if(check is not null)
            {
                check.IsChecked = _selectedPlaylistItems.Count == vm.SavedPlaylistNames.Count;
            }
        }
    }

    public void ListSelectAllPlaylists(object? source, RoutedEventArgs e) 
    {
        var list = this.FindControl<ListBox>("ListSavedPlaylists");
        var check = (CheckBox)source;
        if(check is not null && list is not null)
        {
            if ((bool)check.IsChecked)
            {
                list.SelectAll();
            }
            else
            {
                list.UnselectAll();
            }
        }

    }
}