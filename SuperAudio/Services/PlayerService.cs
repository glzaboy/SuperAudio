using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media.Audio;

namespace SuperAudio.Services
{
    [SupportedOSPlatform("Windows10.0.19041.0")]
    public sealed partial class PlayerService : IDisposable
    {
        public Dictionary<string, PlayerInfoItem> Devices { get; set; } = [];
        private DeviceWatcher? DeviceWatcher { get; set; }
        private bool Inited { get; set; } = false;

        public event TypedEventHandler<DeviceWatcher, DeviceInformation>? Added;
        public event TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>? Removed;

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
            if (!Devices.ContainsKey(args.Id))
            {
                Devices.Add(args.Id, new PlayerInfoItem { DeviceInformation = args });
                Added?.Invoke(sender, args);
            }
        }



        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            // Find the device for the given id and remove it from the list. 
            if (Devices.TryGetValue(args.Id, out var playerInfoItem))
            {
                if (playerInfoItem.PlaybackConnection != null)
                {
                    playerInfoItem.Dispose();
                }
                Devices.Remove(args.Id);
                Removed?.Invoke(sender, args);
            }
        }
        public void Dispose()
        {
            foreach (var device in Devices)
            {
                if (device.Value != null)
                {

                    device.Value.Dispose();
                    Devices.Remove(device.Key);
                }
            }
            // 在 PlayerService.Dispose 中
            foreach (var key in new List<string>(Devices.Keys))
            {
                if (Devices.TryGetValue(key, out var item))
                {
                    item?.Dispose(); // 现在会正确清理
                }
                Devices.Remove(key);
            }


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
            Inited = false;
        }
    }
}
