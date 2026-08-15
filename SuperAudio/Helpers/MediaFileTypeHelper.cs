using System;
using System.Collections.Generic;

namespace SuperAudio.Helpers
{
    /// <summary>
    /// 根据文件扩展名判断媒体引擎（Windows.Media.Playback.MediaPlayer）能够播放的媒体文件类型。
    /// 采用一份常用音视频容器扩展名基线，结果带缓存，首次调用后复用。
    /// 相比动态枚举系统解码器，这种方式简单、稳定，且在打包（MSIX）等受限环境下也不会失效。
    /// </summary>
    public static class MediaFileTypeHelper
    {
        // 媒体引擎（MF / 系统解码器）通常可播放的音频容器扩展名基线
        private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3", ".wav", ".flac", ".aac", ".m4a", ".ogg", ".wma", ".opus", ".wv", ".ape", ".mid", ".midi", ".amr"
        };

        // 媒体引擎通常可播放的视频容器扩展名基线
        private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".m4v", ".webm", ".flv", ".mpg", ".mpeg", ".ts", ".m2ts", ".3gp", ".asf"
        };

        private static HashSet<string>? _audioCache;
        private static HashSet<string>? _videoCache;

        /// <summary>获取媒体引擎支持的音频文件扩展名集合。</summary>
        public static IReadOnlySet<string> GetAudioExtensions()
        {
            return _audioCache ??= new HashSet<string>(AudioExtensions, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>获取媒体引擎支持的视频文件扩展名集合。</summary>
        public static IReadOnlySet<string> GetVideoExtensions()
        {
            return _videoCache ??= new HashSet<string>(VideoExtensions, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>获取媒体引擎支持的音频 + 视频文件扩展名集合（两者并集）。</summary>
        public static IReadOnlySet<string> GetMediaExtensions()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            set.UnionWith(GetAudioExtensions());
            set.UnionWith(GetVideoExtensions());
            return set;
        }
    }
}
