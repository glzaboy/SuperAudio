using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media.Audio;

namespace SuperAudio.Services
{
    [SupportedOSPlatform("Windows10.0.19041.0")]
    public sealed partial class PlayerService : IDisposable
    {
        public Collection<DeviceInformation> Devices { get; set; } = [];
        private readonly Dictionary<string, AudioPlaybackConnection> audioPlaybackConnections = [];
        private DeviceWatcher? DeviceWatcher { get; set; }
        private bool Inited { get; set; } = false;

        public event TypedEventHandler<DeviceWatcher, DeviceInformation>? Added;
        public event TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>? Removed;
        public event TypedEventHandler<AudioPlaybackConnection, object>? StateChanged;

        public void Init()
        {
            if (Inited) return;
            Inited = true;
            DeviceWatcher = DeviceInformation.CreateWatcher(AudioPlaybackConnection.GetDeviceSelector());
            DeviceWatcher.Added += DeviceWatcher_Added;
            DeviceWatcher.Removed += DeviceWatcher_Removed;
            DeviceWatcher.Start();

        }
        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            Devices.Add(args);
            Added?.Invoke(sender, args);
        }
        public async Task EnableAudioPlaybackConnectionAsync(string deviceId)
        {
            if (!audioPlaybackConnections.ContainsKey(deviceId))
            {
                // Create the audio playback connection from the selected device id and add it to the dictionary. 
                // This will result in allowing incoming connections from the remote device. 
                var playbackConnection = AudioPlaybackConnection.TryCreateFromId(deviceId);

                if (playbackConnection != null)
                {
                    // The device has an available audio playback connection. 
                    playbackConnection.StateChanged += PlaybackConnection_StateChanged; ;
                    audioPlaybackConnections.Add(deviceId, playbackConnection);
                    await playbackConnection.StartAsync();
                    /*
                    OpenAudioPlaybackConnectionButtonButton.IsEnabled = true;*/
                }
            }
        }
        public async Task ReleaseAudioPlaybackConnectionAsync(string deviceId)
        {
            if (audioPlaybackConnections.TryGetValue(deviceId, out var connection))
            {
                // 关闭并释放连接
                connection.StateChanged -= PlaybackConnection_StateChanged;
                connection.Dispose();
                audioPlaybackConnections.Remove(deviceId);
                /*ConnectionState.Text = "Connection released.";
                OpenAudioPlaybackConnectionButtonButton.IsEnabled = false;*/
            }
            else
            {
                // ConnectionState.Text = "No active connection for this device.";
            }
        }

        private void PlaybackConnection_StateChanged(AudioPlaybackConnection sender, object args)
        {
            StateChanged?.Invoke(sender, args);
        }

        public async Task OpenAudioAsync(string deviceId)
        {
            if (audioPlaybackConnections.TryGetValue(deviceId, out var selectedConnection))
            {
                var openConnection = await selectedConnection.OpenAsync();
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
        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {

            // Find the device for the given id and remove it from the list. 
            foreach (DeviceInformation device in Devices)
            {
                if (device.Id == args.Id)
                {
                    Devices.Remove(device);
                    break;
                }
            }

            if (audioPlaybackConnections.TryGetValue(args.Id, out AudioPlaybackConnection? connectionToRemove))
            {
                connectionToRemove.Dispose();
                _ = audioPlaybackConnections.Remove(args.Id);
            }
            Removed?.Invoke(sender, args);
        }
        public void Dispose()
        {
            // 1. 释放所有音频播放连接 (无论 DeviceWatcher 状态如何)
            foreach (var connection in audioPlaybackConnections.Values)
            {
                connection.Dispose(); // 这里会触发 StateChanged 事件
            }
            audioPlaybackConnections.Clear();

            // 2. 停止并清理 DeviceWatcher
            if (DeviceWatcher != null)
            {
                DeviceWatcher.Added -= DeviceWatcher_Added;
                DeviceWatcher.Removed -= DeviceWatcher_Removed;
                // 检查 DeviceWatcher 是否正在运行或已启动，然后停止它
                if (DeviceWatcher.Status == DeviceWatcherStatus.Started ||
                    DeviceWatcher.Status == DeviceWatcherStatus.Created)
                {
                    DeviceWatcher.Stop();
                }
                Devices.Clear();
            }

        }
    }
}
