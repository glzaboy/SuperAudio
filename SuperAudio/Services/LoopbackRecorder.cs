using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace SuperAudio.Services
{
    public partial class LoopbackRecorder : IDisposable
    {
        private WasapiLoopbackCapture? _loopbackCapture;
        private WaveFileWriter? _waveWriter;
        private string? _tempWavPath;
        private TaskCompletionSource<bool>? _recordingStoppedTcs;
        private bool _isRecording;

        public bool IsRecording => _isRecording;


        /// <summary>
        /// 开始环回录音（写入临时 WAV 文件，内存占用极低）
        /// </summary>
        public async Task StartLoopbackRecordingAsync()
        {
            if (_isRecording)
                throw new InvalidOperationException("录音已在进行中");

            _tempWavPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".wav");
            _recordingStoppedTcs = new TaskCompletionSource<bool>();

            _loopbackCapture = new WasapiLoopbackCapture();
            _waveWriter = new WaveFileWriter(_tempWavPath, _loopbackCapture.WaveFormat);

            _loopbackCapture.DataAvailable += (s, e) =>
            {
                _waveWriter?.Write(e.Buffer, 0, e.BytesRecorded);
            };
            _loopbackCapture.RecordingStopped += (s, e) =>
            {
                _waveWriter?.Dispose();
                _waveWriter = null;
                _loopbackCapture?.Dispose();
                _loopbackCapture = null;
                _isRecording = false;
                _recordingStoppedTcs?.TrySetResult(true);
            };

            _loopbackCapture.StartRecording();
            _isRecording = true;

            await Task.CompletedTask;
        }

        /// <summary>
        /// 停止录制，转码为目标格式并删除临时 WAV
        /// </summary>
        /// <param name="outputPath">最终输出文件完整路径</param>
        /// <param name="format">格式：wav, mp3, aac（或m4a）</param>
        public async Task StopLoopbackRecordingAsync(string outputPath, string format = "wav")
        {
            if (!_isRecording || _loopbackCapture == null)
                return;

            _loopbackCapture.StopRecording();
            await _recordingStoppedTcs!.Task;

            // 转码
            await Task.Run(() =>
            {
                using var reader = new AudioFileReader(_tempWavPath!);
                string ext = format.ToLowerInvariant();

                if (ext == "wav")
                {
                    File.Copy(_tempWavPath!, outputPath + "." + ext, overwrite: true);
                }
                else if (ext == "mp3")
                {
                    MediaFoundationEncoder.EncodeToMp3(reader, outputPath + "." + ext);
                }
                else if (ext == "aac" || ext == "m4a")
                {
                    MediaFoundationEncoder.EncodeToAac(reader, outputPath + "." + ext);
                }
                else
                {
                    throw new NotSupportedException($"不支持的格式: {format}。请使用 wav, mp3 或 aac。");
                }
            });

            // 删除临时 WAV
            if (File.Exists(_tempWavPath))
                File.Delete(_tempWavPath);
        }

        /// <summary>
        /// 获取用户的“音乐”文件夹路径（无需特殊权限）
        /// </summary>
        public static string GetMusicFolderPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        }

        /// <summary>
        /// 获取音乐库中指定文件的完整路径
        /// </summary>
        public string GetMusicFilePath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("文件名不能为空", nameof(fileName));

            string folder = GetMusicFolderPath();
            if (string.IsNullOrEmpty(folder))
                throw new InvalidOperationException("无法获取音乐库路径。");

            return Path.Combine(folder, fileName);
        }

        /// <summary>
        /// 检查文件是否存在于音乐库中，并返回 StorageFile（若存在）
        /// </summary>
        public async Task<StorageFile?> TryGetMusicFileAsync(string fileName)
        {
            string fullPath = GetMusicFilePath(fileName);
            if (!File.Exists(fullPath))
                return null;

            // 使用 StorageFile 获取（用于UWP/WinUI访问）
            return await StorageFile.GetFileFromPathAsync(fullPath);
        }

        // 释放资源
        public void Dispose()
        {
            if (_isRecording)
            {
                _loopbackCapture?.StopRecording();
                _loopbackCapture?.Dispose();
            }
            _waveWriter?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
