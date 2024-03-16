using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.IO;

namespace Adobe.Substance
{
    /// <summary>
    /// Utility functions for platform specific setup.
    /// </summary>
    public static class PlatformUtils
    {
        public static bool IsCPU()
        {
            if (Application.platform == RuntimePlatform.OSXPlayer ||
#if UNITY_2021_3_OR_NEWER
                            Application.platform == RuntimePlatform.OSXServer ||
#endif
                            Application.platform == RuntimePlatform.OSXEditor)
            {
                return false;
            }
            else if (Application.platform == RuntimePlatform.LinuxPlayer ||
#if UNITY_2021_3_OR_NEWER
                     Application.platform == RuntimePlatform.LinuxServer ||
#endif
                     Application.platform == RuntimePlatform.LinuxEditor)
            {
                return false;
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer ||
#if UNITY_2021_3_OR_NEWER
                     Application.platform == RuntimePlatform.WindowsServer ||
#endif
                     Application.platform == RuntimePlatform.WindowsEditor)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get full path to the appropriate substance engine based on the current graphics API.
        /// </summary>
        /// <returns>Path to the substance engine.</returns>
        public static string GetEnginePath()
        {
            var engineName = GetEngineName();
            return GetPluginLibraryPath(engineName);
        }

        /// <summary>
        /// Get the path to the substance plugin (Only used when plugin is dynamically loaded).
        /// </summary>
        /// <returns>Path to the unity plugin.</returns>
        public static string GetPluginPath()
        {
            var pluginName = GetPluginName();
            return GetPluginLibraryPath(pluginName);
        }

        private static string GetPluginLibraryPath(string libraryName)
        {
#if UNITY_EDITOR
            var path = $"{PathUtils.SubstanceRootPath}/Runtime/Plugins/{libraryName}";
            return Path.GetFullPath(path);
#elif UNITY_STANDALONE_WIN
            return Path.Combine(Path.Combine(Path.Combine(Application.dataPath, "Plugins"), "x86_64"), libraryName);
#elif UNITY_EDITOR_OSX
            return Path.Combine(Path.Combine(Application.dataPath, "PlugIns"), libraryName);
#else
            return Path.Combine(Path.Combine(Application.dataPath, "Plugins"), libraryName);
#endif
        }

        /// <summary>
        /// Get engine library name.
        /// </summary>
        /// <returns>Substance engien name.</returns>
        private static string GetEngineName()
        {
            if (Application.platform == RuntimePlatform.LinuxEditor
                    || Application.platform == RuntimePlatform.LinuxPlayer)
            {
                return "libsubstance_ogl3_blend.so.9";
            }
            else if (Application.platform == RuntimePlatform.OSXEditor
                    || Application.platform == RuntimePlatform.OSXPlayer)
            {
                return "libsubstance_mtl_blend.dylib";
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor
                    || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return "substance_d3d11pc_blend.dll";
            }
            else
            {
                return string.Empty;
            }
        }

        private static string GetPluginName()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "libsbsario.dylib";

                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    return "libsbsario.so";

                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "sbsario.dll";

                default:
                    return string.Empty;
            }
        }
    }
}