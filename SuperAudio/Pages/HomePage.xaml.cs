using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SuperAudio.ViewModels;
using Windows.Devices.Enumeration;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SuperAudio.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private HomePageViewModel ViewModel { get; }
        public HomePage()
        {
            InitializeComponent();
            ViewModel = App.Host.Services.GetRequiredService<HomePageViewModel>();

            this.DataContext = ViewModel;
        }
        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.InitCommand.Execute(this);
        }
        private async void EnableAudioPlaybackConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceListView.SelectedItem is not null)
            {
                var selectedDeviceId = (DeviceListView.SelectedItem as DeviceInformation).Id;
                ViewModel.EnableAudioPlaybackConnectionCommand.Execute(selectedDeviceId);
                /*if (!audioPlaybackConnections.ContainsKey(selectedDeviceId))
                {
                    // Create the audio playback connection from the selected device id and add it to the dictionary. 
                    // This will result in allowing incoming connections from the remote device. 
                    var playbackConnection = AudioPlaybackConnection.TryCreateFromId(selectedDeviceId);

                    if (playbackConnection != null)
                    {
                        // The device has an available audio playback connection. 
                        playbackConnection.StateChanged += PlaybackConnection_StateChanged; ;
                        audioPlaybackConnections.Add(selectedDeviceId, playbackConnection);
                        await playbackConnection.StartAsync();
                        OpenAudioPlaybackConnectionButtonButton.IsEnabled = true;
                    }
                }*/
            }
        }
        private void ReleaseAudioPlaybackConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceListView.SelectedItem is not DeviceInformation selectedDevice)
            {
                ConnectionState.Text = "No device selected to release.";
                return;
            }

            string deviceId = selectedDevice.Id;
            ViewModel.ReleaseAudioPlaybackConnectionCommand.Execute(deviceId);
            /*if (audioPlaybackConnections.TryGetValue(deviceId, out var connection))
            {
                // 关闭并释放连接
                connection.Dispose();
                audioPlaybackConnections.Remove(deviceId);
                ConnectionState.Text = "Connection released.";
                OpenAudioPlaybackConnectionButtonButton.IsEnabled = false;
            }
            else
            {
                ConnectionState.Text = "No active connection for this device.";
            }*/
        }
    }
}
