using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Globalization;
using SuperAudio.Helpers;
using SuperAudio.Helpers.SettingsHelper;
using SuperAudio.ViewModels;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
        private SettingsViewModel ViewModel { get; }
        public SettingsPage()
        {
            InitializeComponent();
            ViewModel = App.Host.Services.GetRequiredService<SettingsViewModel>();
            
        }
        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.InitCommand.Execute(sender);
        }
    }
}
