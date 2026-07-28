using CommunityToolkit.Mvvm.ComponentModel;

namespace SuperAudio.ViewModels
{
    public partial class PlayerPageViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial FileItem CurrentItem { get; set; }
    }
}
