using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using moos.ViewModels;

namespace moos.Views.MainWindowControls;

public partial class MetadataModal : UserControl
{
    public MetadataModal()
    {
        InitializeComponent();
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

    public void CheckForDialogChanges(object source, KeyEventArgs args)
    {
        var vm = (MainWindowViewModel)DataContext!;
        vm.SetMetadataFormActionsCommand.Execute(null);
    }
}