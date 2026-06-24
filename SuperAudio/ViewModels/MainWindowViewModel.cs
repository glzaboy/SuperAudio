using CommunityToolkit.Mvvm.ComponentModel;

namespace SuperAudio.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string? Title { get; set; } = App.ResourceLoader.GetString("Main_Title");
        [ObservableProperty]

        public partial bool IsPaneToggleButtonVisible { get; set; } = true;


    }
}
