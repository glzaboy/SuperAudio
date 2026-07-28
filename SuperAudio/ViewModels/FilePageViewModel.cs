using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace SuperAudio.ViewModels
{
    public class FileItem
    {
        public string FullPath { get; set; }
        public string DisplayName { get; set; }
        public Symbol Icon { get; set; }
        public string DateModified { get; set; }
        public bool IsFolder { get; set; }

        public FileItem(FileSystemInfo info)
        {
            FullPath = info.FullName;
            DisplayName = info.Name;
            IsFolder = (info is DirectoryInfo);
            Icon = IsFolder ? Symbol.Folder : Symbol.Document;
            DateModified = info.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
        }
    }
    public partial class FilePageViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial ObservableCollection<FileItem> FileItems { get; set; }
        [ObservableProperty]
        public partial string CurrentPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        [RelayCommand]
        public void Refresh()
        {
            LoadDirectory(CurrentPath);
        }
        public void LoadDirectory(string path)
        {
            try
            {
                var dir = new DirectoryInfo(path);
                if (!dir.Exists) return;

                CurrentPath = path;

                FileItems = [];

                // 添加子文件夹
                foreach (var subDir in dir.GetDirectories())
                {
                    FileItems.Add(new(subDir));
                }

                // 添加文件（可过滤音频类型，例如只显示 .mp3/.wav等）
                foreach (var file in dir.GetFiles())
                {
                    // 若只想显示音频文件，取消注释下面筛选条件
                    // string ext = file.Extension.ToLower();
                    // if (ext == ".mp3" || ext == ".wav" || ext == ".flac" || ext == ".aac")
                    FileItems.Add(new(file));
                }
            }
            catch (Exception)
            { }
        }
        [RelayCommand]
        public void GoUp(object parameter)
        {
            var parent = Directory.GetParent(CurrentPath);
            if (parent != null)
            {
                CurrentPath = parent.FullName;
                LoadDirectory(parent.FullName);
            }

        }
    }
}
