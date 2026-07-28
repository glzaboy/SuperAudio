using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using SuperAudio.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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
                var animationService = ConnectedAnimationService.GetForCurrentView();
                var container = FileListView.ContainerFromItem(item) as ListViewItem;
                if (container != null)
                {
                    // 在 ListViewItem 的视觉树中查找名为 "ItemIcon" 的 SymbolIcon
                    var icon = FindVisualChild<SymbolIcon>(container, "ItemIcon");
                    if (icon != null)
                    {
                        // 准备动画：Key 使用 FullPath
                        animationService.PrepareToAnimate(item.FullPath, icon);
                    }
                }
                App.MainWindow.OpenPlayer(item);
            }
        }
        // 辅助方法：在视觉树中查找指定名称的子元素
        private T FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T element && element.Name == name)
                    return element;
                var result = FindVisualChild<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
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
        // FilePage.xaml.cs
        public void OnReturnFromPlayer(FileItem item)
        {
            try
            {
                if (item == null) return;

                var animationService = ConnectedAnimationService.GetForCurrentView();
                var animationKey = item.FullPath + "_return";
                var animation = animationService.GetAnimation(animationKey);
                if (animation == null) return;

                // 如果列表项已回收，先滚动到可视区域
                FileListView?.ScrollIntoView(item);
                var container = FileListView?.ContainerFromItem(item) as ListViewItem;
                if (container != null)
                {
                    var icon = FindVisualChild<SymbolIcon>(container, "ItemIcon");
                    if (icon != null)
                    {
                        animation.TryStart(icon);
                    }
                }
                // 等待布局更新（异步）
                /*_ = DispatcherQueue.TryEnqueue(async () =>
                {
                    await Task.Delay(50);
                    var container = FileListView?.ContainerFromItem(item) as ListViewItem;
                    if (container != null)
                    {
                        var icon = FindVisualChild<SymbolIcon>(container, "ItemIcon");
                        if (icon != null)
                        {
                            animation.TryStart(icon);
                        }
                    }
                });*/
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnReturnFromPlayer 异常: {ex}");
            }
        }
    }
}
