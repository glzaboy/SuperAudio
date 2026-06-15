using CommunityToolkit.Mvvm.ComponentModel;

namespace SuperAudio.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string? Title { get; set; } = "超级蓝牙播放器";
        [ObservableProperty]

        public partial bool IsPaneToggleButtonVisible { get; set; } = true;


    }
}
