using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SuperAudio.Helpers
{
    /// <summary>
    /// 提供 Windows 资源管理器文件选择功能的辅助类。
    /// 封装了 Shell API（如 <see cref="SHOpenFolderAndSelectItems"/>），
    /// 用于打开指定文件夹并高亮选中一个或多个文件/子文件夹。
    /// </summary>
    /// <remarks>
    /// <para><b>作者：</b>[填写你的姓名或团队名称]</para>
    /// <para><b>创建日期：</b>2026-07-31</para>
    /// <para><b>版本：</b>1.0.0</para>
    /// <para><b>公司：</b>[可选：公司或组织名称]</para>
    /// <para><b>依赖环境：</b>仅支持 Windows 操作系统（Vista 及以上），依赖 <c>shell32.dll</c>。</para>
    /// <para><b>核心职责：</b></para>
    /// <list type="bullet">
    /// <item><description>将文件系统路径转换为 Shell PIDL（指向项标识符列表的指针）。</description></item>
    /// <item><description>调用 <see cref="SHOpenFolderAndSelectItems"/> 打开资源管理器并选中目标。</description></item>
    /// <item><description>自动管理非托管内存（PIDL）的分配与释放，避免内存泄漏。</description></item>
    /// </list>
    /// <para><b>使用示例：</b>请参考 <see cref="OpenFolderAndSelectFiles"/> 方法的文档。</para>
    /// </remarks>
    /// <seealso cref="OpenFolderAndSelectFiles"/>
    /// <seealso cref="ILCreateFromPath"/>
    /// <seealso cref="ILFree"/>
    /// <seealso cref="SHOpenFolderAndSelectItems"/>
    internal partial class ExplorerHelper
    {
        /// <summary>
        /// 根据文件系统路径创建一个 PIDL（指向项标识符列表的指针），
        /// 该 PIDL 可用于 Windows Shell API 操作（如打开文件夹并选中项目）。
        /// </summary>
        /// <param name="pszPath">
        /// 一个 <see cref="string"/>，表示要转换的文件或文件夹的路径。
        /// <para>
        /// <b>路径格式要求：</b>
        /// <list type="bullet">
        /// <item><description>支持绝对路径（如 <c>"C:\Users\Public\file.txt"</c>）或相对路径（如 <c>".\documents"</c>），
        /// 但建议使用绝对路径以确保可靠解析。</description></item>
        /// <item><description>支持长路径（超过 <c>MAX_PATH</c> 260 个字符），但需要确保路径前缀为 <c>\\?\</c>（如 <c>"\\?\C:\VeryLongPath..."</c>），
        /// 否则可能因长度限制而失败。</description></item>
        /// <item><description>路径分隔符可使用反斜杠 <c>\</c> 或正斜杠 <c>/</c>，系统会自动转换。</description></item>
        /// <item><description>如果路径指向不存在的文件或文件夹，函数将返回 <see cref="IntPtr.Zero"/>，并可通过 <see cref="Marshal.GetLastWin32Error"/> 获取错误码。</description></item>
        /// </list>
        /// </para>
        /// </param>
        /// <returns>
        /// 返回一个 <see cref="IntPtr"/>，指向新分配的绝对 PIDL（<b>ITEMIDLIST</b> 结构）。
        /// 如果成功，调用方必须负责使用 <see cref="ILFree(IntPtr)"/> 释放该内存；
        /// 如果失败（如路径无效、系统资源不足等），返回 <see cref="IntPtr.Zero"/>，
        /// 此时可调用 <see cref="Marshal.GetLastWin32Error"/> 获取扩展错误信息。
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>重要使用须知：</b>
        /// <list type="bullet">
        /// <item>
        /// <description><b>内存所有权：</b> 该函数通过 Shell 的 COM 任务分配器分配内存，
        /// 因此必须使用 <see cref="ILFree(IntPtr)"/>（而非 <c>Marshal.FreeHGlobal</c>）释放，
        /// 以确保与分配器匹配，防止堆损坏。</description>
        /// </item>
        /// <item>
        /// <description><b>错误处理：</b> 返回 <see cref="IntPtr.Zero"/> 并不代表异常，
        /// 而是正常的错误指示。应检查返回值和 <c>GetLastError</c>（通过 <see cref="Marshal.GetLastWin32Error"/>）
        /// 来诊断具体原因（常见错误码包括 <c>ERROR_FILE_NOT_FOUND</c>、<c>ERROR_PATH_NOT_FOUND</c>、
        /// <c>ERROR_ACCESS_DENIED</c> 等）。</description>
        /// </item>
        /// <item>
        /// <description><b>字符串编组：</b> 本方法指定了 <see cref="StringMarshalling.Utf16"/>，
        /// 因此 <paramref name="pszPath"/> 作为 Unicode（UTF-16）字符串传递给底层 <c>ILCreateFromPathW</c> 函数，
        /// 支持国际字符和中文路径，无需额外转码。</description>
        /// </item>
        /// <item>
        /// <description><b>相对路径行为：</b> 如果传入相对路径，函数将基于当前进程的工作目录进行解析，
        /// 但此行为可能因应用程序环境（如 UWP 沙箱）而异，故推荐使用绝对路径。</description>
        /// </item>
        /// <item>
        /// <description><b>性能考虑：</b> 该函数会执行文件系统访问（解析路径），
        /// 频繁调用可能影响性能，建议缓存 PIDL 或重用。</description>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// 以下示例演示如何创建 PIDL，使用它调用 Shell 函数，并正确释放：
        /// <code>
        /// string filePath = @"C:\Users\Public\example.txt";
        /// IntPtr pidl = ILCreateFromPath(filePath);
        /// if (pidl == IntPtr.Zero)
        /// {
        ///     int error = Marshal.GetLastWin32Error();
        ///     throw new IOException($"无法创建 PIDL，错误码: {error}");
        /// }
        /// try
        /// {
        ///     // 使用 pidl 调用 SHOpenFolderAndSelectItems 等 API
        /// }
        /// finally
        /// {
        ///     ILFree(pidl);
        ///     pidl = IntPtr.Zero;
        /// }
        /// </code>
        /// </example>
        [LibraryImport("shell32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr ILCreateFromPath([MarshalAs(UnmanagedType.LPTStr)] string pszPath);

        /// <summary>
        /// 释放由 Shell 函数（如 <c>ILCreateFromPath</c> 或 <c>SHParseDisplayName</c>）分配的 PIDL
        /// （指向项标识符列表的指针）所占用的内存。
        /// 这是避免内存泄漏的关键清理步骤。
        /// </summary>
        /// <param name="pidl">
        /// 要释放的 <see cref="IntPtr"/>，指向一个 <b>ITEMIDLIST</b> 结构。
        /// 该指针必须是由 Shell 分配函数（如 <c>ILCreateFromPathW</c>）成功返回的 PIDL。
        /// <para>
        /// 如果传入 <see cref="IntPtr.Zero"/>，此函数通常不会执行任何操作（内部做了空指针检查），
        /// 因此可以安全地直接传入而不必额外判断非空。
        /// </para>
        /// </param>
        /// <remarks>
        /// <para>
        /// <b>重要内存管理规则：</b>
        /// <list type="bullet">
        /// <item>
        /// <description><b>来源匹配：</b> 此函数专门用于释放由 Shell 的 <c>ILCreateFromPath</c>、<c>SHParseDisplayName</c>
        /// 或 <c>SHGetDesktopFolder</c> 等函数分配的 PIDL。不要用它来释放由 <c>Marshal.AllocCoTaskMem</c> 或 <c>new IntPtr</c> 手动构造的指针。
        /// </description>
        /// </item>
        /// <item>
        /// <description><b>底层实现：</b> 在内部，<c>ILFree</c> 实际上是对 COM 任务内存分配器 <c>CoTaskMemFree</c> 的简单封装。
        /// 因此，它与 <c>Marshal.FreeCoTaskMem</c> 在功能上是等效的，但显式使用 <c>ILFree</c> 能提高代码的可读性和语义明确性。
        /// </description>
        /// </item>
        /// <item>
        /// <description><b>双重释放风险：</b> 调用 <c>ILFree</c> 后，请务必将对应的 <see cref="IntPtr"/> 变量置为 <see cref="IntPtr.Zero"/>，
        /// 以防止后续代码意外重复释放导致内存损坏（Corrupt Heap）或访问冲突异常。
        /// </description>
        /// </item>
        /// <item>
        /// <description><b>典型配对模式：</b> 通常在 <c>using</c> 块无法直接管理的非托管资源场景下，配合 <c>try-finally</c>
        /// 使用，确保在 <c>finally</c> 块中调用此函数。
        /// </description>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// 以下示例演示了如何正确分配和释放 PIDL：
        /// <code>
        /// IntPtr pidl = ILCreateFromPath(@"C:\Example\file.txt");
        /// try
        /// {
        ///     // 使用 pidl 调用 SHOpenFolderAndSelectItems 等函数
        /// }
        /// finally
        /// {
        ///     ILFree(pidl);
        ///     pidl = IntPtr.Zero; // 防止悬空指针
        /// }
        /// </code>
        /// </example>
        [LibraryImport("shell32.dll")]
        private static partial void ILFree(IntPtr pidl);

        /// <summary>
        /// 打开一个文件夹窗口，并在其中选中指定的项目（文件或子文件夹）。
        /// 该函数通过调用 Windows Shell 的 <c>SHOpenFolderAndSelectItems</c> API 实现，
        /// 常用于在资源管理器中高亮显示特定文件。
        /// </summary>
        /// <param name="pidlFolder">
        /// 一个 <see cref="IntPtr"/>，指向要打开的文件夹的 <b>ITEMIDLIST</b>（PIDL，即指向项标识符列表的指针）。
        /// 该 PIDL 必须是绝对路径，并且应通过 <c>SHParseDisplayName</c> 或 <c>ILCreateFromPath</c> 等函数获取。
        /// </param>
        /// <param name="cidl">
        /// <see cref="uint"/> 类型，指定 <paramref name="apidl"/> 数组中包含的 PIDL 数量。
        /// 如果该值为 0，则函数仅打开文件夹而不选中任何项。
        /// </param>
        /// <param name="apidl">
        /// 一个 <see cref="IntPtr"/> 数组，每个元素指向一个要选中的项目的 <b>ITEMIDLIST</b>（PIDL）。
        /// 这些 PIDL 必须是相对于 <paramref name="pidlFolder"/> 的子项标识符（即相对 PIDL）。
        /// 数组大小由 <paramref name="cidl"/> 决定。
        /// 使用 <see cref="MarshalAs(UnmanagedType.LPArray)"/> 确保托付数组正确传递给本机代码。
        /// </param>
        /// <param name="dwFlags">
        /// <see cref="uint"/> 类型，保留标志。当前必须始终设置为 0。
        /// </param>
        /// <returns>
        /// 返回 <see cref="int"/> 类型的 HRESULT 值。
        /// 如果操作成功，则返回 <c>S_OK (0)</c>；否则返回相应的错误代码（如 <c>E_FAIL</c>、<c>E_INVALIDARG</c> 等）。
        /// 建议调用 <see cref="Marshal.ThrowExceptionForHR(int)"/> 来将错误代码转换为异常，便于调试。
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>使用须知：</b>
        /// <list type="bullet">
        /// <item><description>调用前需确保 <paramref name="pidlFolder"/> 和 <paramref name="apidl"/> 中的 PIDL 均已通过 <c>CoTaskMemAlloc</c> 分配，并且最终由调用方负责释放（使用 <c>ILFree</c> 或 <c>CoTaskMemFree</c>）。</description></item>
        /// <item><description>如果只需要打开文件夹而不选中特定项，可将 <paramref name="cidl"/> 设为 0，并将 <paramref name="apidl"/> 设为 <c>null</c>。</description></item>
        /// <item><description>该函数从 Windows Vista 起可用，在较早的 Windows 版本上可能不受支持。</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        [LibraryImport("shell32.dll", SetLastError = true)]
        private static partial int SHOpenFolderAndSelectItems(
            IntPtr pidlFolder,
            uint cidl,
            [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl,
            uint dwFlags
        );

        // ========== 公共方法 ==========

        /// <summary>
        /// 打开 Windows 资源管理器窗口，定位到指定的文件夹，并高亮选中其中的多个文件或子文件夹。
        /// 此方法是对 Shell API <see cref="SHOpenFolderAndSelectItems"/> 的封装，简化了 PIDL 的创建与释放流程。
        /// </summary>
        /// <param name="folderPath">
        /// 要打开的文件夹的完整路径（如 <c>"C:\Users\Public\Documents"</c>）。
        /// <para>
        /// <b>约束：</b>
        /// <list type="bullet">
        /// <item><description>路径必须存在，否则抛出 <see cref="FileNotFoundException"/>。</description></item>
        /// <item><description>支持长路径（需使用 <c>\\?\</c> 前缀），但本方法未特殊处理，由底层 <see cref="ILCreateFromPath"/> 决定。</description></item>
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
        /// 本方法使用 <c>try-finally</c> 模式保证所有通过 <see cref="ILCreateFromPath"/> 分配的 PIDL
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

            // 1. 将文件夹路径标准化（解析其中的 .. 和 .）
            string fullFolderPath = Path.GetFullPath(folderPath);

            // 2. 获取文件夹的 PIDL
            IntPtr folderPidl = ILCreateFromPath(fullFolderPath);
            if (folderPidl == IntPtr.Zero)
            {
                throw new FileNotFoundException($"无法获取文件夹 '{fullFolderPath}' 的 PIDL，请检查路径是否存在。");
            }

            // 3. 处理每个文件路径
            var validPidls = new List<IntPtr>();
            foreach (string path in filePaths)
            {
                // ★ 核心改动：先拼接（如果必要），然后用 GetFullPath 规范化
                string combinedPath = Path.IsPathRooted(path) ? path : Path.Combine(fullFolderPath, path);
                string fullPath = Path.GetFullPath(combinedPath);

                // 跳过不存在的文件或文件夹
                if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
                    continue;

                IntPtr pidl = ILCreateFromPath(fullPath);
                if (pidl != IntPtr.Zero)
                    validPidls.Add(pidl);
            }

            if (validPidls.Count == 0)
            {
                ILFree(folderPidl);
                throw new FileNotFoundException("没有有效的文件路径可选中。");
            }

            IntPtr[] pidlArray = [.. validPidls];
            try
            {
                int result = SHOpenFolderAndSelectItems(folderPidl, (uint)pidlArray.Length, pidlArray, 0);
                return result == 0;
            }
            finally
            {
                ILFree(folderPidl);
                foreach (IntPtr pidl in pidlArray)
                    if (pidl != IntPtr.Zero)
                        ILFree(pidl);
            }
        }
    }
}
