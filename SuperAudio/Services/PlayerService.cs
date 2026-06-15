using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media.Audio;

namespace SuperAudio.Services
{
    public sealed partial class PlayerService : IDisposable
    {
        public ObservableCollection<DeviceInformation> Devices { get; set; } = [];
        private readonly Dictionary<string, AudioPlaybackConnection> audioPlaybackConnections = [];
        private DeviceWatcher? DeviceWatcher { get; set; }
        private bool Inited { get; set; } = false;

        public event TypedEventHandler<DeviceWatcher, DeviceInformation> Added;
        public event TypedEventHandler<DeviceWatcher, DeviceInformationUpdate> Removed;

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
            Added.Invoke(sender, args);
        }
        public async void EnableAudioPlaybackConnection(string deviceId)
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
        public void ReleaseAudioPlaybackConnection(string deviceId)
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
            throw new NotImplementedException();
        }

        public async void OpenAudio(string deviceId)
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
            Removed.Invoke(sender, args);
        }
        public void Dispose()
        {
            if (DeviceWatcher.Status == DeviceWatcherStatus.Created)
            {
                DeviceWatcher.Stop();
                Devices.Clear();
            }

        }
    }
}
