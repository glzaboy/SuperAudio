using Microsoft.UI.Xaml.Data;
using System;
using Windows.Media.Core;

namespace SuperAudio.Converters
{
    public class PathToMediaSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string path && !string.IsNullOrEmpty(path))
            {
                try
                {
                    // 转换为 Uri，本地文件需要 file:/// 前缀
                    var uri = new Uri("file:///" + path.Replace("\\", "/"));
                    return MediaSource.CreateFromUri(uri);
                }
                catch
                {
                    // 如果转换失败，返回 null
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}