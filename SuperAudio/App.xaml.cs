using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using SuperAudio.Helpers;
using SuperAudio.Pages;
using SuperAudio.Services;
using SuperAudio.ViewModels;
using System;
using Windows.ApplicationModel.Activation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SuperAudio
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        internal static MainWindow MainWindow { get; private set; } = null!;
        internal static IHost Host { get; private set; }=null!;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            UnhandledException += HandleExceptions;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var mainInstance = AppInstance.FindOrRegisterForKey("main");

            if (!mainInstance.IsCurrent)
            {
                // Redirect the activation (and args) to the "main" instance, and exit.
                var activatedEventArgs =AppInstance.GetCurrent().GetActivatedEventArgs();
                await mainInstance.RedirectActivationToAsync(activatedEventArgs);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                return;
            }


            HostApplicationBuilder hostApplicationBuilder = new HostApplicationBuilder();
            hostApplicationBuilder.Services.AddSingleton<MainWindow>();
            hostApplicationBuilder.Services.AddSingleton<MainWindowViewModel>();
            hostApplicationBuilder.Services.AddSingleton<HomePageViewModel>();
            hostApplicationBuilder.Services.AddSingleton<PlayerService>();
            Host = hostApplicationBuilder.Build();
            Host.Start();
            MainWindow = Host.Services.GetRequiredService<MainWindow>();
            MainWindow.Closed += async (s, e) =>
            {
                if (Host!=null)
                {
                    try
                    {
                        await Host.StopAsync(TimeSpan.FromSeconds(10));
                    }
                    catch (OperationCanceledException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Host shutdown timed out ex {ex}.");
                    }
                }
            };
            WindowHelper.TrackWindow(MainWindow);
            EnsureWindow();
        }
        private async void EnsureWindow()
        {
           // await ControlInfoDataSource.Instance.GetGroupsAsync();
            //await IconsDataSource.Instance.LoadIcons();

            //MainWindow.AddNavigationMenuItems();

            ThemeHelper.Initialize();

            var targetPageType = typeof(HomePage);
            var targetPageArguments = string.Empty;
            AppActivationArguments eventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (eventArgs != null &&
                eventArgs.Kind == ExtendedActivationKind.Protocol &&
                eventArgs.Data is ProtocolActivatedEventArgs protocolArgs)
            {
                string uri = protocolArgs.Uri.LocalPath.Replace("/", "");
                targetPageArguments = uri;

                if (uri == "Settings")
                {
                    targetPageType = typeof(SettingsPage);
                }
                else if (uri == "Home")
                {
                    targetPageType = typeof(HomePage);
                }
            }
            // Handle JumpList launch arguments (passed as command-line args)
            else if (eventArgs != null &&
                eventArgs.Kind == ExtendedActivationKind.Launch &&
                eventArgs.Data is ILaunchActivatedEventArgs launchArgs &&
                !string.IsNullOrEmpty(launchArgs.Arguments))
            {
                string arg = launchArgs.Arguments.Trim();
                targetPageArguments = arg;

                
            }

            MainWindow.Navigate(targetPageType, targetPageArguments);

            if (targetPageType == typeof(HomePage))
            {
                var navItem = (NavigationViewItem)MainWindow.NavigationView.MenuItems[0];
                navItem.IsSelected = true;
            }

            // Initialize the taskbar JumpList with fixed tasks and favorites.
            //await JumpListHelper.UpdateJumpListAsync();

            // Activate the startup window.
            MainWindow.Activate();
        }
        /// <summary>
        /// Prevents the app from crashing when a exception gets thrown and notifies the user.
        /// </summary>
        /// <param name="sender">The app as an object.</param>
        /// <param name="e">Details about the exception.</param>
        private void HandleExceptions(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            if (NativeMethods.IsAppPackaged)
            {
                e.Handled = true; //Don't crash the app.

                //Create the notification.
                var notification = new AppNotificationBuilder()
                    .AddText("An exception was thrown.")
                    .AddText($"Type: {e.Exception.GetType()}")
                    .AddText($"Message: {e.Message}\r\n" +
                             $"HResult: {e.Exception.HResult}")
                    .BuildNotification();

                //Show the notification
                AppNotificationManager.Default.Show(notification);
            }
        }
    }
}
