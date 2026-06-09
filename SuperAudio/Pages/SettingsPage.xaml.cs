using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SuperAudio.Helpers;
using SuperAudio.Helpers.SettingsHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SuperAudio.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private int lastNavigationSelectionMode = 0;
        public SettingsPage()
        {
            InitializeComponent();
        }
        private async void bugRequestCard_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/microsoft/WinUI-Gallery/issues/new/choose"));

        }
        private void toCloneRepoCard_Click(object sender, RoutedEventArgs e)
        {
            DataPackage package = new DataPackage();
            package.SetText(gitCloneTextBlock.Text);
            Clipboard.SetContent(package);
        }
        private void soundToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (soundToggle.IsOn == true)
            {
                SpatialAudioCard.IsEnabled = true;
                ElementSoundPlayer.State = ElementSoundPlayerState.On;
            }
            else
            {
                SpatialAudioCard.IsEnabled = false;
                spatialSoundBox.IsOn = false;

                ElementSoundPlayer.State = ElementSoundPlayerState.Off;
                ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.Off;
            }
        }
        private void soundPageHyperlink_Click(object sender, RoutedEventArgs e)
        {
            //App.MainWindow.Navigate(typeof(ItemPage), "Sound");
        }
        private void themeMode_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is not UIElement senderUiLement ||
                (themeMode.SelectedItem as ComboBoxItem)?.Tag.ToString() is not string selectedTheme ||
                WindowHelper.GetWindowForElement(this) is not Window window)
            {
                return;
            }

            ThemeHelper.RootTheme = EnumHelper.GetEnum<ElementTheme>(selectedTheme);
            var elementThemeResolved = ThemeHelper.RootTheme == ElementTheme.Default ? ThemeHelper.ActualTheme : ThemeHelper.RootTheme;
            TitleBarHelper.ApplySystemThemeToCaptionButtons(window, elementThemeResolved);

            // announce visual change to automation
            UIHelper.AnnounceActionForAccessibility(
                senderUiLement,
                $"Theme changed to {elementThemeResolved}",
                "ThemeChangedNotificationActivityId");
        }
        private void CheckRecentAndFavoriteButtonStates()
        {
            ClearRecentBtn.IsEnabled = SettingsHelper.Current.RecentlyVisited.Count > 0;
            UnfavoriteBtn.IsEnabled = SettingsHelper.Current.Favorites.Count > 0;
        }
        private async void ClearRecentBtn_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Clear recently visited samples?",
                PrimaryButtonText = "Clear",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                Content = "This will remove all samples from your recent history.",
                RequestedTheme = this.ActualTheme
            };
            dialog.PrimaryButtonClick += (s, args) =>
            {
                SettingsHelper.Current.UpdateRecentlyVisited(items => items.Clear());
                CheckRecentAndFavoriteButtonStates();
            };
            var result = await dialog.ShowAsync();
        }
        private async void UnfavoriteBtn_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Remove all favorites?",
                PrimaryButtonText = "Remove",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                Content = "This will unfavorite all your samples.",
                RequestedTheme = this.ActualTheme
            };
            dialog.PrimaryButtonClick += (s, args) =>
            {
                SettingsHelper.Current.UpdateFavorites(items => items.Clear());
                CheckRecentAndFavoriteButtonStates();
            };
            var result = await dialog.ShowAsync();
        }
        private void spatialSoundBox_Toggled(object sender, RoutedEventArgs e)
        {
            if (soundToggle.IsOn == true)
            {
                ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.Off;
            }
            else
            {
                ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.On;
            }
        }
        private void navigationLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Since setting the left mode does not look at the old setting we 
            // need to check if this is an actual update
            if (navigationLocation.SelectedIndex != lastNavigationSelectionMode)
            {
                NavigationOrientationHelper.IsLeftModeForElement(navigationLocation.SelectedIndex == 0);
                lastNavigationSelectionMode = navigationLocation.SelectedIndex;
            }
        }
    }
}
