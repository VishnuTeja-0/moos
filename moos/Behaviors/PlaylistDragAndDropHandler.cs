using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactions.DragAndDrop;
using moos.Models;
using moos.ViewModels;

namespace moos.Behaviors;

public class PlaylistDragAndDropHandler : DropHandlerBase
{
    private enum DragDirection
    {
        Up,
        Down
    }

    private DragDirection _direction;

    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        return sender is DataGrid && sourceContext is PlaylistItem && (e.Source is Interactive {DataContext: PlaylistItem item }) && targetContext is MainWindowViewModel;
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        var sourceId = (sourceContext as PlaylistItem)!.Id;
        var targetId = (e.Source as Interactive)!.DataContext is PlaylistItem { Id: var id } ? id : default;
        (targetContext as MainWindowViewModel)!.ReorderPlaylistTrackCommand.Execute((sourceId, targetId));

        return true;
    }
}
