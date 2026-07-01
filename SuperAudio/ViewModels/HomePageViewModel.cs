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
                Devices = [.. playerService.Devices.Values];

            });
        }
        [SupportedOSPlatform("Windows10.0.19041.0")]
        private void PlayerService_Added(Windows.Devices.Enumeration.DeviceWatcher sender, Windows.Devices.Enumeration.DeviceInformation args)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                Devices = [.. playerService.Devices.Values];

            });
        }
        [RelayCommand]
        public static async Task OpenBluetoothSettingAsync(string deviceId)
        {
            try
            {
                ContentDialog inputDialog = new()
                {
                    Content = new TextBlock { Text = App.ResourceLoader.GetString("ReqBluetoothTip") },
                    PrimaryButtonText = App.ResourceLoader.GetString("ContentBtn_OK"),
                    DefaultButton = ContentDialogButton.Primary,
                    // 关键！必须设置 XamlRoot
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };
                _ = inputDialog.ShowAsync();
                // 优先使用Launcher（UWP环境）`
                bool success = await Windows.System.Launcher.LaunchUriAsync(
                    new Uri("ms-settings:bluetooth"), new() { TreatAsUntrusted = true });

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
