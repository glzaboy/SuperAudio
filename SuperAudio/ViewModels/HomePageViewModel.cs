using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using SuperAudio.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace SuperAudio.ViewModels
{
    public partial class HomePageViewModel(PlayerService playerService) : ObservableObject
    {
        [ObservableProperty]
        public partial string Title { get; set; } = $"播放";

        [ObservableProperty]
        public partial ObservableCollection<PlayerInfoItem> Devices { get; set; } = [];

        [ObservableProperty]
        public partial string? ConnectionStateText { get; private set; } = "初始化";

        [RelayCommand]
        [SupportedOSPlatform("Windows10.0.19041.0")]
        public void Init()
        {
            playerService.Init();
            playerService.Added += PlayerService_Added;
            playerService.Removed += PlayerService_Removed;
        }
        

        [SupportedOSPlatform("Windows10.0.19041.0")]
        private void PlayerService_Removed(Windows.Devices.Enumeration.DeviceWatcher sender, Windows.Devices.Enumeration.DeviceInformationUpdate args)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                Devices = [..playerService.Devices.Values];

            });
        }
        [SupportedOSPlatform("Windows10.0.19041.0")]
        private void PlayerService_Added(Windows.Devices.Enumeration.DeviceWatcher sender, Windows.Devices.Enumeration.DeviceInformation args)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                Devices = [..playerService.Devices.Values];

            });
        }
        [RelayCommand]
        public static async Task OpenBluetoothSettingAsync(string deviceId)
        {
            try
            {
                ContentDialog inputDialog = new ContentDialog
                {
                    Title = "请输入你的名字",
                    Content = new TextBlock { Text = "即将弹出蓝牙功能界面，您可以连接新设备" },
                    PrimaryButtonText = "确定",
                    DefaultButton=ContentDialogButton.Primary,
                    // 关键！必须设置 XamlRoot
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };
                inputDialog.ShowAsync();
                // 优先使用Launcher（UWP环境）
                bool success = await Windows.System.Launcher.LaunchUriAsync(
                    new Uri("ms-settings:bluetooth"), new() { TreatAsUntrusted=true});

                if (!success)
                {
                    // 回退到Process方式
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "ms-settings:bluetooth",
                        UseShellExecute = true
                    });
                }
            }
            catch
            {
                // 最终回退方案
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:bluetooth",
                    UseShellExecute = true
                });
            }
        }
    }
}
