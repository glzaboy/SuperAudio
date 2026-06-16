using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuperAudio.Services;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Windows.Media.Audio;

namespace SuperAudio.ViewModels
{
    public partial class HomePageViewModel(PlayerService playerService) : ObservableObject
    {
        [ObservableProperty]
        public partial string Title { get; set; } = $"播放";

        [ObservableProperty]
        public partial ObservableCollection<Windows.Devices.Enumeration.DeviceInformation> Devices { get; set; } = [];

        [ObservableProperty]
        public partial string? ConnectionStateText { get; private set; } = "初始化";

        [RelayCommand]
        [SupportedOSPlatform("Windows10.0.19041.0")]
        public void Init()
        {
            playerService.Init();
            playerService.Added += PlayerService_Added;
            playerService.Removed += PlayerService_Removed;
            playerService.StateChanged += PlayerService_StateChanged;
        }
        [SupportedOSPlatform("Windows10.0.19041.0")]
        private void PlayerService_StateChanged(AudioPlaybackConnection sender, object args)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                if (sender?.State == AudioPlaybackConnectionState.Closed)
                {

                    ConnectionStateText = "Disconnected";
                }
                else if (sender?.State == AudioPlaybackConnectionState.Opened)
                {
                    ConnectionStateText = "Connected";
                }
                else
                {
                    ConnectionStateText = "Unknown";
                }
            });
        }

        [SupportedOSPlatform("Windows10.0.19041.0")]
        private void PlayerService_Removed(Windows.Devices.Enumeration.DeviceWatcher sender, Windows.Devices.Enumeration.DeviceInformationUpdate args)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                Devices = [..playerService.Devices];

            });
        }
        [SupportedOSPlatform("Windows10.0.19041.0")]
        private void PlayerService_Added(Windows.Devices.Enumeration.DeviceWatcher sender, Windows.Devices.Enumeration.DeviceInformation args)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                Devices = [..playerService.Devices];

            });
        }
        [RelayCommand]
        [SupportedOSPlatform("Windows10.0.19041.0")]
        public async Task EnableAudioPlaybackConnectionAsync(string deviceId)
        {

            await playerService.EnableAudioPlaybackConnectionAsync(deviceId);
            await playerService.OpenAudioAsync(deviceId);
        }
        [RelayCommand]
        [SupportedOSPlatform("Windows10.0.19041.0")]
        public async Task ReleaseAudioPlaybackConnectionAsync(string deviceId)
        {

            await playerService.ReleaseAudioPlaybackConnectionAsync(deviceId);
            ConnectionStateText = "关闭";
        }
    }
}
