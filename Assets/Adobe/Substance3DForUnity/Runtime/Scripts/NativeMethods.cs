//Do not dynamically load on Android.
#if (!UNITY_EDITOR && UNITY_ANDROID)
#define ALG_SBSARIO_STATIC_LOAD
//Do not dynamically load on IOS.
#elif (!UNITY_EDITOR && UNITY_IOS)
#define ALG_SBSARIO_STATIC_LOAD

#elif (UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX)
#define ALG_SBSARIO_STATIC_LOAD
#else
//Dynamically load on Mac and Linux and Windows.
#define ALG_SBSARIO_DYNAMIC_LOAD
#endif

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Adobe.Substance
{
    internal static class NativeMethods
    {
#if (UNITY_IOS && !UNITY_EDITOR)
        public const string NativeAssembly = "__Internal";
        public const CharSet NativeCharSet = CharSet.Ansi;
        public const CallingConvention NativeCallingConvention = CallingConvention.StdCall;
#elif(UNITY_EDITOR_LINUX)
        public const string NativeAssembly = "sbsario";
        public const CharSet NativeCharSet = CharSet.Ansi;
        public const CallingConvention NativeCallingConvention = CallingConvention.StdCall;
#else
        public const string NativeAssembly = "sbsario";
        public const CharSet NativeCharSet = CharSet.Ansi;
        public const CallingConvention NativeCallingConvention = CallingConvention.StdCall;
#endif

        public static string substancePath = null;

        private static object _locker = new object();

        internal static IntPtr sbsario_get_version()
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_get_version();
            }
        }

        //! @brief Acquire the git hash of the sbsario library
        //! @note This may be called without initializing the library
        //! @return Constant string containing the sbsario git hash
        internal static IntPtr sbsario_get_hash()
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_get_hash();
            }
        }

        //! @brief Initialize the sbsario library
        //! @param engine_path File path to the Substance engine on disk, as a null-terminated
        //!        UTF-8 C string
        //! @return Error type enum representic success or an error
        internal static uint sbsario_initialize(string pluginPath, string engine_path, IntPtr memoryBudget)
        {
            lock (_locker)
            {
                try
                {
#if ALG_SBSARIO_DYNAMIC_LOAD
                    return NativeMethodsImpl.sbsario_initialize(engine_path, pluginPath, memoryBudget);
#else
                    return NativeMethodsImpl.sbsario_initialize(engine_path, memoryBudget);
#endif
                }
                catch (DllNotFoundException e)
                {
                    throw e;
                }
            }
        }

        //! @brief Shut down the sbsario library
        //! @return Error type enum representing success or error.
        internal static uint sbsario_shutdown()
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_shutdown();
            }
        }

        //! @brief Shut down the sbsario library
        //! @return Error type enum representing success or error
        public static IntPtr sbsario_sbsar_open(string sbsar_path)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_open(sbsar_path);
            }
        }

        public static IntPtr sbsario_sbsar_load_from_memory(IntPtr data, IntPtr size)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_load_from_memory(data, size);
            }
        }

        //! @brief Shut down the sbsario library
        //! @return Error type enum representing success or error
        internal static uint sbsario_sbsar_close(IntPtr sbsar_handle)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_close(sbsar_handle);
            }
        }

        internal static IntPtr sbsario_sbsar_get_graph_count(IntPtr sbsar_handle)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_graph_count(sbsar_handle);
            }
        }

        internal static IntPtr sbsario_sbsar_get_output_count(IntPtr sbsar_handle, IntPtr graph)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_output_count(sbsar_handle, graph);
            }
        }

        internal static uint sbsario_sbsar_get_output_desc(IntPtr sbsar_handle, IntPtr graph, IntPtr output, out NativeOutputDesc desc)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_output_desc(sbsar_handle, graph, output, out desc);
            }
        }

        internal static IntPtr sbsario_sbsar_get_input_count(IntPtr sbsar_handle, IntPtr graph)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_input_count(sbsar_handle, graph);
            }
        }

        internal static uint sbsario_sbsar_get_input_desc(IntPtr sbsar_handle, IntPtr graph, IntPtr output, out NativeInputDesc desc)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_input_desc(sbsar_handle, graph, output, out desc);
            }
        }

        internal static uint sbsario_sbsar_get_numeric_input_desc(IntPtr sbsar_handle, IntPtr graph, IntPtr inputIndex, out NativeNumericInputDesc desc)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_numeric_input_desc(sbsar_handle, graph, inputIndex, out desc);
            }
        }

        internal static uint sbsario_sbsar_get_enum_input_desc(IntPtr sbsar_handle, IntPtr graph, IntPtr inputIndex, IntPtr valuesArray, IntPtr size)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_enum_input_values(sbsar_handle, graph, inputIndex, valuesArray, size);
            }
        }

        internal static uint sbsario_sbsar_set_input(IntPtr sbsar_handle, IntPtr graph, ref NativeData data)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_set_input(sbsar_handle, graph, ref data);
            }
        }

        internal static uint sbsario_sbsar_get_input(IntPtr sbsar_handle, IntPtr graph, IntPtr input, out NativeData data)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_input(sbsar_handle, graph, input, out data);
            }
        }

        internal static uint sbsario_sbsar_get_input_visibility(IntPtr sbsar_handle, IntPtr graph, IntPtr input, out NativeInputVisibility visibilityInfo)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_input_visibility(sbsar_handle, graph, input, out visibilityInfo);
            }
        }

        internal static uint sbsario_sbsar_render(IntPtr sbsar_handle, IntPtr graph, out IntPtr result)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_render(sbsar_handle, graph, out result);
            }
        }

        internal static uint sbsario_sbsar_clear_results(IntPtr sbsar_handle, IntPtr graph)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_clear_results(sbsar_handle, graph);
            }
        }

        internal static uint sbsario_sbsar_utils_copy_texture(ref NativeDataImage src, ref NativeDataImage dst, uint flags)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_utils_copy_texture(ref src, ref dst, flags);
            }
        }

        internal static uint sbsario_sbsar_make_preset_from_current_state(IntPtr sbsar_handle, IntPtr graph, ref NativePreset preset)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_make_preset_from_current_state(sbsar_handle, graph, ref preset);
            }
        }

        internal static uint sbsario_sbsar_get_presets_count(IntPtr sbsar_handle, IntPtr graph, ref IntPtr count)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_presets_count(sbsar_handle, graph, ref count);
            }
        }

        internal static uint sbsario_sbsar_get_preset(IntPtr sbsar_handle, IntPtr graph, IntPtr presetIndex, ref NativePresetInfo preset)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_preset(sbsar_handle, graph, presetIndex, ref preset);
            }
        }

        internal static uint sbsario_sbsar_apply_preset(IntPtr sbsar_handle, IntPtr graph, ref NativePreset preset)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_apply_preset(sbsar_handle, graph, ref preset);
            }
        }

        internal static uint sbsario_sbsar_apply_baked_preset(IntPtr sbsar_handle, IntPtr graph, IntPtr presetIndex)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_apply_baked_preset(sbsar_handle, graph, presetIndex);
            }
        }

        internal static uint sbsario_sbsar_assign_as_alpha_channel(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, IntPtr alphaOutput, float minValue, float maxValue)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_assign_as_alpha_channel(sbsar_handle, graph, targetOutput, alphaOutput, minValue, maxValue);
            }
        }

        internal static uint sbsario_sbsar_create_virtual_output(IntPtr sbsar_handle, IntPtr graph, ref NativeOutputDesc desc, ref NativeOutputFormat format)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_create_virtual_output(sbsar_handle, graph, ref desc, ref format);
            }
        }

        internal static uint sbsario_sbsar_get_output_uid(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, out IntPtr uid)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_output_uid(sbsar_handle, graph, targetOutput, out uid);
            }
        }

        internal static uint sbsario_sbsar_create_output_copy(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, out NativeOutputDesc desc)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_create_output_copy(sbsar_handle, graph, targetOutput, out desc);
            }
        }

        internal static uint sbsario_sbsar_set_output_format_override(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, ref NativeOutputFormat format)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_set_output_format_override(sbsar_handle, graph, targetOutput, ref format);
            }
        }

        internal static uint sbsario_sbsar_get_output_format_override(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, out IntPtr isFormatOverridden, out NativeOutputFormat format)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_output_format_override(sbsar_handle, graph, targetOutput, out isFormatOverridden, out format);
            }
        }

        internal static uint sbsario_sbsar_get_graph_thumbnail(IntPtr sbsar_handle, IntPtr graph, out NativeThumbnail thumbnail)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_graph_thumbnail(sbsar_handle, graph, out thumbnail);
            }
        }

        internal static uint sbsario_sbsar_get_physical_size(IntPtr sbsar_handle, IntPtr graph, out NativePhysicalSize physicaSize)
        {
            lock (_locker)
            {
                return NativeMethodsImpl.sbsario_sbsar_get_physical_size(sbsar_handle, graph, out physicaSize);
            }
        }

        #region Impl

        //! @brief Native interface to the sbsario library, for communicating between the
        //!        unmanaged code and the managed code
        private static class NativeMethodsImpl
        {
#if ALG_SBSARIO_DYNAMIC_LOAD

            public static void LoadSBSARIO()
            {
            }

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate IntPtr sbsario_get_version_delegate();

            public static IntPtr sbsario_get_version()
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_get_version_delegate)) as sbsario_get_version_delegate;
                return function.Invoke();
            }

#else

            //! @brief Acquire the version of the sbsario library
            //! @note This may be called without initializing the library
            //! @return Constant string containing the sbsario semantic version
            [DllImport(NativeAssembly)]
            internal static extern IntPtr sbsario_get_version();

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate IntPtr sbsario_get_hash_delegate();

            public static IntPtr sbsario_get_hash()
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_get_hash_delegate)) as sbsario_get_hash_delegate;
                return function.Invoke();
            }

#else

            //! @brief Acquire the git hash of the sbsario library
            //! @note This may be called without initializing the library
            //! @return Constant string containing the sbsario git hash
            [DllImport(NativeAssembly)]
            internal static extern IntPtr sbsario_get_hash();

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            private delegate uint sbsario_initialize_delegate(IntPtr engine_path, IntPtr memoryBudget);

            public static uint sbsario_initialize(string engine_path, string pluginPath, IntPtr memoryBudget)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                DLLHelpers.LoadDLL(pluginPath);

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_initialize_delegate)) as sbsario_initialize_delegate;
                var globalStr = Marshal.StringToHGlobalAnsi(engine_path);
                return function.Invoke(globalStr, memoryBudget);
            }

#else

            //! @brief Initialize the sbsario library
            //! @param engine_path File path to the Substance engine on disk, as a null-terminated
            //!        UTF-8 C string
            //! @return Error type enum representic success or an error
            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_initialize([MarshalAs(UnmanagedType.LPStr)] string engine_path, IntPtr memoryBudget);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_shutdown_delegate();

            public static uint sbsario_shutdown()
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_shutdown_delegate)) as sbsario_shutdown_delegate;
                var result = function.Invoke();

                DLLHelpers.UnloadDLL();
                return result;
            }

#else

            //! @brief Shut down the sbsario library
            //! @return Error type enum representing success or error
            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_shutdown();

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate IntPtr sbsario_sbsar_open_delegate([MarshalAs(UnmanagedType.LPStr)] string sbsar_path);

            public static IntPtr sbsario_sbsar_open(string sbsar_handle)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_open_delegate)) as sbsario_sbsar_open_delegate;
                return function.Invoke(sbsar_handle);
            }

#else

            //! @brief Shut down the sbsario library
            //! @return Error type enum representing success or error
            [DllImport(NativeAssembly)]
            public static extern IntPtr sbsario_sbsar_open([MarshalAs(UnmanagedType.LPStr)] string sbsar_path);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate IntPtr sbsario_sbsar_load_from_memory_delegate(IntPtr data, IntPtr size);

            public static IntPtr sbsario_sbsar_load_from_memory(IntPtr data, IntPtr size)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_load_from_memory_delegate)) as sbsario_sbsar_load_from_memory_delegate;
                return function.Invoke(data, size);
            }

#else

            //! @brief Shut down the sbsario library
            //! @return Error type enum representing success or error
            [DllImport(NativeAssembly)]
            public static extern IntPtr sbsario_sbsar_load_from_memory(IntPtr data, IntPtr size);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_close_delegate(IntPtr sbsar_handle);

            public static uint sbsario_sbsar_close(IntPtr sbsar_handle)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_close_delegate)) as sbsario_sbsar_close_delegate;
                return function.Invoke(sbsar_handle);
            }

#else

            //! @brief Shut down the sbsario library
            //! @return Error type enum representing success or error
            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_close(IntPtr sbsar_handle);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate IntPtr sbsario_sbsar_get_graph_count_delegate(IntPtr sbsar_handle);

            public static IntPtr sbsario_sbsar_get_graph_count(IntPtr sbsar_handle)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_graph_count_delegate)) as sbsario_sbsar_get_graph_count_delegate;
                return function.Invoke(sbsar_handle);
            }

#else

            //! @brief
            //! @return
            [DllImport(NativeAssembly)]
            internal static extern IntPtr sbsario_sbsar_get_graph_count(IntPtr sbsar_handle);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate IntPtr sbsario_sbsar_get_output_count_delegate(IntPtr sbsar_handle, IntPtr graph);

            public static IntPtr sbsario_sbsar_get_output_count(IntPtr sbsar_handle, IntPtr graph)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_output_count_delegate)) as sbsario_sbsar_get_output_count_delegate;
                return function.Invoke(sbsar_handle, graph);
            }

#else

            //! @brief
            //! @return
            [DllImport(NativeAssembly)]
            internal static extern IntPtr sbsario_sbsar_get_output_count(IntPtr sbsar_handle, IntPtr graph);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_get_output_desc_delegate(IntPtr sbsar_handle, IntPtr graph, IntPtr output, out NativeOutputDesc desc);

            public static uint sbsario_sbsar_get_output_desc(IntPtr sbsar_handle, IntPtr graph, IntPtr output, out NativeOutputDesc desc)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_output_desc_delegate)) as sbsario_sbsar_get_output_desc_delegate;
                return function.Invoke(sbsar_handle, graph, output, out desc);
            }

#else

            //! @brief
            //! @return
            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_get_output_desc(IntPtr sbsar_handle, IntPtr graph, IntPtr output, out NativeOutputDesc desc);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate IntPtr sbsario_sbsar_get_input_count_delegate(IntPtr sbsar_handle, IntPtr graph);

            public static IntPtr sbsario_sbsar_get_input_count(IntPtr sbsar_handle, IntPtr graph)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_input_count_delegate)) as sbsario_sbsar_get_input_count_delegate;
                return function.Invoke(sbsar_handle, graph);
            }

#else

            //! @brief
            //! @return
            [DllImport(NativeAssembly)]
            internal static extern IntPtr sbsario_sbsar_get_input_count(IntPtr sbsar_handle, IntPtr graph);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_get_input_desc_delegate(IntPtr sbsar_handle, IntPtr graph, IntPtr output, out NativeInputDesc desc);

            public static uint sbsario_sbsar_get_input_desc(IntPtr sbsar_handle, IntPtr graph, IntPtr output, out NativeInputDesc desc)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_input_desc_delegate)) as sbsario_sbsar_get_input_desc_delegate;
                return function.Invoke(sbsar_handle, graph, output, out desc);
            }

#else

            //! @brief
            //! @return
            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_get_input_desc(IntPtr sbsar_handle, IntPtr graph, IntPtr output, out NativeInputDesc desc);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_get_numeric_input_desc_delegate(IntPtr sbsar_handle, IntPtr graph, IntPtr input, out NativeNumericInputDesc desc);

            public static uint sbsario_sbsar_get_numeric_input_desc(IntPtr sbsar_handle, IntPtr graph, IntPtr input, out NativeNumericInputDesc desc)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_numeric_input_desc_delegate)) as sbsario_sbsar_get_numeric_input_desc_delegate;
                return function.Invoke(sbsar_handle, graph, input, out desc);
            }

#else

            //! @brief
            //! @return
            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_get_numeric_input_desc(IntPtr sbsar_handle, IntPtr graph, IntPtr input, out NativeNumericInputDesc desc);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_get_enum_input_values_delegate(IntPtr sbsar_handle, IntPtr graph, IntPtr input, IntPtr valuesArray, IntPtr arraySize);

            public static uint sbsario_sbsar_get_enum_input_values(IntPtr sbsar_handle, IntPtr graph, IntPtr input, IntPtr valuesArray, IntPtr arraySize)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_enum_input_values_delegate)) as sbsario_sbsar_get_enum_input_values_delegate;
                return function.Invoke(sbsar_handle, graph, input, valuesArray, arraySize);
            }

#else

            //! @brief
            //! @return
            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_get_enum_input_values(IntPtr sbsar_handle, IntPtr graph, IntPtr input, IntPtr valuesArray, IntPtr arraySize);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_set_input_delegate(IntPtr sbsar_handle, IntPtr graph, ref NativeData data);

            public static uint sbsario_sbsar_set_input(IntPtr sbsar_handle, IntPtr graph, ref NativeData data)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_set_input_delegate)) as sbsario_sbsar_set_input_delegate;
                return function.Invoke(sbsar_handle, graph, ref data);
            }

#else

            //! @brief
            //! @return
            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_set_input(IntPtr sbsar_handle, IntPtr graph, ref NativeData data);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_get_input_delegate(IntPtr sbsar_handle, IntPtr graph, IntPtr input, out NativeData data);

            public static uint sbsario_sbsar_get_input(IntPtr sbsar_handle, IntPtr graph, IntPtr input, out NativeData data)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_input_delegate)) as sbsario_sbsar_get_input_delegate;
                return function.Invoke(sbsar_handle, graph, input, out data);
            }

#else

            //! @brief
            //! @return
            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_get_input(IntPtr sbsar_handle, IntPtr graph, IntPtr input, out NativeData data);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_render_delegate(IntPtr sbsar_handle, IntPtr graph, out IntPtr result);

            public static uint sbsario_sbsar_render(IntPtr sbsar_handle, IntPtr graph, out IntPtr result)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_render_delegate)) as sbsario_sbsar_render_delegate;
                return function.Invoke(sbsar_handle, graph, out result);
            }

#else

            //! @brief
            //! @return
            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_render(IntPtr sbsar_handle, IntPtr graph, out IntPtr result);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_clear_results_delegate(IntPtr sbsar_handle, IntPtr graph);

            public static uint sbsario_sbsar_clear_results(IntPtr sbsar_handle, IntPtr graph)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_clear_results_delegate)) as sbsario_sbsar_clear_results_delegate;
                return function.Invoke(sbsar_handle, graph);
            }

#else
            /** @brief Implementation to lear any stored memory for renders with the graph
            @param sbsar_object Pointer to a valid sbsar object to clear results on
            @param graph Index of the graph to clear the results of
            @return sbsario_error_t enum representing success or the error **/

            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_clear_results(IntPtr sbsar_handle, IntPtr graph);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_utils_copy_texture_delegate(ref NativeDataImage src, ref NativeDataImage dst, uint flags);

            public static uint sbsario_sbsar_utils_copy_texture(ref NativeDataImage src, ref NativeDataImage dst, uint flags)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_utils_copy_texture_delegate)) as sbsario_sbsar_utils_copy_texture_delegate;
                return function.Invoke(ref src, ref dst, flags);
            }

#else

            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_utils_copy_texture(ref NativeDataImage src, ref NativeDataImage dst, uint flags);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_make_preset_from_current_state_delegate(IntPtr sbsar_handle, IntPtr graph, ref NativePreset preset);

            public static uint sbsario_sbsar_make_preset_from_current_state(IntPtr sbsar_handle, IntPtr graph, ref NativePreset preset)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_make_preset_from_current_state_delegate)) as sbsario_sbsar_make_preset_from_current_state_delegate;
                return function.Invoke(sbsar_handle, graph, ref preset);
            }

#else

            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_make_preset_from_current_state(IntPtr sbsar_handle, IntPtr graph, ref NativePreset preset);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_get_presets_count_delegate(IntPtr sbsar_handle, IntPtr graph, ref IntPtr count);

            public static uint sbsario_sbsar_get_presets_count(IntPtr sbsar_handle, IntPtr graph, ref IntPtr count)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_presets_count_delegate)) as sbsario_sbsar_get_presets_count_delegate;
                return function.Invoke(sbsar_handle, graph, ref count);
            }

#else
            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_get_presets_count(IntPtr sbsar_handle, IntPtr graph, ref IntPtr count);
#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_get_preset_delegate(IntPtr sbsar_handle, IntPtr graph, IntPtr presetIndex, ref NativePresetInfo preset);

            public static uint sbsario_sbsar_get_preset(IntPtr sbsar_handle, IntPtr graph, IntPtr presetIndex, ref NativePresetInfo preset)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_preset_delegate)) as sbsario_sbsar_get_preset_delegate;
                return function.Invoke(sbsar_handle, graph, presetIndex, ref preset);
            }

#else
            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_get_preset(IntPtr sbsar_handle, IntPtr graph, IntPtr presetIndex, ref NativePresetInfo preset);
#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_apply_preset_delegate(IntPtr sbsar_handle, IntPtr graph, ref NativePreset preset);

            public static uint sbsario_sbsar_apply_preset(IntPtr sbsar_handle, IntPtr graph, ref NativePreset preset)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_apply_preset_delegate)) as sbsario_sbsar_apply_preset_delegate;
                return function.Invoke(sbsar_handle, graph, ref preset);
            }

#else

            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_apply_preset(IntPtr sbsar_handle, IntPtr graph, ref NativePreset preset);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_apply_backed_preset_delegate(IntPtr sbsar_handle, IntPtr graph, IntPtr presetIndex);

            public static uint sbsario_sbsar_apply_baked_preset(IntPtr sbsar_handle, IntPtr graph, IntPtr presetIndex)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_apply_backed_preset_delegate)) as sbsario_sbsar_apply_backed_preset_delegate;
                return function.Invoke(sbsar_handle, graph, presetIndex);
            }

#else

            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_apply_baked_preset(IntPtr sbsar_handle, IntPtr graph, IntPtr presetIndex);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_get_input_visibility_delegate(IntPtr sbsar_handle, IntPtr graph, IntPtr input, out NativeInputVisibility visibilityInfo);

            public static uint sbsario_sbsar_get_input_visibility(IntPtr sbsar_handle, IntPtr graph, IntPtr input, out NativeInputVisibility visibilityInfo)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_input_visibility_delegate)) as sbsario_sbsar_get_input_visibility_delegate;
                return function.Invoke(sbsar_handle, graph, input, out visibilityInfo);
            }

#else

            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_get_input_visibility(IntPtr sbsar_handle, IntPtr graph, IntPtr input, out NativeInputVisibility visibilityInfo);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_assign_as_alpha_channel_delegate(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, IntPtr alphaOutput, float minValue, float maxValue);

            public static uint sbsario_sbsar_assign_as_alpha_channel(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, IntPtr alphaOutput, float minValue, float maxValue)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_assign_as_alpha_channel_delegate)) as sbsario_sbsar_assign_as_alpha_channel_delegate;
                return function.Invoke(sbsar_handle, graph, targetOutput, alphaOutput, minValue, maxValue);
            }

#else

            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_assign_as_alpha_channel(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, IntPtr alphaOutput, float minValue, float maxValue);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_create_virtual_output_delegate(IntPtr sbsar_handle, IntPtr graph, ref NativeOutputDesc desc, ref NativeOutputFormat format);

            public static uint sbsario_sbsar_create_virtual_output(IntPtr sbsar_handle, IntPtr graph, ref NativeOutputDesc desc, ref NativeOutputFormat format)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_create_virtual_output_delegate)) as sbsario_sbsar_create_virtual_output_delegate;
                return function.Invoke(sbsar_handle, graph, ref desc, ref format);
            }

#else

            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_create_virtual_output(IntPtr sbsar_handle, IntPtr graph, ref NativeOutputDesc desc, ref NativeOutputFormat format);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_get_output_uid_delegate(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, out IntPtr uid);

            public static uint sbsario_sbsar_get_output_uid(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, out IntPtr uid)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_output_uid_delegate)) as sbsario_sbsar_get_output_uid_delegate;
                return function.Invoke(sbsar_handle, graph, targetOutput, out uid);
            }

#else

            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_get_output_uid(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, out IntPtr uid);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_create_output_copy_delegate(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, out NativeOutputDesc desc);

            public static uint sbsario_sbsar_create_output_copy(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, out NativeOutputDesc desc)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_create_output_copy_delegate)) as sbsario_sbsar_create_output_copy_delegate;
                return function.Invoke(sbsar_handle, graph, targetOutput, out desc);
            }

#else

            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_create_output_copy(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, out NativeOutputDesc desc);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_set_output_format_override_delegate(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, ref NativeOutputFormat format);

            public static uint sbsario_sbsar_set_output_format_override(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, ref NativeOutputFormat format)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_set_output_format_override_delegate)) as sbsario_sbsar_set_output_format_override_delegate;
                return function.Invoke(sbsar_handle, graph, targetOutput, ref format);
            }

#else

            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_set_output_format_override(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, ref NativeOutputFormat format);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_get_output_format_override_delegate(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, out IntPtr isFormatOverridden, out NativeOutputFormat format);

            public static uint sbsario_sbsar_get_output_format_override(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, out IntPtr isFormatOverridden, out NativeOutputFormat format)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_output_format_override_delegate)) as sbsario_sbsar_get_output_format_override_delegate;
                return function.Invoke(sbsar_handle, graph, targetOutput, out isFormatOverridden, out format);
            }

#else

            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_get_output_format_override(IntPtr sbsar_handle, IntPtr graph, IntPtr targetOutput, out IntPtr isFormatOverridden, out NativeOutputFormat format);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_get_graph_thumbnail_delegate(IntPtr sbsar_handle, IntPtr graph, out NativeThumbnail thumbnail);

            public static uint sbsario_sbsar_get_graph_thumbnail(IntPtr sbsar_handle, IntPtr graph, out NativeThumbnail thumbnail)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_graph_thumbnail_delegate)) as sbsario_sbsar_get_graph_thumbnail_delegate;
                return function.Invoke(sbsar_handle, graph, out thumbnail);
            }

#else

            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_get_graph_thumbnail(IntPtr sbsar_handle, IntPtr graph, out NativeThumbnail thumbnail);

#endif

#if ALG_SBSARIO_DYNAMIC_LOAD

            private delegate uint sbsario_sbsar_get_physical_size_delegate(IntPtr sbsar_handle, IntPtr graph, out NativePhysicalSize physicaSize);

            public static uint sbsario_sbsar_get_physical_size(IntPtr sbsar_handle, IntPtr graph, out NativePhysicalSize physicaSize)
            {
                string myName = System.Reflection.MethodBase.GetCurrentMethod().Name;

                if (DLLHelpers.DllHandle == IntPtr.Zero)
                    throw new SubstanceEngineNotFoundException(substancePath);

                var function = DLLHelpers.GetFunction(myName, typeof(sbsario_sbsar_get_physical_size_delegate)) as sbsario_sbsar_get_physical_size_delegate;
                return function.Invoke(sbsar_handle, graph, out physicaSize);
            }

#else

            [DllImport(NativeAssembly)]
            internal static extern uint sbsario_sbsar_get_physical_size(IntPtr sbsar_handle, IntPtr graph, out NativePhysicalSize physicaSize);

#endif

            #endregion Impl
        }
    }
}