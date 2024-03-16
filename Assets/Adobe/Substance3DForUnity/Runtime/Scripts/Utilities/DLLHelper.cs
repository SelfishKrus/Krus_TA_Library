using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Adobe.Substance
{
    /// <summary>
    /// Utility class for dynamically import substance plugin.
    /// </summary>
    internal class DLLHelpers
    {
        [DllImport("kernel32", SetLastError = true)]
        protected static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress", SetLastError = true)]
        protected static extern IntPtr GetProcAddress(IntPtr hModule, string procname);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("libdl")]
        protected static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libdl")]
        protected static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libdl")]
        protected static extern IntPtr dlerror();

        [DllImport("libdl")]
        protected static extern int dlclose(IntPtr handle);

        internal static IntPtr DllHandle = IntPtr.Zero;
        private static object[] mParams = new object[0];

        internal static object[] GetParams(int size)
        {
            Array.Resize<object>(ref mParams, size);
            return mParams;
        }

        internal static void LoadDLL(string dllPath)
        {
            if (DllHandle == IntPtr.Zero)
            {
                if (IsWindows())
                {
                    var oldWD = System.IO.Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(dllPath));

                    DllHandle = LoadLibraryW(dllPath);

                    if (DllHandle == IntPtr.Zero)
                    {
                        var error = Marshal.GetLastWin32Error();
                        Debug.LogError($"LoadLibraryW error: {error}");
                    }

                    Directory.SetCurrentDirectory(oldWD);
                }
                else if (IsMac() || IsLinux())
                {
                    DllHandle = dlopen(dllPath, 3);

                    if (DllHandle == IntPtr.Zero)
                    {
                        IntPtr errorMessage = dlerror();
                        Debug.LogError($"dlerror: {Marshal.PtrToStringAnsi(errorMessage)}");
                    }
                }

                if (DllHandle == IntPtr.Zero)
                    throw new ArgumentException($"Fail to load substance engine: {dllPath}");
            }
        }

        internal static void UnloadDLL()
        {
            if (DllHandle != IntPtr.Zero)
            {
                if (IsWindows())
                {
                    FreeLibrary(DllHandle);
                }
                else if (IsMac() || IsLinux())
                {
                    dlclose(DllHandle);
                }

                DllHandle = IntPtr.Zero;
            }
        }

        internal static Delegate GetFunction(string funcname, Type t)
        {
            IntPtr ptr = IntPtr.Zero;

            if (DllHandle == IntPtr.Zero)
                return null;

            if (IsWindows())
            {
                ptr = GetProcAddress(DllHandle, funcname);
            }
            else if (IsMac() || IsLinux())
            {
                ptr = dlsym(DllHandle, funcname);
            }

            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            return Marshal.GetDelegateForFunctionPointer(ptr, t);
        }

        private static bool IsWindows()
        {
            return (Application.platform == RuntimePlatform.WindowsEditor
                    || Application.platform == RuntimePlatform.WindowsPlayer);
        }

        private static bool IsMac()
        {
            return (Application.platform == RuntimePlatform.OSXEditor
                    || Application.platform == RuntimePlatform.OSXPlayer);
        }

        private static bool IsLinux()
        {
            return (Application.platform == RuntimePlatform.LinuxEditor
                    || Application.platform == RuntimePlatform.LinuxPlayer);
        }
    }
}