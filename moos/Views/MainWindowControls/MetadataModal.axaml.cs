using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using moos.ViewModels;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
namespace moos.Views.MainWindowControls;

public partial class MetadataModal : UserControl
{
    private Grid _metadataForm;
    public MetadataModal()
    {
        InitializeComponent();

        var _modalGrid = this.FindControl<Grid>("GridMetadataForm");

        Observable
        .FromEventPattern<KeyEventArgs>(_modalGrid, "KeyUp")
        .Throttle(TimeSpan.FromMilliseconds(300))
        .Subscribe(e => Dispatcher.UIThread.Post(() => CheckForDialogChanges(e.EventArgs)));
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
        if (iconSource.DataContext is string selectedArtist)
        {
            vm.RemoveDialogArtistCommand.Execute(selectedArtist);
        }
        
    }

    public void CheckForDialogChanges(KeyEventArgs args)
    {
        var vm = (MainWindowViewModel)DataContext!;
        vm.SetMetadataFormActionsCommand.Execute(null);
    }
}