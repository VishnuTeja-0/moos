using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using moos.ViewModels;

namespace moos.Views.MainWindowControls;

public partial class Library : UserControl
{
    public Library()
    {
        InitializeComponent();
    }
    
    public void SetTrackSelection(object source, TappedEventArgs args)
    {
        var localLibraryGrid = (DataGrid)source;
        if (localLibraryGrid is not null && localLibraryGrid.SelectedItems.Count > 0)
        {
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