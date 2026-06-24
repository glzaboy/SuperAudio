using System;
using System.Diagnostics;

namespace SuperAudio.Helpers
{
    internal class AppLifeHelper
    {
        /// <summary>
        /// 重启应用程序
        /// </summary>
        public static void RestartApplication()
        {
            try
            {
                bool isPackaged = NativeMethods.IsAppPackaged;

                if (isPackaged)
                {
                    // 打包模式
                    Microsoft.Windows.AppLifecycle.AppInstance.Restart(string.Empty);
                }
                else
                {
                    // 未打包模式
                    var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = exePath,
                            UseShellExecute = true
                        });
                        App.Current.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"重启失败: {ex.Message}");

                // 如果打包模式失败，尝试未打包模式
                try
                {
                    var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = exePath,
                            UseShellExecute = true
                        });
                        App.Current.Exit();
                    }
                }
                catch
                {
                    // 完全失败
                }
            }
        }
    }
}
