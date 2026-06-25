using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;

namespace SuperAudio.Services
{
    public partial class PlayerInfoItem : ObservableObject, IDisposable
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DisableCommand))]
        [NotifyCanExecuteChangedFor(nameof(EnableCommand))]
        public partial AudioPlaybackConnection? PlaybackConnection { get; private set; }

        public required DeviceInformation DeviceInformation { get; set; }
        //[ObservableProperty]
        public string? ConnectionStateText { get; private set; } = App.ResourceLoader.GetString("AudioPlaybackConnectionState_Disconnected");

        private readonly SemaphoreSlim _operationLock = new(1, 1);
        private bool _disposed = false;
        [SupportedOSPlatform("Windows10.0.19041.0")]
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // ✅ 先获取锁，防止操作进行中
            _operationLock.Wait();
            try
            {
                if (PlaybackConnection != null)
                {
                    PlaybackConnection.StateChanged -= PlaybackConnection_StateChanged;
                    PlaybackConnection.Dispose();
                    PlaybackConnection = null;
                }
            }
            finally
            {
                _operationLock.Release();
                _operationLock.Dispose();  // ✅ 释放信号量
            }

            GC.SuppressFinalize(this);
        }
        [SupportedOSPlatform("Windows10.0.19041.0")]
        public async Task EnableAudioPlaybackConnectionAsync()
        {
            if (PlaybackConnection == null)
            {
                ConnectionStateText = App.ResourceLoader.GetString("AudioPlaybackConnectionState_Connecting");
                App.MainWindow.DispatcherQueue.TryEnqueue(() => {
                    OnPropertyChanged(nameof(ConnectionStateText));
                });
                PlaybackConnection = AudioPlaybackConnection.TryCreateFromId(DeviceInformation.Id);
                if (PlaybackConnection != null)
                {
                    PlaybackConnection.StateChanged += PlaybackConnection_StateChanged;
                    try
                    {
                        await PlaybackConnection.StartAsync();
                    }
                    catch
                    {
                        // 启动失败，清理资源
                        PlaybackConnection.StateChanged -= PlaybackConnection_StateChanged;
                        PlaybackConnection.Dispose();
                        PlaybackConnection = null;
                        throw;
                    }

                }
            }
        }
        [SupportedOSPlatform("Windows10.0.19041.0")]
        public async Task ReleaseAudioPlaybackConnectionAsync()
        {
            if (PlaybackConnection != null)
            {
                // 关闭并释放连接
                PlaybackConnection.StateChanged -= PlaybackConnection_StateChanged;
                PlaybackConnection.Dispose();
                PlaybackConnection = null;
                ConnectionStateText = App.ResourceLoader.GetString("AudioPlaybackConnectionState_Disconnected");
                App.MainWindow.DispatcherQueue.TryEnqueue(() => {
                    OnPropertyChanged(nameof(ConnectionStateText));
                });
            }
            else
            {
                // ConnectionState.Text = "No active connection for this device.";
            }
        }
        [SupportedOSPlatform("Windows10.0.19041.0")]
        public async Task OpenAudioAsync()
        {
            if (PlaybackConnection != null)
            {
                try
                {
                    var openConnection = await PlaybackConnection.OpenAsync();
                    if ((openConnection.Status == AudioPlaybackConnectionOpenResultStatus.Success))
                    {
                    }
                    else
                    {
                        throw new InvalidOperationException($"{App.ResourceLoader.GetString("AudioPlaybackConnectionState_Error")}: {openConnection.Status}");
                    }
                }
                catch(Exception ex)
                {
                    // 启动失败，清理资源
                    PlaybackConnection.StateChanged -= PlaybackConnection_StateChanged;
                    PlaybackConnection.Dispose();
                    PlaybackConnection = null;
                    ConnectionStateText = $"{ex.Message}";
                    App.MainWindow.DispatcherQueue.TryEnqueue(() => {
                        OnPropertyChanged(nameof(ConnectionStateText));
                    });
                }
                
                
            }
        }
        [SupportedOSPlatform("Windows10.0.19041.0")]
        private void PlaybackConnection_StateChanged(AudioPlaybackConnection sender, object args)
        {
            if(sender== null)
            {
                return;
            }
            if (sender?.DeviceId == null)
            {
                return;
            }
            if (!Equals(sender.DeviceId, DeviceInformation.Id))
            {
                return;
            }
            ConnectionStateText = sender.State switch
            {
                AudioPlaybackConnectionState.Closed => App.ResourceLoader.GetString("AudioPlaybackConnectionState_Disconnected"),
                AudioPlaybackConnectionState.Opened => App.ResourceLoader.GetString("AudioPlaybackConnectionState_Connected"),
                _ => "未知"
            };
            
            App.MainWindow.DispatcherQueue.TryEnqueue(() => {
                OnPropertyChanged(nameof(ConnectionStateText));
            });
        }
        public bool CheckEnable()
        {
            return !_disposed && PlaybackConnection == null;
        }
        [RelayCommand(CanExecute = nameof(CheckEnable))]

        [SupportedOSPlatform("Windows10.0.19041.0")]
        public async Task Enable()
        {
            if (_disposed) return;
            await _operationLock.WaitAsync();
            try
            {
                await EnableAudioPlaybackConnectionAsync();
                await OpenAudioAsync();
            }
            finally
            {
                _operationLock.Release();
            }
            
        }
        public bool CheckDisable()
        {
            return !_disposed && PlaybackConnection != null;
        }
        [RelayCommand(CanExecute = nameof(CheckDisable))]

        [SupportedOSPlatform("Windows10.0.19041.0")]
        public async Task Disable()
        {
            if (_disposed) return;
            await _operationLock.WaitAsync();
            try
            {
                await ReleaseAudioPlaybackConnectionAsync();
            }
            finally
            {
                _operationLock.Release();
            }
            
        }
    }
}
