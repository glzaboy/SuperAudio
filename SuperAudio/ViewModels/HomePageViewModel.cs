using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuperAudio.Services;
using System.Collections.ObjectModel;

namespace SuperAudio.ViewModels
{
    public partial class HomePageViewModel(PlayerService playerService) : ObservableObject
    {
        [ObservableProperty]
        public partial string Title { get; set; } = $"播放";

        [ObservableProperty]
        public partial ObservableCollection<Windows.Devices.Enumeration.DeviceInformation> Devices { get; set; } = [];

        [RelayCommand]
        public void Init()
        {
            playerService.Init();
            playerService.Added += PlayerService_Added;
            playerService.Removed += PlayerService_Removed;
        }

        private void PlayerService_Removed(Windows.Devices.Enumeration.DeviceWatcher sender, Windows.Devices.Enumeration.DeviceInformationUpdate args)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                Devices = playerService.Devices;
                // Find the device for the given id and remove it from the list. 

            });
        }

        private void PlayerService_Added(Windows.Devices.Enumeration.DeviceWatcher sender, Windows.Devices.Enumeration.DeviceInformation args)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                Devices = playerService.Devices;
                // Find the device for the given id and remove it from the list. 

            });
        }
        [RelayCommand]
        public void OpenAudio(string deviceId)
        {

            playerService.OpenAudio(deviceId);
        }
        [RelayCommand]
        public void EnableAudioPlaybackConnection(string deviceId)
        {

            playerService.EnableAudioPlaybackConnection(deviceId);
        }
        [RelayCommand]
        public void ReleaseAudioPlaybackConnection(string deviceId)
        {

            playerService.ReleaseAudioPlaybackConnection(deviceId);
        }
    }
}
