using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance
{
    public class Engine
    {
        private enum LoadState : uint
        {
            Engine_Unloaded = 0x00u, //!< Engine is currently not loaded.
            Engine_Loaded = 0x02u, //!< The engine is loaded
            Engine_FatalError = 0x04u, //!< An unrecoverable error has occurred
        }

        private static LoadState sLoadState = LoadState.Engine_Unloaded;

        public static bool IsInitialized => sLoadState == LoadState.Engine_Loaded;

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        private const int MAX_TEXTURE_SIZE = 512;
        private const int MEMORY_BUGET = 512;
#else
        private const int MEMORY_BUGET = 2048;
#endif

        public static string Version()
        {
            var version_ptr = NativeMethods.sbsario_get_version();
            return Marshal.PtrToStringAnsi(version_ptr);
        }

        public static string GetHash()
        {
            var version_ptr = NativeMethods.sbsario_get_hash();
            return Marshal.PtrToStringAnsi(version_ptr);
        }


        /// <summary>
        /// Initialize the Substance Engine by consuming it dynamically.
        /// </summary>
        /// <param name="pluginPath">Path to the sbsario library.</param>
        /// <param name="enginePath">Path to the substance engine library.</param>
        /// <exception cref="SubstanceException"></exception>
        public static void Initialize(string pluginPath, string enginePath)
        {
            if (sLoadState == LoadState.Engine_Loaded)
                return;

            IntPtr memoryBudget = (IntPtr)MEMORY_BUGET;

            var code = (ErrorCode)NativeMethods.sbsario_initialize(pluginPath, enginePath, memoryBudget);

            // On success, set the engine state to loaded
            if (code == ErrorCode.SBSARIO_ERROR_OK)
                sLoadState = LoadState.Engine_Loaded;

            if (code != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(code);
        }

        /// <summary>
        /// Initialize the Substance Engine by consuming it as a static library. 
        /// </summary>
        public static void Initialize()
        {
            if (sLoadState == LoadState.Engine_Loaded)
                return;

            IntPtr memoryBudget = (IntPtr)MEMORY_BUGET;

            var code = (ErrorCode)NativeMethods.sbsario_initialize(null, null, memoryBudget);

            if (sLoadState == LoadState.Engine_Unloaded)
            {
                // On success, set the engine state to loaded
                if (code == ErrorCode.SBSARIO_ERROR_OK)
                    sLoadState = LoadState.Engine_Loaded;
            }

            if (code != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(code);
        }

        /// <summary>
        /// Shuts down the substance engine.
        /// </summary>
        public static void Shutdown()
        {
            var code = (ErrorCode)NativeMethods.sbsario_shutdown();

            if (sLoadState == LoadState.Engine_Loaded)
            {
                // On success, set the engine to an unloaded state
                if (code == ErrorCode.SBSARIO_ERROR_OK)
                    sLoadState = LoadState.Engine_Unloaded;
            }

            if (code != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(code);
        }

        /// <summary>
        /// Load a substance graph into the engine.
        /// </summary>
        /// <param name="data">Binary content of the sbsar file.</param>
        /// <param name="graphID">Id of the graph to be loaded.</param>
        /// <returns>A SubstanceNativeGraph object.</returns>
        public static SubstanceNativeGraph OpenFile(byte[] data, int graphID)
        {
            if (sLoadState != LoadState.Engine_Loaded)
                throw new ArgumentException("Engine must be loaded before creating Native Handler");

            var substanceFile = new SubstanceNativeGraph(data, graphID);
            return substanceFile;
        }

        /// <summary>
        /// Get the total graph count for this substance object
        /// </summary>
        /// <returns>Total graph count.</returns>
        public static int GetFileGraphCount(byte[] fileContent)
        {
            int size = Marshal.SizeOf(fileContent[0]) * fileContent.Length;
            var nativeMemory = Marshal.AllocHGlobal(size);
            Marshal.Copy(fileContent, 0, nativeMemory, size);

            IntPtr handler = default;

            try
            {
                handler = NativeMethods.sbsario_sbsar_load_from_memory(nativeMemory, (IntPtr)size);

                if (handler == default)
                    throw new ArgumentException();

                return (int)NativeMethods.sbsario_sbsar_get_graph_count(handler);
            }
            finally
            {
                if (handler != default)
                    NativeMethods.sbsario_sbsar_close(handler);

                Marshal.FreeHGlobal(nativeMemory);
            }
        }
    }
}