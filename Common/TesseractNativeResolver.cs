using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Tesseract;

namespace ToolApp.Common;

/// <summary>
/// Linux 服务器上 Tesseract 原生库文件名经常与 NuGet 包预期不一致（例如系统是 liblept.so.5，
/// 但托管包装层查找 libleptonica-1.82.0.so）。在这里做一次进程内映射，避免环境差异导致 OCR 失败。
/// </summary>
internal static class TesseractNativeResolver
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return;
        }

        EnsureLinuxNativeAliasesInAppBase();

        var asm = typeof(TesseractEngine).Assembly;
        NativeLibrary.SetDllImportResolver(asm, ResolveLinuxLibrary);
    }

    public static string GetDiagnostics()
    {
        var sb = new StringBuilder();
        var appBase = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        sb.Append("resolverInitialized=").Append(_initialized ? "true" : "false");
        sb.Append(", os=").Append(RuntimeInformation.OSDescription);
        sb.Append(", appBase=").Append(appBase);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            AppendPathState(sb, Path.Combine(appBase, "libleptonica-1.82.0.so"));
            AppendPathState(sb, Path.Combine(appBase, "libtesseract-5.so"));
            AppendPathState(sb, "/lib64/liblept.so.5");
            AppendPathState(sb, "/lib64/libtesseract.so.4");
            AppendPathState(sb, "/usr/lib64/liblept.so.5");
            AppendPathState(sb, "/usr/lib64/libtesseract.so.4");
        }

        return sb.ToString();
    }

    private static void AppendPathState(StringBuilder sb, string path)
    {
        sb.Append(", ");
        sb.Append(path);
        sb.Append("=");
        sb.Append(File.Exists(path) ? "exists" : "missing");
    }

    private static void EnsureLinuxNativeAliasesInAppBase()
    {
        var appBase = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

        EnsureAlias(
            aliasPath: Path.Combine(appBase, "libleptonica-1.82.0.so"),
            candidates: new[]
            {
                "/lib64/libleptonica-1.82.0.so",
                "/usr/lib64/libleptonica-1.82.0.so",
                "/lib64/liblept.so.5",
                "/usr/lib64/liblept.so.5"
            });

        EnsureAlias(
            aliasPath: Path.Combine(appBase, "libtesseract-5.so"),
            candidates: new[]
            {
                "/lib64/libtesseract-5.so",
                "/usr/lib64/libtesseract-5.so",
                "/lib64/libtesseract.so.4",
                "/usr/lib64/libtesseract.so.4"
            });
    }

    private static void EnsureAlias(string aliasPath, string[] candidates)
    {
        try
        {
            if (File.Exists(aliasPath))
            {
                return;
            }

            var source = string.Empty;
            foreach (var c in candidates)
            {
                if (File.Exists(c))
                {
                    source = c;
                    break;
                }
            }

            if (string.IsNullOrEmpty(source))
            {
                return;
            }

            try
            {
                File.CreateSymbolicLink(aliasPath, source);
                return;
            }
            catch
            {
                // some environments disallow symlink; fallback to file copy.
            }

            File.Copy(source, aliasPath, overwrite: false);
        }
        catch
        {
            // best effort only, real load errors are surfaced later by OCR call.
        }
    }

    private static IntPtr ResolveLinuxLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        // 先让运行时按默认规则尝试，避免影响正常环境。
        if (NativeLibrary.TryLoad(libraryName, assembly, searchPath, out var handle))
        {
            return handle;
        }

        var appBase = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

        // Tesseract 5.x 常见差异：包装层固定找 libleptonica-1.82.0.so，但系统实际只有 liblept.so.5。
        if (libraryName.Contains("libleptonica", StringComparison.OrdinalIgnoreCase) ||
            libraryName.Contains("liblept", StringComparison.OrdinalIgnoreCase))
        {
            if (TryLoadAny(
                new[]
                {
                    Path.Combine(appBase, "libleptonica-1.82.0.so"),
                    Path.Combine(appBase, "liblept.so.5"),
                    "/lib64/libleptonica-1.82.0.so",
                    "/usr/lib64/libleptonica-1.82.0.so",
                    "/lib64/liblept.so.5",
                    "/usr/lib64/liblept.so.5",
                    "libleptonica.so",
                    "liblept.so.5"
                },
                out handle))
            {
                return handle;
            }
        }

        // 部分发行版只提供 libtesseract.so.4（或其他版本）。
        if (libraryName.Contains("tesseract", StringComparison.OrdinalIgnoreCase))
        {
            if (TryLoadAny(
                new[]
                {
                    Path.Combine(appBase, "libtesseract-5.so"),
                    Path.Combine(appBase, "libtesseract.so.4"),
                    "/lib64/libtesseract-5.so",
                    "/usr/lib64/libtesseract-5.so",
                    "/lib64/libtesseract.so.4",
                    "/usr/lib64/libtesseract.so.4",
                    "libtesseract.so.5",
                    "libtesseract.so.4",
                    "libtesseract.so"
                },
                out handle))
            {
                return handle;
            }
        }

        return IntPtr.Zero;
    }

    private static bool TryLoadAny(string[] candidates, out IntPtr handle)
    {
        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            try
            {
                if (NativeLibrary.TryLoad(candidate, out handle))
                {
                    return true;
                }
            }
            catch
            {
                // try next candidate
            }
        }

        handle = IntPtr.Zero;
        return false;
    }
}

