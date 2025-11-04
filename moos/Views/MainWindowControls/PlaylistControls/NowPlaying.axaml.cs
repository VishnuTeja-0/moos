using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using moos.Models;
using moos.ViewModels;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace moos.Views.MainWindowControls.PlaylistControls;

public partial class NowPlaying : UserControl
{
    ObservableCollection<int> _selectedIds = new();

    public NowPlaying()
    {
        InitializeComponent();

        var _playingGrid = this.FindControl<Grid>("GridNowPlaying");

        Observable
            .FromEventPattern<KeyEventArgs>(_playingGrid, "KeyUp")
            .Throttle(TimeSpan.FromMilliseconds(300))
            .Subscribe(e => Dispatcher.UIThread.Post(() => CheckForPlaylistChanges(e.EventArgs)));
    }

    public void SetPlaylistIndexSelection(object? source, SelectionChangedEventArgs e)
    {
        var playlist = (DataGrid)source;
        if (playlist is not null)
        {
            var vm = (MainWindowViewModel)DataContext!;
            _selectedIds = new ObservableCollection<int>(
                playlist.SelectedItems
                .Cast<object>()
                .OfType<PlaylistItem>()
                .Select(item => item.Id));
            vm.SelectedPlaylistIds = _selectedIds;
        }
    }

    public void PlaySinglePlaylistTrack(object? source, TappedEventArgs e)
    {
        var playlist = (DataGrid)source;
        var eventSource = e.Source as Interactive;
        var eventSourceName = eventSource!.Name;
        if (playlist is not null && playlist.SelectedItems.Count > 0 &&
            eventSourceName is "CellBorder" or "CellTextBlock")
        {
            var vm = (MainWindowViewModel)DataContext!;
            vm.PlayTrackByPlaylistPositionCommand.Execute(null);
        }
    }

    public void CheckForPlaylistChanges(KeyEventArgs args)
    {
        var vm = (MainWindowViewModel)DataContext!;
        vm.RaisePropertyChanged(nameof(vm.IsPlaylistChanged));
    }
}