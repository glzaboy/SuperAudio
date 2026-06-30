using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuperAudio.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SuperAudio.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string? Title { get; set; } = App.ResourceLoader.GetString("Main_Title");
        [ObservableProperty]

        public partial bool IsPaneToggleButtonVisible { get; set; } = true;
        private bool _isRecording;
        private string _recordIcon = "\uE7C8"; // 播放/开始图标
        private string _recordButtonText = "录制";

        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                SetProperty(ref _isRecording, value);
                RecordIcon = value ? "\uE783" : "\uE7C8"; // 停止图标 vs 开始图标
                RecordButtonText = value ? "停止" : "录制";
            }
        }

        public string RecordIcon
        {
            get => _recordIcon;
            set => SetProperty(ref _recordIcon, value);
        }

        public string RecordButtonText
        {
            get => _recordButtonText;
            set => SetProperty(ref _recordButtonText, value);
        }
        public ICommand RecordCommand { get; }

        private readonly LoopbackRecorder _recorder;

        public MainWindowViewModel()
        {
            _recorder = new LoopbackRecorder();
            RecordCommand = new RelayCommand(async () => await ExecuteRecordAsync());
        }

        private async Task ExecuteRecordAsync()
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
                }
            }
            else
            {
                // 停止录制
                try
                {
                    string musicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    string fileName = $"录音_{DateTime.Now:yyyyMMdd_HHmmss}.mp3";
                    await _recorder.StopLoopbackRecordingAsync(_recorder.GetMusicFilePath(fileName), "mp3");
                    IsRecording = false;
                    // 显示保存成功提示
                }
                catch (Exception ex)
                {
                    // 处理错误
                }
            }
        }
    }
}
