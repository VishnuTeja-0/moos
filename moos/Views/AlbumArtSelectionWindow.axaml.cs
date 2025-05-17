using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using moos.ViewModels;

namespace moos.Views;

public partial class AlbumArtSelectionWindow : ReactiveWindow<AlbumArtSelectionWindowViewModel>
{
    private TaskCompletionSource<Bitmap?> _taskCompletionSource = new ();

    public AlbumArtSelectionWindow()
    {
        InitializeComponent();
        this.GetObservable(HeightProperty).Subscribe(height =>
        {
            var searchResultsPane = this.FindControl<Border>("PaneResults");
            searchResultsPane.Height = height * 0.4;
        });
    }

    public Task<Bitmap?> ShowDialogWithResult(Window window)
    {
        this.ShowDialog(window);
        return _taskCompletionSource.Task;
    }
    
    private void OnSaveButtonClick(object? sender, RoutedEventArgs e)
    {
        var vm = (AlbumArtSelectionWindowViewModel)DataContext!;
        _taskCompletionSource.TrySetResult(vm.SelectedAlbumArt);
        this.Close();
    }
    
    public void TestPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var vm = (AlbumArtSelectionWindowViewModel)DataContext!;
        Debug.WriteLine(vm.CropX + " " + vm.CropY);
    }
}