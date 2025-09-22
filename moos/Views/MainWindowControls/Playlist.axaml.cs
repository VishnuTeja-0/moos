using Avalonia.Controls;
using moos.Models;
using moos.Services;
using moos.ViewModels;
using SoundFlow.Components;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace moos.Views.MainWindowControls;

public partial class Playlist : UserControl
{
    public Playlist()
    {
        InitializeComponent();
    }

}