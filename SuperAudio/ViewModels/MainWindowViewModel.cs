using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SuperAudio.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string? Title { get; set; } = "Super Audio Player";

        [RelayCommand]
        private void Test()
        {

        }
    }
}
