using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SuperAudio.Helpers;
using SuperAudio.Pages;
using SuperAudio.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SuperAudio
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public Action? NavigationViewLoaded { get; set; }
        /// <summary>
        /// 防止循环的标志位
        /// </summary>
        private bool _isUpdatingSelection = false;
        public MainWindowViewModel ViewModel { get; private set; }
        public NavigationView NavigationView
        {
            get { return NavigationViewControl; }
        }
        public MainWindow()
        {
            InitializeComponent();

            this.ViewModel = App.Host.Services.GetRequiredService<MainWindowViewModel>();
            this.RootGrid.DataContext = ViewModel;
            this.ExtendsContentIntoTitleBar = true;

        }
        private async void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            await SetWindowIconAsync();

            // We need to set the minimum size here because the XamlRoot is not available in the constructor.
            NavigationOrientationHelper.UpdateNavigationViewForElement(NavigationOrientationHelper.IsLeftMode());
        }
        private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            NavigationViewControl.IsPaneOpen = !NavigationViewControl.IsPaneOpen;
        }
        private void OnRootFrameNavigated(object sender, NavigationEventArgs e)
        {
            UpdateNavigationViewSelection();
        }
        // 同步菜单高亮的方法
        private void UpdateNavigationViewSelection()
        {
            if (_isUpdatingSelection)
            {
                return;
            }
            _isUpdatingSelection = true;
            try
            {
                Type currentPageType = rootFrame.CurrentSourcePageType;

                if (currentPageType == null) return;

                if (IsSettingPageTag(currentPageType.Name))
                {
                    NavigationViewControl.SelectedItem = NavigationViewControl.SettingsItem;
                    return;
                }

                // 根据当前页面类型找到对应的菜单项（遍历所有一级菜单及子菜单）
                NavigationViewItem targetItem = FindMenuItemByTag(currentPageType.Name);
                if (targetItem != null)
                {
                    if (NavigationViewControl.SelectedItem == null)
                    {
                        NavigationViewControl.SelectedItem = targetItem;
                    }
                    else
                    {
                        if (!NavigationViewControl.SelectedItem.Equals(targetItem))
                        {
                            NavigationViewControl.SelectedItem = targetItem;
                        }
                    }
                }
            }
            finally
            {
                _isUpdatingSelection = false;
            }
        }
        // 递归查找 NavigationViewItem（支持嵌套菜单）
        private NavigationViewItem FindMenuItemByTag(string tag)
        {
            var menuitems = NavigationViewControl.MenuItems.OfType<NavigationViewItem>();
            var MenuItem = menuitems.Where(menuItem => tag.Equals(menuItem.Tag?.ToString())).ToList();
            return MenuItem.First();
        }
        private bool IsSettingPageTag(string tag)
        {
            return tag.Equals(nameof(SettingsPage));
        }

        private void OnRootFrameNavigating(object sender, NavigatingCancelEventArgs e)
        {
            //TestContentLoadedCheckBox.IsChecked = false;
        }
        private void OnPaneDisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            if (sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
            {
                ViewModel.IsPaneToggleButtonVisible = false;
            }
            else
            {
                ViewModel.IsPaneToggleButtonVisible = true;
            }
        }
        private void OnNavigationViewControlLoaded(object sender, RoutedEventArgs e)
        {
            // Delay necessary to ensure NavigationView visual state can match navigation
            Task.Delay(500).ContinueWith(_ => this.NavigationViewLoaded?.Invoke(), TaskScheduler.FromCurrentSynchronizationContext());

            var navigationView = sender as NavigationView;
            navigationView?.RegisterPropertyChangedCallback(NavigationView.IsPaneOpenProperty, OnIsPaneOpenChanged);
        }
        private void OnIsPaneOpenChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (sender is not NavigationView navigationView)
            {
                return;
            }

            var announcementText = navigationView.IsPaneOpen ? "Navigation Pane Opened" : "Navigation Pane Closed";

            UIHelper.AnnounceActionForAccessibility(navigationView, announcementText, "NavigationViewPaneIsOpenChangeNotificationId");
        }
        // Wraps a call to rootFrame.Navigate to give the Page a way to know which NavigationRootPage is navigating.
        // Please call this function rather than rootFrame.Navigate to navigate the rootFrame.
        public void Navigate(Type pageType, object? targetPageArguments = null, NavigationTransitionInfo? navigationTransitionInfo = null)
        {
            rootFrame.Navigate(pageType, targetPageArguments, navigationTransitionInfo);

            // Ensure the NavigationView selection is set to the correct item to mark the sample's page as visited
            //if (pageType.Equals(typeof(ItemPage)) && targetPageArguments != null)
            //{
            // Mark the item sample's page visited
            //SettingsHelper.Current.UpdateRecentlyVisited(items => items.AddAsFirst(targetPageArguments.ToString() ?? "", SettingsHelper.MaxRecentlyVisitedSamples));
            //}
        }
        private void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (_isUpdatingSelection) return;
            if (args.IsSettingsSelected)
            {
                if (rootFrame.CurrentSourcePageType != typeof(SettingsPage))
                {
                    Navigate(typeof(SettingsPage));
                }
            }
            else
            {
                var selectItem = args.SelectedItemContainer;
                if (selectItem.Tag is string tag)
                {
                    if (tag == nameof(HomePage)) // 或者 "HomePage"
                    {
                        if (rootFrame.CurrentSourcePageType != typeof(HomePage))
                        {
                            Navigate(typeof(HomePage));
                        }
                    }
                }
            }
        }
        private void TitleBar_BackRequested(TitleBar sender, object args)
        {
            if (this.rootFrame.CanGoBack)
            {
                this.rootFrame.GoBack();
            }
        }
        private async Task SetWindowIconAsync()
        {
            try
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                string iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "Logo.ico");

                // 检查文件是否存在
                if (!System.IO.File.Exists(iconPath))
                {
                    System.Diagnostics.Debug.WriteLine($"图标文件不存在: {iconPath}");
                    var uri = new Uri("ms-appx:///Assets/Logo.ico");
                    var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
                    var filePath = file.Path;

                    appWindow.SetIcon(iconPath);
                    return;
                }


                // 设置图标
                appWindow.SetIcon(iconPath);

                System.Diagnostics.Debug.WriteLine($"图标设置成功: {iconPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置图标失败: {ex.Message}");
            }
        }
    }
}
