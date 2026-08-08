using System;
using System.Collections.Generic;
using System.IO;
using Windows.Win32;
using Windows.Win32.UI.Shell.Common;

namespace SuperAudio.Helpers
{
    /// <summary>
    /// 提供 Windows 资源管理器文件选择功能的辅助类。
    /// 封装了 Shell API（如 <see cref="SHOpenFolderAndSelectItems"/>），
    /// 用于打开指定文件夹并高亮选中一个或多个文件/子文件夹。
    /// </summary>
    /// <remarks>
    /// <para><b>作者：</b>glzaboy@163.com</para>
    /// <para><b>创建日期：</b>2026-07-31</para>
    /// <para><b>版本：</b>1.0.0</para>
    /// <para><b>公司：</b>[可选：公司或组织名称]</para>
    /// <para><b>依赖环境：</b>仅支持 Windows 操作系统（Vista 及以上），依赖 <c>shell32.dll</c>。</para>
    /// <para><b>核心职责：</b></para>
    /// <list type="bullet">
    /// <item><description>将文件系统路径转换为 Shell PIDL（指向项标识符列表的指针）。</description></item>
    /// <item><description>调用 <see cref="PInvoke.SHOpenFolderAndSelectItems"/> 打开资源管理器并选中目标。</description></item>
    /// <item><description>自动管理非托管内存（PIDL）的分配与释放，避免内存泄漏。</description></item>
    /// </list>
    /// <para><b>使用示例：</b>请参考 <see cref="OpenFolderAndSelectFiles"/> 方法的文档。</para>
    /// </remarks>
    /// <seealso cref="OpenFolderAndSelectFiles"/>
    internal partial class ExplorerHelper
    {
        /// <summary>
        /// 打开 Windows 资源管理器窗口，定位到指定的文件夹，并高亮选中其中的多个文件或子文件夹。
        /// 此方法是对 Shell API <see cref="PInvoke.SHOpenFolderAndSelectItems"/> 的封装，简化了 PIDL 的创建与释放流程。
        /// </summary>
        /// <param name="folderPath">
        /// 要打开的文件夹的完整路径（如 <c>"C:\Users\Public\Documents"</c>）。
        /// <para>
        /// <b>约束：</b>
        /// <list type="bullet">
        /// <item><description>路径必须存在，否则抛出 <see cref="FileNotFoundException"/>。</description></item>
        /// <item><description>支持长路径（需使用 <c>\\?\</c> 前缀），但本方法未特殊处理，由底层 <see cref="PInvoke.ILCreateFromPath"/> 决定。</description></item>
        /// <item><description>参数不能为 <c>null</c> 或空字符串，否则抛出 <see cref="ArgumentException"/>。</description></item>
        /// </list>
        /// </para>
        /// </param>
        /// <param name="filePaths">
        /// 一个 <see cref="string"/> 数组，指定要选中的文件或子文件夹的完整路径。
        /// 支持多个目标，至少需要提供一个有效路径。
        /// <para>
        /// <b>处理逻辑：</b>
        /// <list type="bullet">
        /// <item><description>方法会验证每个路径是否存在（使用 <see cref="File.Exists"/> 和 <see cref="Directory.Exists"/>），
        /// 不存在的路径会被自动跳过，不会导致整体失败。</description></item>
        /// <item><description>如果所有路径均无效，则抛出 <see cref="FileNotFoundException"/>。</description></item>
        /// <item><description>路径数量无硬性上限，但受限于系统资源（建议不超过数百个）。</description></item>
        /// </list>
        /// </para>
        /// </param>
        /// <returns>
        /// 如果资源管理器成功打开并选中了指定项目，则返回 <c>true</c>；
        /// 如果底层 API 调用失败（返回非零 HRESULT），则返回 <c>false</c>。
        /// 注意：即使返回 <c>false</c>，本方法也会确保所有非托管资源（PIDL）已被正确释放。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="folderPath"/> 为 <c>null</c> 或空字符串时抛出；
        /// 或当 <paramref name="filePaths"/> 为 <c>null</c> 或空数组时抛出。
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// 在以下两种情况抛出：
        /// <list type="bullet">
        /// <item><description><paramref name="folderPath"/> 不存在（无法创建文件夹的 PIDL）。</description></item>
        /// <item><description><paramref name="filePaths"/> 中所有路径均无效（没有可用的 PIDL）。</description></item>
        /// </list>
        /// </exception>
        /// <remarks>
        /// <para>
        /// <b>资源管理（关键）：</b>
        /// 本方法使用 <c>try-finally</c> 模式保证所有通过 <see cref="PInvoke.ILCreateFromPath"/> 分配的 PIDL
        /// 都会被 <see cref="ILFree"/> 释放，无论操作成功与否。调用方无需关心 PIDL 的生存期。
        /// </para>
        /// <para>
        /// <b>行为细节：</b>
        /// <list type="bullet">
        /// <item><description>如果 <paramref name="filePaths"/> 中包含与 <paramref name="folderPath"/> 相同的路径，
        /// 该路径会被视为文件夹本身并尝试选中，但资源管理器可能不会高亮显示文件夹自身（取决于系统版本）。</description></item>
        /// <item><description>如果指定的文件位于其他磁盘或网络位置，请确保路径格式正确且具有访问权限，否则对应的 PIDL 创建会失败并被跳过。</description></item>
        /// <item><description>此方法依赖于 Windows Shell，因此仅适用于 Windows 操作系统（Vista 及以上）。在非 Windows 环境（如 Linux）下调用将引发 <see cref="EntryPointNotFoundException"/>。</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// <b>性能提示：</b>
        /// 批量选中大量文件时，PIDL 的创建和释放会消耗一定资源。如果频繁调用，建议在调用方缓存路径或考虑批量处理。
        /// </para>
        /// </remarks>
        /// <example>
        /// 以下示例演示如何打开“下载”文件夹并选中两个指定的文件：
        /// <code>
        /// bool success = OpenFolderAndSelectFiles(
        ///     @"C:\Users\John\Downloads",
        ///     @"C:\Users\John\Downloads\report.pdf",
        ///     @"C:\Users\John\Downloads\image.png"
        /// );
        /// if (success)
        ///     Console.WriteLine("资源管理器已打开并选中目标文件。");
        /// else
        ///     Console.WriteLine("操作失败，请检查日志。");
        /// </code>
        /// </example>
        public static bool OpenFolderAndSelectFiles(string folderPath, params string[] filePaths)
        {
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("文件夹路径不能为空", nameof(folderPath));

            if (filePaths == null || filePaths.Length == 0)
                throw new ArgumentException("至少要指定一个文件", nameof(filePaths));

            string fullFolderPath = Path.GetFullPath(folderPath);

            // 使用 unsafe 块调用 PInvoke
            unsafe
            {
                // 获取文件夹 PIDL
                ITEMIDLIST* folderPidlPtr = PInvoke.ILCreateFromPath(fullFolderPath);
                IntPtr folderPidl = (IntPtr)folderPidlPtr;
                if (folderPidl == IntPtr.Zero)
                {
                    throw new FileNotFoundException($"无法获取文件夹 '{fullFolderPath}' 的 PIDL，请检查路径是否存在。");
                }
                var validPidls = new List<IntPtr>();
                foreach (string path in filePaths)
                {
                    string combinedPath = Path.IsPathRooted(path) ? path : Path.Combine(fullFolderPath, path);
                    string fullPath = Path.GetFullPath(combinedPath);

                    if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
                        continue;

                    ITEMIDLIST* pidlPtr = PInvoke.ILCreateFromPath(fullPath);
                    if (pidlPtr != null)
                        validPidls.Add((IntPtr)pidlPtr);
                }

                if (validPidls.Count == 0)
                {
                    PInvoke.ILFree(folderPidlPtr);
                    throw new FileNotFoundException("没有有效的文件路径可选中。");
                }

                IntPtr[] pidlArray = [.. validPidls];
                // 转换为 ITEMIDLIST* 数组
                ITEMIDLIST*[] pidlPtrs = new ITEMIDLIST*[pidlArray.Length];
                for (int i = 0; i < pidlArray.Length; i++)
                    pidlPtrs[i] = (ITEMIDLIST*)pidlArray[i];

                try
                {
                    fixed (ITEMIDLIST** ppidl = pidlPtrs)
                    {
                        int result = PInvoke.SHOpenFolderAndSelectItems(
                            (ITEMIDLIST*)folderPidl,
                            (uint)pidlPtrs.Length,
                            ppidl,
                            0
                        );
                        return result == 0;
                    }
                }
                finally
                {
                    PInvoke.ILFree(folderPidlPtr);
                    foreach (IntPtr pidl in pidlArray)
                        if (pidl != IntPtr.Zero)
                            PInvoke.ILFree((ITEMIDLIST*)pidl);
                }
            }
        }
    }
}
