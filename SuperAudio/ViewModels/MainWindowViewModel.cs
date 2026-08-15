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

        // 录音开始时确定的输出信息，供停止后打开文件使用（与 Start 传入 LoopbackRecorder 的一致）
        private string? _recordingOutputPath;
        private string? _recordingFormat;

        [RelayCommand]
        public async Task RecordAsync()
        {
            if (!IsRecording)
            {
                // 开始录制：先确定输出路径与格式，传给录音器，
                // 这样即便程序在录音途中退出（只走到 Dispose），也能按此名字/格式兜底保存。
                try
                {
                    string fileName = $"{App.ResourceLoader.GetString("RecordFilePrefix")}{DateTime.Now:yyyyMMdd_HHmmss}";
                    _recordingOutputPath = _recorder.GetMusicFilePath(fileName);
                    _recordingFormat = SelectedFormat.ToLowerInvariant();
                    await _recorder.StartLoopbackRecordingAsync(_recordingOutputPath, _recordingFormat);
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
                    await _recorder.StopLoopbackRecordingAsync();
                    if (IsOpenFileAfterRecording && _recordingOutputPath != null && _recordingFormat != null)
                    {
                        OpenFileInExplorer(_recordingOutputPath + "." + _recordingFormat);
                    }
                    // 显示保存成功提示
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"停止录制失败: {ex.Message}");
                    // 异常仍向上抛出，由全局异常处理通知用户
                }
                finally
                {
                    // 无论成功或失败，都必须复位录制状态，避免 UI 永久卡在"录音中"
                    IsRecording = false;
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
