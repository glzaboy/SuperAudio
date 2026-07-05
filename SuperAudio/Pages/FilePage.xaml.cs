using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using SuperAudio.ViewModels;
using System;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SuperAudio.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FilePage : Page
    {
        private FilePageViewModel ViewModel { get; }
        private string _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        public FilePage()
        {
            InitializeComponent();
            ViewModel = App.Host.Services.GetRequiredService<FilePageViewModel>();
            ViewModel.LoadDirectory(ViewModel.CurrentPath);
        }
        // 加载指定目录
        

        // 单击条目：进入文件夹 或 打开文件（可自定义）
        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not FileItem item) return;

            if (item.IsFolder)
            {
                ViewModel.LoadDirectory(item.FullPath);
            }
            else
            {
                // 音频文件操作：播放或显示信息，此处示例弹窗
                ShowInfoDialog($"选择了文件: {item.DisplayName}");
            }
        }

        // 地址栏按回车导航
        private void OnAddressBarKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string path = AddressBar.Text.Trim();
                if (Directory.Exists(path))
                    ViewModel.LoadDirectory(path);
                else
                    ShowErrorDialog("路径不存在");
            }
        }

        // 简单的对话框辅助（WinUI 3 使用 ContentDialog）
        private async void ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "错误",
                Content = message,
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void ShowInfoDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "信息",
                Content = message,
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
