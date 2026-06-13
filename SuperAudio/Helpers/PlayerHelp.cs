using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Media.Audio;

namespace SuperAudio.Helpers
{
    internal sealed class PlayerHelp
    {
        private Dictionary<string, AudioPlaybackConnection> audioPlaybackConnections = [];
        private ObservableCollection<Windows.Devices.Enumeration.DeviceInformation> devices = [];
    }
}
