using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SuperAudio.Helpers;
using SuperAudio.Helpers.SettingsHelper;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.System;

namespace SuperAudio.ViewModels
{
    public partial class SettingsViewModel:ObservableObject
    {
        [RelayCommand]
        public void Init()
        {
            switch (SettingsHelper.Current.Language)
            {
                case "en-US":
                    LanageSelect = Languages.Where(d => Equals(d.Tag, "en-US")).First();
                    break;
                case "zh-CN":
                    LanageSelect = Languages.Where(d => Equals(d.Tag, "zh-CN")).First();
                    break;
                case "zh-TW":
                    LanageSelect = Languages.Where(d => Equals(d.Tag, "zh-TW")).First();
                    break;
                default:
                    LanageSelect=Languages.Where(d => Equals(d.Tag, "Auto")).First();
                    break;
            }
            if (App.MainWindow.NavigationView.PaneDisplayMode == NavigationViewPaneDisplayMode.Auto)
            {
                NavStyleSelect = NavStyles.Where(d => Equals(d.Tag, "Left")).First();
            }
            else
            {
                NavStyleSelect = NavStyles.Where(d => Equals(d.Tag, "Top")).First();
            }
        }
        #region 语言
        [ObservableProperty]
        public partial ObservableCollection<ComboBoxItem> Languages { get; set; } = [
            new ComboBoxItem(){
                Content="Auto",
                Tag="Auto"
            },
            new ComboBoxItem(){
                Content="English",
                Tag="en-US"
            },
            new ComboBoxItem(){
                Content="简体中文",
                Tag="zh-CN"
            },
            new ComboBoxItem(){
                Content="繁體中文",
                Tag="zh-TW"
            }
        ];
        private ComboBoxItem? _lanageSelect = null;

        public ComboBoxItem? LanageSelect
        {
            get => _lanageSelect;
            set
            {
                if (_lanageSelect == null)
                {
                    SetProperty(ref _lanageSelect, value);
                    return;
                }
                if (Equals(_lanageSelect, value)) return;
                if (value != null)
                {
                    var tag = value.Tag.ToString() ?? "";
                    if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride != tag)
                    {
                        switch (tag)
                        {
                            case "Auto":
                                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = string.Empty;
                                SettingsHelper.Current.Language = string.Empty;
                                break;
                            default:
                                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = tag ?? "";
                                SettingsHelper.Current.Language = tag ?? "";
                                break;
                        }
                        ContentDialog dialog = new()
                        {
                            XamlRoot = App.MainWindow.Content.XamlRoot,
                            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                            //Title = "Remove all favorites?",
                            PrimaryButtonText = App.ResourceLoader.GetString("ContentBtn_OK"),
                            CloseButtonText = App.ResourceLoader.GetString("ContentBtn_No"),
                            DefaultButton = ContentDialogButton.Primary,
                            Content = App.ResourceLoader.GetString("ReStartAlert")
                        };
                        dialog.PrimaryButtonClick += (s, args) =>
                        {
                            AppLifeHelper.RestartApplication();
                        };
                        _ = dialog.ShowAsync();

                    }
                }
            }
        }
        #endregion 语言
        #region 导航样式
        [ObservableProperty]
        public partial ObservableCollection<ComboBoxItem> NavStyles { get; set; } = [
            new ComboBoxItem(){
                Content=App.ResourceLoader.GetString("NavigationStyle_SideBar"),
                Tag="Left"
            },
            new ComboBoxItem(){
                Content=App.ResourceLoader.GetString("NavigationStyle_Top"),
                Tag="Top"
            }
        ];
        private ComboBoxItem? _navStyleSelect = null;

        public ComboBoxItem? NavStyleSelect
        {
            get => _navStyleSelect;
            set
            {
                if (_navStyleSelect == null)
                {
                    SetProperty(ref _navStyleSelect, value);
                    return;
                }
                if (Equals(_navStyleSelect, value)) return;
                if (value != null)
                {
                    var tag = value.Tag.ToString() ?? "";
                    NavigationOrientationHelper.IsLeftModeForElement(Equals( tag,"Left"));
                }
                SetProperty(ref _navStyleSelect, value);
            }
        }
        #endregion 导航样式

        [RelayCommand]
        public static void ToBugReport()
        {
            _ = Launcher.LaunchUriAsync(new Uri("https://steamsda.com/?f=super%20bluetoth%20speaker"));
        }
        
    }
}
