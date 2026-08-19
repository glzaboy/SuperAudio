using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using SuperAudio.Helpers;
using SuperAudio.Pages;
using SuperAudio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SuperAudio
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private Storyboard? _blinkStoryboard;
        public Action? NavigationViewLoaded { get; set; }
        public TrayIcon? TrayIcon { get; private set; }
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

            if (this.AppWindow.Presenter is OverlappedPresenter presenter)
            {
                // 设置最小尺寸
                presenter.PreferredMinimumWidth = 800;
                presenter.PreferredMinimumHeight = 600;

                // 你的其他设置，例如禁用最大化按钮
                //presenter.IsMaximizable = false;
                // presenter.IsMinimizable = false;
                // presenter.IsResizable = false;
            }



            ViewModel = App.Host.Services.GetRequiredService<MainWindowViewModel>();
            RootGrid.DataContext = ViewModel;
            ExtendsContentIntoTitleBar = true;
            AppWindow.Changed += async (s, e) =>
            {
                if (e.DidPresenterChange && s.Presenter is OverlappedPresenter presenter)
                {
                    if (presenter.State == OverlappedPresenterState.Minimized)
                    {
                        s.Hide();
                        AppNotification notification = new AppNotificationBuilder()
                        .AddText(App.ResourceLoader.GetString("AppHide"))
                        .SetAppLogoOverride(new Uri("ms-appx:///Assets/ControlImages/SquareLogo.png"), AppNotificationImageCrop.Circle)
                        .SetTimeStamp(DateTime.Now)
                        .SetDuration(AppNotificationDuration.Default)
                        .MuteAudio()
                        .BuildNotification();

                        AppNotificationManager.Default.Show(notification);
                    }
                }
            };

        }
        private void SetupRecordingDotAnimation()
        {
            var dot = RecordingDot;
            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(500),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(animation, dot);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);
            _blinkStoryboard = storyboard;

            ViewModel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(MainWindowViewModel.IsRecording))
                {
                    if (ViewModel.IsRecording)
                        _blinkStoryboard.Begin();
                    else
                    {
                        _blinkStoryboard.Stop();
                        dot.Opacity = 1;
                    }
                }
            };
        }
        private async void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            await SetWindowIconAsync();
            SetupRecordingDotAnimation();
            // We need to set the minimum size here because the XamlRoot is not available in the constructor.
            NavigationOrientationHelper.UpdateNavigationViewForElement(NavigationOrientationHelper.IsLeftMode());
        }
        private void TitleBar_PaneToggleRequested(Microsoft.UI.Xaml.Controls.TitleBar sender, object args)
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
                NavigationViewItem? targetItem = FindMenuItemByTag(currentPageType.Name);
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
        private NavigationViewItem? FindMenuItemByTag(string tag)
        {
            var menuitems = NavigationViewControl.MenuItems.OfType<NavigationViewItem>();
            var MenuItem = menuitems.Where(menuItem => tag.Equals(menuItem.Tag?.ToString())).ToList();
            return MenuItem.FirstOrDefault();
        }
        private bool IsSettingPageTag(string tag)
        {
            return tag.Equals(nameof(SettingsPage));
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
                    if (tag == nameof(MediaLibraryPage)) // 或者 "HomePage"
                    {
                        if (rootFrame.CurrentSourcePageType != typeof(MediaLibraryPage))
                        {
                            Navigate(typeof(MediaLibraryPage));
                        }
                    }
                    if (tag == nameof(ChangelogPage)) // 或者 "ChangelogPage"
                    {
                        if (rootFrame.CurrentSourcePageType != typeof(ChangelogPage))
                        {
                            Navigate(typeof(ChangelogPage));
                        }
                    }
                }
            }
        }
        private void TitleBar_BackRequested(Microsoft.UI.Xaml.Controls.TitleBar sender, object args)
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

                string iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "Logo.ico");

                // 检查文件是否存在
                if (!System.IO.File.Exists(iconPath))
                {
                    System.Diagnostics.Debug.WriteLine($"图标文件不存在: {iconPath}");
                    var uri = new Uri("ms-appx:///Assets/Logo.ico");
                    var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
                    iconPath = file.Path;
                }


                // 设置图标
                AppWindow.SetIcon(iconPath);
                TrayIcon = new(1, iconPath, App.ResourceLoader.GetString("Main_Title"))
                {
                    IsVisible = true
                };
                TrayIcon.Selected += (s, e) =>
                {
                    this.Restore();
                    Activate();
                };
                TrayIcon.ContextMenu += (s, e) =>
                {

                    MenuFlyout menuFlyout = new();
                    MenuFlyoutItem MenuItem_Home = new() { Text = App.ResourceLoader.GetString("Menu_Home/Content") };
                    MenuItem_Home.Click += (s, e) =>
                    {
                        this.Navigate(typeof(HomePage));
                    };
                    menuFlyout.Items.Add(MenuItem_Home);
                    MenuFlyoutItem MenuItem_MediaLibrary = new() { Text = App.ResourceLoader.GetString("Menu_MediaLibrary/Content") };
                    MenuItem_MediaLibrary.Click += (s, e) =>
                    {
                        this.Navigate(typeof(MediaLibraryPage));
                    };
                    menuFlyout.Items.Add(MenuItem_MediaLibrary);
                    MenuFlyoutItem MenuItem_ChangeLog = new() { Text = App.ResourceLoader.GetString("Menu_Changelog/Content") };
                    MenuItem_ChangeLog.Click += (s, e) =>
                    {
                        this.Navigate(typeof(ChangelogPage));
                    };
                    menuFlyout.Items.Add(MenuItem_ChangeLog);
                    menuFlyout.Items.Add(new MenuFlyoutSeparator());
                    var MenuItem_Exit = new MenuFlyoutItem() { Text = App.ResourceLoader.GetString("AppExit"), Icon = new SymbolIcon(Symbol.Clear) };
                    MenuItem_Exit.Click += (s, e) => { Close(); };
                    menuFlyout.Items.Add(MenuItem_Exit);

                    e.Flyout = menuFlyout;
                };

                System.Diagnostics.Debug.WriteLine($"图标设置成功: {iconPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置图标失败: {ex.Message}");
            }
        }
        public void OpenPlayer(List<FileItem> clickedItems)
        {
            // 1. 实例化 PlayerPage（不用导航，直接 new）
            var playerPage = new PlayerPage();
            // 2. 传数据进去（在 PlayerPage 里定义一个公开方法）
            playerPage.LoadData(clickedItems);

            // 3. 把 Page 放进覆盖层的 ContentControl 中
            PlayerContentHost.Content = playerPage;

            // 4. 显示覆盖层（全屏出现）
            PlayerOverlay.Visibility = Visibility.Visible;
        }

        private void OnClosePlayerClick(object sender, RoutedEventArgs e)
        {
            // 1. 获取当前的 PlayerPage 实例，并让它在卸载前准备返回动画
            if (PlayerContentHost.Content is PlayerPage playerPage)
            {
                // 触发 Unloaded 事件（通过清空 Content）
                // 但为了确保顺序，也可以显式调用一个方法
                // 我们直接让 Unloaded 处理准备动画，所以只需清空 Content
                // 2. 获取当前播放的文件（从 PlayerPage 的 ViewModel 中取）
                var playerViewModel = App.Host.Services.GetRequiredService<PlayerPageViewModel>();
                FileItem currentFile = playerViewModel.PlayListItems[0];
                // 2. 准备返回动画（此时元素仍在树中）
                //playerPage.PrepareReturnAnimation();

                // 3. 隐藏覆盖层并清空 Content（这会触发 PlayerPage 的 Unloaded）


                // 3. 清空 Content（这会触发 Unloaded，但 Unloaded 中不再有动画操作）
                PlayerContentHost.Content = null;

                // 4. 隐藏覆盖层
                PlayerOverlay.Visibility = Visibility.Collapsed;

                // 4. 通知 FilePage 执行返回动画（需要获取 FilePage 实例）
                if (rootFrame.Content is MediaLibraryPage filePage && currentFile != null)
                {

                    //filePage.OnReturnFromPlayer(currentFile);


                    //PlayerContentHost.Content = null;
                }
            }

        }
    }
}
