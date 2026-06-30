using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SuperAudio.Helpers.SettingsHelper;
using SuperAudio.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SuperAudio.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string? Title { get; set; } = App.ResourceLoader.GetString("Main_Title");
        [ObservableProperty]

        public partial bool IsPaneToggleButtonVisible { get; set; } = true;


        [ObservableProperty]
        public partial bool IsRecording { get; set; }

        partial void OnIsRecordingChanged(bool value)
        {
            // 当录制状态变化时，通知依赖属性
            OnPropertyChanged(nameof(IsFormatSelectionEnabled));
        }
        public bool IsFormatSelectionEnabled => !IsRecording;

        private readonly LoopbackRecorder _recorder = App.Host.Services.GetRequiredService<LoopbackRecorder>();

        [RelayCommand]
        public async Task RecordAsync()
        {
            if (!IsRecording)
            {
                // 开始录制
                try
                {
                    await _recorder.StartLoopbackRecordingAsync();
                    IsRecording = true;
                    // 可选：显示通知
                }
                catch (Exception ex)
                {
                    // 处理错误
                    Debug.WriteLine($"启动录制失败: {ex.Message}");
                    // 可以重新抛出或处理
                    throw; // 或通知 UI
                }
            }
            else
            {
                // 停止录制
                try
                {
                    string musicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    string fileName = $"{App.ResourceLoader.GetString("RecordFilePrefix")}{DateTime.Now:yyyyMMdd_HHmmss}";
                    await _recorder.StopLoopbackRecordingAsync(_recorder.GetMusicFilePath(fileName), SelectedFormat.ToLowerInvariant());
                    if (IsOpenFileAfterRecording)
                    {
                        OpenFileInExplorer(_recorder.GetMusicFilePath(fileName) + "." + SelectedFormat.ToLowerInvariant());
                    }
                    IsRecording = false;
                    // 显示保存成功提示
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"启动录制失败: {ex.Message}");
                    // 可以重新抛出或处理
                    throw; // 或通知 UI
                    // 处理错误
                }
            }
        }
        private void OpenFileInExplorer(string filePath)
        {
            try
            {
                Process.Start("explorer.exe", $"/select, \"{filePath}\"");
            }
            catch (Exception ex)
            {
                // 静默处理，不影响录制流程
                Debug.WriteLine($"打开文件位置失败: {ex.Message}");
            }
        }
        [ObservableProperty]
        public partial string SelectedFormat { get; set; } = "wav";

        [ObservableProperty]
        public partial string SelectedFormatDisplay { get; set; } = "WAV";
        [ObservableProperty]
        public partial bool IsOpenFileAfterRecording { get; set; } = SettingsHelper.Current.IsOpenFileAfterRecording;
        // 当属性变化时自动保存
        partial void OnIsOpenFileAfterRecordingChanged(bool value)
        {
            SettingsHelper.Current.IsOpenFileAfterRecording = value;
        }
        [RelayCommand]
        public void SelectFormat(string format)
        {
            SelectedFormat = format;
            SelectedFormatDisplay = format.ToUpper(); ;
        }
    }
}
