using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace SuperAudio.Pages
{
    /// <summary>
    /// 更新日志
    /// </summary>
    public sealed partial class ChangelogPage : Page
    {
        public class VersionItem
        {
            public string? VersionDisplay { get; set; }   // 如 "1.0.4 – 首页焕新"
            public string? Date { get; set; }             // "2026-08-18"
            public List<string> Features { get; set; } = [];
        }
        public ChangelogPage()
        {
            InitializeComponent();
        }
        public List<VersionItem> ChangelogData
        { get; set; } = [
                new VersionItem
                {
                    VersionDisplay = "1.0.4",
                    Date = "2026-08-06",
                    Features =
                    [
                        "增加更新日志，优化媒体库顶部按钮，修正Home按钮不生效问题"
                    ]
                },
                new VersionItem
                {
                    VersionDisplay = "1.0.3",
                    Date = "2026-08-04",
                    Features =
                    [
                        "新增媒体库：自动归集录制文件，支持内播/系统播放器，多选连续播放"
                    ]
                },
                new VersionItem
                {
                    VersionDisplay = "1.0.2",
                    Date = "2026-06-30",
                    Features =
                    [
                        "新增音频录制：接收蓝牙音频时一键录制为 MP3,WAV,AAC文件"
                    ]
                },
                new VersionItem
                {
                    VersionDisplay = "1.0.1",
                    Date = "2026-06-28",
                    Features =
                    [
                         "新增托盘图标，最小化时自动缩小到托盘"
                    ]
                },
                new VersionItem
                {
                    VersionDisplay = "1.0.0",
                    Date = "2026-06-20",
                    Features =
                    [
                        "支持手机蓝牙连接，在电脑上播放手机音频"
                    ]
                }
            ];
    }
}
