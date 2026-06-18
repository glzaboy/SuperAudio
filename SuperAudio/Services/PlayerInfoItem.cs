using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media.Audio;

namespace SuperAudio.Services
{
    public partial class PlayerInfoItem: ObservableObject,IDisposable
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DisableCommand))]
        [NotifyCanExecuteChangedFor(nameof(EnableCommand))]
        public partial AudioPlaybackConnection? PlaybackConnection { get; private set; }

        public required DeviceInformation DeviceInformation { get; set; }
        [ObservableProperty]
        public partial string? ConnectionStateText { get; private set; } = "初始化";

        public event TypedEventHandler<AudioPlaybackConnection, object>? StateChanged;

        [SupportedOSPlatform("Windows10.0.19041.0")]
        public void Dispose()
        {
            if (PlaybackConnection != null)
            {
                PlaybackConnection?.Dispose();
                PlaybackConnection = null;
            }
        }
        [SupportedOSPlatform("Windows10.0.19041.0")]
        public async Task EnableAudioPlaybackConnectionAsync()
        {
            if (PlaybackConnection == null)
            {
                PlaybackConnection = AudioPlaybackConnection.TryCreateFromId(DeviceInformation.Id);
                if (PlaybackConnection != null)
                {
                    // The device has an available audio playback connection. 
                    PlaybackConnection.StateChanged += PlaybackConnection_StateChanged; 
                    await PlaybackConnection.StartAsync();
                    /*
                    OpenAudioPlaybackConnectionButtonButton.IsEnabled = true;*/
                }
            }
        }
        [SupportedOSPlatform("Windows10.0.19041.0")]
        public async Task ReleaseAudioPlaybackConnectionAsync()
        {
            if(PlaybackConnection != null) {
                // 关闭并释放连接
                PlaybackConnection.StateChanged -= PlaybackConnection_StateChanged;
                PlaybackConnection.Dispose();
                PlaybackConnection=null;
                ConnectionStateText = "断开";
            }
            else
            {
                // ConnectionState.Text = "No active connection for this device.";
            }
        }
        [SupportedOSPlatform("Windows10.0.19041.0")]
        public async Task OpenAudioAsync()
        {
            if (PlaybackConnection!=null)
            {
                var openConnection = await PlaybackConnection.OpenAsync();
                if ((openConnection.Status == AudioPlaybackConnectionOpenResultStatus.Success))
                {
                    // Notify that the AudioPlaybackConnection is connected. 
                    //ConnectionState.Text = "Connected";
                }
                else
                {
                    // Notify that the connection attempt did not succeed. 
                    //ConnectionState.Text = "Disconnected (attempt failed)";
                }
            }
        }
        [SupportedOSPlatform("Windows10.0.19041.0")]
        private void PlaybackConnection_StateChanged(AudioPlaybackConnection sender, object args)
        {
            
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                if (sender?.State == AudioPlaybackConnectionState.Closed)
                {
                    ConnectionStateText = "Disconnected";
                }
                else if (sender?.State == AudioPlaybackConnectionState.Opened)
                {
                    ConnectionStateText = "连接";
                }
                else
                {
                    ConnectionStateText = "Unknown";
                }
            });
            
            StateChanged?.Invoke(sender, args);
        }
        public bool CheckEnable()
        {
            return PlaybackConnection == null;
        }
        [RelayCommand(CanExecute =nameof(CheckEnable))]
        
        [SupportedOSPlatform("Windows10.0.19041.0")]
        public async Task Enable()
        {
            await EnableAudioPlaybackConnectionAsync();
            await OpenAudioAsync();
        }
        public bool CheckDIsable()
        {
            return PlaybackConnection != null;
        }
        [RelayCommand(CanExecute =nameof(CheckDIsable))]
        
        [SupportedOSPlatform("Windows10.0.19041.0")]
        public async Task Disable()
        {
            await ReleaseAudioPlaybackConnectionAsync();
        }
    }
}
