
using Avalonia.Threading;
using moos.Models;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reactive.Concurrency;


namespace moos.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ICommand DownloadYoutubeMp3DirectCommand { get; }

    private int GetYoutubeVideo()
    {
        Test = YtUrl ?? _Test;

        return 1;
    }

    private async void LoadLibrary()
    {
        try
        {
            Console.WriteLine("Test");
            string libraryFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\moos";
            Console.WriteLine($"{libraryFolder}");
            await Task.Run(() => LocalLibrary.LoadLocalCollection(libraryFolder));
        }
        catch(Exception ex)
        {
            //Logging
            Console.WriteLine(ex.Message);
        }
        
    }

    private Library _LocalLibrary = new();

    public Library LocalLibrary
    {   
        get => _LocalLibrary;
        set => this.RaiseAndSetIfChanged(ref _LocalLibrary, value);            
    }

    private string? _YtUrl;
    public string? YtUrl
    {
        get => _YtUrl;
        set => this.RaiseAndSetIfChanged(ref _YtUrl, value);
    }

    private string _Test = "Hello";

    public string Test
    {
        get => _Test;
        set => this.RaiseAndSetIfChanged(ref _Test, value);
    }

    public MainWindowViewModel()
    {
        RxApp.MainThreadScheduler.Schedule(LoadLibrary);

        DownloadYoutubeMp3DirectCommand = ReactiveCommand.Create(() => 
        {
            GetYoutubeVideo();
        } );
    }
}
