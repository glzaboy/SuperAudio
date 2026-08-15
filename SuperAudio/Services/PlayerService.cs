using System;
using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media.Audio;

namespace SuperAudio.Services
{
    [SupportedOSPlatform("Windows10.0.19041.0")]
    public sealed partial class PlayerService : IDisposable
    {
        // 使用线程安全的字典：DeviceWatcher 的 Added/Removed 回调在后台线程触发，
        // 与 UI 线程的枚举（HomePageViewModel 拷贝 Values）并发访问，普通 Dictionary 会抛 InvalidOperationException。
        public ConcurrentDictionary<string, PlayerInfoItem> Devices { get; } = new();
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
            if (Devices.TryAdd(args.Id, new PlayerInfoItem { DeviceInformation = args }))
            {
                Added?.Invoke(sender, args);
            }
        }




        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            // 原子移除并清理，避免与并发的 Added / 枚举竞争
            if (Devices.TryRemove(args.Id, out var playerInfoItem))
            {
                playerInfoItem?.Dispose();
                Removed?.Invoke(sender, args);
            }
        }
        public void Dispose()
        {
            // 遍历快照释放每个连接（Dispose 内部会自行加锁，不会修改 Devices 集合）
            foreach (var item in Devices.Values)
            {
                item?.Dispose();
            }
            Devices.Clear();

            // 停止并清理 DeviceWatcher
            if (DeviceWatcher != null)
            {
                DeviceWatcher.Added -= DeviceWatcher_Added;
                DeviceWatcher.Removed -= DeviceWatcher_Removed;
                // 检查 DeviceWatcher 是否正在运行或已创建，然后停止它
                if (DeviceWatcher.Status == DeviceWatcherStatus.Started ||
                    DeviceWatcher.Status == DeviceWatcherStatus.Created)
                {
                    DeviceWatcher.Stop();
                }
            }
            Inited = false;
        }
    }
}
