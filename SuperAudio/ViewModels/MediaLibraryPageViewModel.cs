using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using SuperAudio.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

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
    public partial class MediaLibraryPageViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial ObservableCollection<FileItem> FileItems { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<FileItem> SelectedFileItems { get; set; } = [];
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GoHomeCommand))]
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
        [RelayCommand(CanExecute =nameof(CanOpenFileInExplorer))]
        public void OpenFileInExplorer(object parameter)
        {
            
            ExplorerHelper.OpenFolderAndSelectFiles(CurrentPath, [..SelectedFileItems.Select(item => item.FullPath).ToList()]);
        }

        public bool CanOpenFileInExplorer()
        {
            return SelectedFileItems?.Count >= 1;
        }
        [RelayCommand(CanExecute =(nameof(CanPlayWithSystemPlayer)))]
        public void PlayWithSystemPlayer(object parameter)
        {
            if (SelectedFileItems == null || SelectedFileItems.Count == 0)
                return;

            // 生成临时 M3U 文件
            string tempDir = Path.GetTempPath();
            string tempFile = Path.Combine(tempDir, $"Playlist_{Guid.NewGuid():N}.m3u");

            try
            {
                // 写入所有文件路径，每行一个
                var lines = SelectedFileItems.Select(item => item.FullPath);
                File.WriteAllLines(tempFile, lines, Encoding.UTF8);

                // 使用系统默认关联程序打开 .m3u 文件
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });

                // 可选：延迟删除临时文件（例如 5 秒后尝试删除）
                // 但为了简单，我们可以不删除，因为临时目录会定期清理
                // 或者注册一个计时器，在进程退出后删除，但这里不处理
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"生成播放列表失败: {ex.Message}");
                // 可以提示用户
            }
        }
        public bool CanPlayWithSystemPlayer()
        {
            return SelectedFileItems?.Count >= 1;
        }
        [RelayCommand(CanExecute = (nameof(CanPlay)))]
        public void Play(object parameter)
        {
            var lines = SelectedFileItems.Select(item => item);
            App.MainWindow.OpenPlayer(lines.ToList());
        }
        public bool CanPlay()
        {
            return SelectedFileItems?.Count >= 1;
        }
        [RelayCommand(CanExecute = nameof(CanGoHome))]
        public void GoHome(object parameter)
        {
            LoadDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
        }

        public bool CanGoHome()
        {
            if (Environment.GetFolderPath(Environment.SpecialFolder.MyMusic).Equals(CurrentPath))
            {
                return false;
            }
            return true;
        }
    }
}
