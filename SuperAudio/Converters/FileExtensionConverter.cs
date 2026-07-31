using Microsoft.UI.Xaml.Data;
using SuperAudio.ViewModels;
using System;
using System.IO;

namespace SuperAudio.Converters
{
    /// <summary>
    /// 将文件路径转换为大写的扩展名（不含点），如果是文件夹则返回"文件夹"。
    /// </summary>
    partial class  FileExtensionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // 如果传入的是 FileItem 对象，则取其 FullPath
            string? path = null;
            if (value is FileItem fileItem)
                path = fileItem.FullPath;
            else if (value is string str)
                path = str;

            if (string.IsNullOrEmpty(path))
                return "未知";

            try
            {
                // 检查是否为目录
                if (Directory.Exists(path))
                    return "文件夹";

                // 获取扩展名并转换为大写，去掉点
                var ext = Path.GetExtension(path);
                return string.IsNullOrEmpty(ext) ? "文件" : ext.TrimStart('.').ToUpperInvariant();
            }
            catch
            {
                return "文件";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
