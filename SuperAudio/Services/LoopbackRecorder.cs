using NAudio.Wave;
using System;
using System.IO;
using System.Threading;
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

        // 保护 _waveWriter 的并发访问：捕获线程写入与 Dispose 强制收尾必须互斥，
        // 否则会在关闭程序时与 DataAvailable 回调并发写/释放导致 WAV 损坏或异常。
        private readonly Lock _writerLock = new();

        // 录音开始时由调用方传入的目标信息，供 Stop 与 Dispose 兜底保存复用，
        // 这样即使程序在录音途中退出（只走到 Dispose），也能按用户选择的名字/格式落盘。
        private string? _outputPath;
        private string? _format;

        public bool IsRecording => _isRecording;


        /// <summary>
        /// 开始环回录音（写入临时 WAV 文件，内存占用极低）
        /// </summary>
        /// <param name="outputPath">不含扩展名的完整输出路径；停止录制时会据此自动追加 .wav/.mp3/.aac 等扩展名</param>
        /// <param name="format">格式：wav, mp3, aac（或 m4a），默认 wav</param>
        public async Task StartLoopbackRecordingAsync(string outputPath, string format = "wav")
        {
            if (_isRecording)
                throw new InvalidOperationException("录音已在进行中");

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("输出路径不能为空", nameof(outputPath));

            _outputPath = outputPath;
            _format = string.IsNullOrWhiteSpace(format) ? "wav" : format.ToLowerInvariant();

            _tempWavPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".wav");
            _recordingStoppedTcs = new TaskCompletionSource<bool>();

            _loopbackCapture = new WasapiLoopbackCapture();
            _waveWriter = new WaveFileWriter(_tempWavPath, _loopbackCapture.WaveFormat);

            _loopbackCapture.DataAvailable += (s, e) =>
            {
                lock (_writerLock)
                {
                    _waveWriter?.Write(e.Buffer, 0, e.BytesRecorded);
                }
            };
            _loopbackCapture.RecordingStopped += (s, e) =>
            {
                lock (_writerLock)
                {
                    _waveWriter?.Dispose();
                    _waveWriter = null;
                }
                _loopbackCapture?.Dispose();
                _loopbackCapture = null;
                _isRecording = false;
                _recordingStoppedTcs?.TrySetResult(true);
            };

            _loopbackCapture.StartRecording();
            _isRecording = true;
        }

        /// <summary>
        /// 停止录制，转码为目标格式并删除临时 WAV。
        /// 输出路径与格式取自 <see cref="StartLoopbackRecordingAsync"/> 时传入的值。
        /// 实际工作由同步核心 <see cref="StopAndSave"/> 完成；此处用 Task.Run 包一层，
        /// 把较重的转码放到后台线程，避免阻塞 UI。
        /// </summary>
        public async Task StopLoopbackRecordingAsync()
        {
            if (!_isRecording || _loopbackCapture == null)
                return;

            await Task.Run(StopAndSave);
        }

        /// <summary>
        /// 停止录制并保存的同步核心：停止捕获、等待收尾、定稿 WAV 文件头、转码/复制到目标路径、清理临时文件。
        /// <see cref="StopLoopbackRecordingAsync"/> 与 <see cref="Dispose"/> 共用，避免逻辑重复。
        /// Stop 通过 Task.Run 在后台调用它以不阻塞 UI；Dispose 直接同步调用（释放本就同步、且退出时不应 await）。
        /// </summary>
        private void StopAndSave()
        {
            // 1) 仍在录音则请求停止并等待后台 RecordingStopped 收尾
            if (_isRecording && _loopbackCapture != null)
            {
                _loopbackCapture.StopRecording();
                try
                {
                    _recordingStoppedTcs?.Task.Wait(TimeSpan.FromSeconds(5));
                }
                catch { /* 超时或任务异常时忽略，后续兜底清理 */ }
            }
            _isRecording = false;

            // 2) 强制收尾 WAV 文件头：正常由 RecordingStopped 回调 Dispose 掉 _waveWriter，
            //    从而写入正确的 RIFF/data 长度；但若程序在录音途中被直接关闭，捕获后台线程可能
            //    来不及触发 RecordingStopped 即被拆掉，_waveWriter 仍持有未最终化的文件头，
            //    此时写出的 WAV 因长度字段为 0/错误而无法播放。这里在保存前自行 Dispose 兜底，
            //    并用锁保证不与仍在跑的 DataAvailable 写入并发。
            lock (_writerLock)
            {
                try { _waveWriter?.Dispose(); } catch { }
                _waveWriter = null;
            }

            string? tempPath = _tempWavPath;
            string? outPath = _outputPath;
            string format = _format ?? "wav";

            // 3) 若临时录音文件仍然存在，则按目标信息落盘（已在上次正常 Stop 中处理完则跳过）
            if (tempPath != null && File.Exists(tempPath))
            {
                try
                {
                    if (!string.IsNullOrEmpty(outPath))
                    {
                        try
                        {
                            SaveToOutput(tempPath, outPath, format);
                        }
                        catch
                        {
                            // 转码失败（个别格式在受限环境不可用）时退回同名 WAV 兜底
                            File.Copy(tempPath, outPath + ".wav", overwrite: true);
                        }
                    }
                    else
                    {
                        // 理论上 Start 已传入目标信息，不会走到这里；作为最后防线用通用名保存
                        string folder = GetMusicFolderPath();
                        if (!string.IsNullOrEmpty(folder))
                        {
                            string dest = Path.Combine(folder, $"SuperAudio_AutoSave_{DateTime.Now:yyyyMMdd_HHmmss}.wav");
                            File.Copy(tempPath, dest, overwrite: true);
                        }
                    }
                }
                catch { /* 保存失败也不应抛异常，避免影响程序退出 */ }
            }

            // 4) 删除临时 WAV（此时已保存或保存失败，避免泄漏到 Temp 目录）
            if (tempPath != null && File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }
            _tempWavPath = null;
        }

        /// <summary>
        /// 将临时 WAV 转码/复制到目标输出（不含扩展名）。wav 直接拷贝，mp3/aac 经 Media Foundation 转码。
        /// </summary>
        private static void SaveToOutput(string tempWavPath, string outputPath, string format)
        {
            string ext = (format ?? "wav").ToLowerInvariant();

            if (ext == "wav")
            {
                File.Copy(tempWavPath, outputPath + "." + ext, overwrite: true);
            }
            else if (ext == "mp3")
            {
                using var reader = new AudioFileReader(tempWavPath);
                MediaFoundationEncoder.EncodeToMp3(reader, outputPath + "." + ext);
            }
            else if (ext == "aac" || ext == "m4a")
            {
                using var reader = new AudioFileReader(tempWavPath);
                MediaFoundationEncoder.EncodeToAac(reader, outputPath + "." + ext);
            }
            else
            {
                throw new NotSupportedException($"不支持的格式: {format}。请使用 wav, mp3 或 aac。");
            }
            // 删除临时 WAV
            if (File.Exists(tempWavPath))
                File.Delete(tempWavPath);
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

        // 释放资源：复用与 Stop 完全相同的核心逻辑（停止→定稿 WAV 头→落盘→清临时），
        // 同步调用（释放本就同步、且程序退出时不应 await）。最后再释放捕获设备本身。
        public void Dispose()
        {
            StopAndSave();

            // 释放捕获设备（正常 Stop 时已由 RecordingStopped 回调置空，此处为安全的 no-op）
            try { _loopbackCapture?.Dispose(); } catch { }

            GC.SuppressFinalize(this);
        }
    }
}
