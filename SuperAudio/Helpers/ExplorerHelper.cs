using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SuperAudio.Helpers
{
    /// <summary>
    /// 提供 Windows 资源管理器文件选择功能的辅助类
    /// </summary>
    internal partial class ExplorerHelper
    {
        // ========== 导入 Windows Shell API ==========

        /// <summary>
        /// 根据文件路径创建 PIDL（指针 ID 列表）
        /// </summary>
        [LibraryImport("shell32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr ILCreateFromPath([MarshalAs(UnmanagedType.LPTStr)] string pszPath);

        /// <summary>
        /// 释放由 ILCreateFromPath 分配的 PIDL
        /// </summary>
        [LibraryImport("shell32.dll")]
        private static partial void ILFree(IntPtr pidl);

        /// <summary>
        /// 打开文件夹并选中指定的项目
        /// </summary>
        [LibraryImport("shell32.dll", SetLastError = true)]
        private static partial int SHOpenFolderAndSelectItems(
            IntPtr pidlFolder,
            uint cidl,
            [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl,
            uint dwFlags
        );

        // ========== 公共方法 ==========

        /// <summary>
        /// 打开指定文件夹并选中多个文件
        /// </summary>
        /// <param name="folderPath">文件夹完整路径</param>
        /// <param name="filePaths">要选中的文件或子文件夹的完整路径数组</param>
        /// <returns>成功返回 true，否则返回 false</returns>
        public static bool OpenFolderAndSelectFiles(string folderPath, params string[] filePaths)
        {
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("文件夹路径不能为空", nameof(folderPath));

            if (filePaths == null || filePaths.Length == 0)
                throw new ArgumentException("至少要指定一个文件", nameof(filePaths));

            // 1. 获取文件夹的 PIDL
            IntPtr folderPidl = ILCreateFromPath(folderPath);
            if (folderPidl == IntPtr.Zero)
            {
                throw new FileNotFoundException($"无法获取文件夹 '{folderPath}' 的 PIDL，请检查路径是否存在。");
            }

            // 2. 为每个文件创建 PIDL，并过滤掉无效的
            IntPtr[] filePidls = new IntPtr[filePaths.Length];
            int validCount = 0;
            for (int i = 0; i < filePaths.Length; i++)
            {
                string path = filePaths[i];
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    // 路径不存在则跳过（可根据需要改为抛出异常）
                    continue;
                }
                IntPtr pidl = ILCreateFromPath(path);
                if (pidl != IntPtr.Zero)
                {
                    filePidls[validCount] = pidl;
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                // 释放文件夹 PIDL
                ILFree(folderPidl);
                throw new FileNotFoundException("没有有效的文件路径可选中。");
            }

            // 调整数组大小（只保留有效的 PIDL）
            Array.Resize(ref filePidls, validCount);

            try
            {
                // 3. 调用 API 打开文件夹并选中文件
                int result = SHOpenFolderAndSelectItems(folderPidl, (uint)validCount, filePidls, 0);
                return result == 0;
            }
            finally
            {
                // 4. 释放所有 PIDL 资源（无论是否成功）
                //    注意：文件夹 PIDL 也要释放，但 SHOpenFolderAndSelectItems 不会替我们释放
                ILFree(folderPidl);

                foreach (IntPtr pidl in filePidls)
                {
                    if (pidl != IntPtr.Zero)
                        ILFree(pidl);
                }
            }
        }
    }
}
