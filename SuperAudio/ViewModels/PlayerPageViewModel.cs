using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace SuperAudio.ViewModels
{
    public partial class PlayerPageViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial ObservableCollection<FileItem> PlayListItems { get; set; } = [];

        [ObservableProperty]
        public partial MediaPlaybackList PlaybackList { get; set; } = new MediaPlaybackList();

        [RelayCommand]
        public void PlayWithInternalPlayer()
        {
            if (PlayListItems == null || PlayListItems.Count == 0)
                return;

            // 创建或复用实例
            MediaPlaybackList list = new();
            PlaybackList.Items.Clear();

            foreach (var file in PlayListItems)
            {
                var source = MediaSource.CreateFromUri(new System.Uri(file.FullPath));
                var item = new MediaPlaybackItem(source);
                list.Items.Add(item);
            }

            // 通过属性 setter 触发 PropertyChanged，UI 会自动刷新
            PlaybackList = list;
        }

    }
}
