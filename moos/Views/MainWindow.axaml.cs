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
        var localLibraryGrid = (TreeDataGrid)source;
        if (localLibraryGrid != null && localLibraryGrid.RowSelection!.Count > 0) 
        {
            var vm = (MainWindowViewModel) DataContext!;
            vm.SetTrackSelectionCommand.Execute(null);
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
}