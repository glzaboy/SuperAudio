using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SuperAudio.Helps;
using System;
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
        public MainWindow()
        {
            InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
        }
        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // We need to set the minimum size here because the XamlRoot is not available in the constructor.

        }
        private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            NavigationViewControl.IsPaneOpen = !NavigationViewControl.IsPaneOpen;
        }
        private void OnRootFrameNavigated(object sender, NavigationEventArgs e)
        {
            //TestContentLoadedCheckBox.IsChecked = true;
        }

        private void OnRootFrameNavigating(object sender, NavigatingCancelEventArgs e)
        {
            //TestContentLoadedCheckBox.IsChecked = false;
        }
        private void OnPaneDisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            if (sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
            {
                titleBar.IsPaneToggleButtonVisible = false;
            }
            else
            {
                titleBar.IsPaneToggleButtonVisible = true;
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
        private void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

        }
        private void TitleBar_BackRequested(TitleBar sender, object args)
        {
            if (this.rootFrame.CanGoBack)
            {
                this.rootFrame.GoBack();
            }
        }
    }
}
