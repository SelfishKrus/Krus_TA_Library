//! @file substancearchive.cs
//! @brief Substance archive interface
//! @author Galen Helfter - Adobe
//! @date 20210609
//! @copyright Adobe. All rights reserved.

using System;
using System.Runtime.InteropServices;
using UnityEngine;

using Adobe.Substance.Input;
using Adobe.Substance.Input.Description;
using System.Collections.Generic;

namespace Adobe.Substance
{
    /// <summary>
    /// Object that represents a native C++ substance graph.
    /// </summary>
    public sealed class SubstanceNativeGraph : IDisposable
    {
        private IntPtr _handler;
        private bool _disposedValue;
        private bool _inRenderWork;

        public bool IsInitialized { get; set; }

        public bool InRenderWork
        {
            get
            {
                lock (this)
                {
                    return _inRenderWork;
                }
            }
            set
            {
                lock (this)
                {
                    _inRenderWork = value;
                }
            }
        }

        private readonly int _fileGraphID;

        /// <summary>
        /// Create a object that acts as a brige between Unity and Substance C++ SDK.
        /// </summary>
        /// <param name="fileContent">Content of the sbsar file.</param>
        /// <param name="targetGraphID">Target graph ID inside the file.</param>
        internal SubstanceNativeGraph(byte[] fileContent, int targetGraphID)
        {
            _fileGraphID = targetGraphID;
            int size = Marshal.SizeOf(fileContent[0]) * fileContent.Length;
            var nativeMemory = Marshal.AllocHGlobal(size);
            Marshal.Copy(fileContent, 0, nativeMemory, size);

            try
            {
                _handler = NativeMethods.sbsario_sbsar_load_from_memory(nativeMemory, (IntPtr)size);

                if (_handler == default)
                    throw new ArgumentException();
            }
            finally
            {
                if (nativeMemory != default)
                    Marshal.FreeHGlobal(nativeMemory);
            }
        }

        /// <summary>
        /// Renders graph.
        /// </summary>
        /// <returns>Native render result.</returns>
        public IntPtr Render()
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode errorCode = (ErrorCode)NativeMethods.sbsario_sbsar_render(_handler, (IntPtr)_fileGraphID, out IntPtr result);

            if (errorCode != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(errorCode);

            return result;
        }

        /// <summary>
        /// Get raw thumbnail data from graph.
        /// </summary>
        /// <returns>Raw graph thumbnail RGBA data.</returns>
        public byte[] GetThumbnail()
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_graph_thumbnail(_handler, (IntPtr)_fileGraphID, out NativeThumbnail thumbnail);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            if (thumbnail.Size == IntPtr.Zero)
                return null;

            var thumbnailData = new byte[(int)thumbnail.Size];
            Marshal.Copy(thumbnail.Data, thumbnailData, 0, thumbnailData.Length);
            return thumbnailData;
        }

        #region Presets

        public string CreatePresetFromCurrentState()
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            //Alocate 1Mb for the preset file text.
            NativePreset preset = new NativePreset
            {
                XMLString = Marshal.AllocHGlobal(1024 * 1024)
            };

            try
            {
                ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_make_preset_from_current_state(_handler, (IntPtr)_fileGraphID, ref preset);

                if (result != ErrorCode.SBSARIO_ERROR_OK)
                    throw new SubstanceException(result);

                var stringResult = Marshal.PtrToStringAnsi(preset.XMLString);
                return stringResult;
            }
            finally
            {
                //Free native allocated memory.
                Marshal.FreeHGlobal(preset.XMLString);
            }
        }

        /// <summary>
        /// Apply preset string to graph.
        /// </summary>
        /// <param name="presetXML"></param>
        public void ApplyPreset(string presetXML)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            NativePreset preset = new NativePreset();
            preset.XMLString = Marshal.StringToHGlobalAnsi(presetXML);

            try
            {
                ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_apply_preset(_handler, (IntPtr)_fileGraphID, ref preset);

                if (result != ErrorCode.SBSARIO_ERROR_OK)
                    throw new SubstanceException(result);
            }
            finally
            {
                // Always free the unmanaged string.
                Marshal.FreeHGlobal(preset.XMLString);
            }
        }

        public void ApplyBakedPreset(int index)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_apply_baked_preset(_handler, (IntPtr)_fileGraphID, (IntPtr)index);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);
        }

        public List<SubstancePresetInfo> GetPresetsList()
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            List<SubstancePresetInfo> presets = new List<SubstancePresetInfo>();

            IntPtr count = IntPtr.Zero;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_presets_count(_handler, (IntPtr)_fileGraphID, ref count);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            for (int i = 0; i < count.ToInt32(); i++)
            {
                NativePresetInfo nativePresetInfo = new NativePresetInfo();
                result = (ErrorCode)NativeMethods.sbsario_sbsar_get_preset(_handler, (IntPtr)_fileGraphID, (IntPtr)i, ref nativePresetInfo);

                if (result != ErrorCode.SBSARIO_ERROR_OK)
                    throw new SubstanceException(result);

                SubstancePresetInfo presetInfo = new SubstancePresetInfo
                {
                    Description = Marshal.PtrToStringAnsi(nativePresetInfo.mDescription),
                    Label = Marshal.PtrToStringAnsi(nativePresetInfo.mLabel),
                    URL = Marshal.PtrToStringAnsi(nativePresetInfo.mPackageUrl)
                };

                presets.Add(presetInfo);
            }

            return presets;
        }

        #endregion Presets

        #region Output

        /// <summary>
        /// Get the total output count for a given graph in the substance object.
        /// </summary>
        /// <returns>Total graph output count.</returns>
        public int GetOutputCount()
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            return (int)NativeMethods.sbsario_sbsar_get_output_count(_handler, (IntPtr)_fileGraphID);
        }

        public SubstanceOutputDescription GetOutputDescription(int outputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_output_desc(_handler, (IntPtr)_fileGraphID, (IntPtr)outputID, out NativeOutputDesc inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            var identifier = Marshal.PtrToStringAnsi(inputDesc.mIdentifier);
            var label = Marshal.PtrToStringAnsi(inputDesc.mIdentifier);
            var channel = Marshal.PtrToStringAnsi(inputDesc.mChannelUsage);

            if (string.IsNullOrEmpty(channel))
            {
                Debug.LogWarning($"Output {outputID} does not have a channel type. This output will not be rendered. ");
                channel = "unkown";
            }

            if (string.IsNullOrEmpty(identifier))
            {
                Debug.LogWarning($"Output {outputID} does not have a identifier.");
                identifier = "unkown";
            }

            if (string.IsNullOrEmpty(label))
            {
                Debug.LogWarning($"Output {outputID} does not have a label.");
                label = "unkown";
            }

            return new SubstanceOutputDescription()
            {
                Identifier = identifier,
                Label = label,
                Index = (int)inputDesc.mIndex,
                Type = inputDesc.mValueType.ToUnity(),
                Channel = channel
            };
        }

        public SubstanceOutputDescription CreateVirtualOutput(SubstanceVirtualOutputCreateInfo info)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            var description = info.CreateOutputDesc();
            var format = info.CreateOutputFormat();

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_create_virtual_output(_handler, (IntPtr)_fileGraphID, ref description, ref format);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            var identifier = Marshal.PtrToStringAnsi(description.mIdentifier);
            var label = Marshal.PtrToStringAnsi(description.mIdentifier);
            var channel = Marshal.PtrToStringAnsi(description.mChannelUsage);

            return new SubstanceOutputDescription()
            {
                Identifier = identifier,
                Label = label,
                Index = (int)description.mIndex,
                Type = description.mValueType.ToUnity(),
                Channel = channel,
            };
        }

        public int CreateOutputCopy(int outputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_create_output_copy(_handler, (IntPtr)_fileGraphID, (IntPtr)outputID, out NativeOutputDesc inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            return (int)inputDesc.mIndex;
        }

        public uint GetOutputUID(int outputIndex)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_output_uid(_handler, (IntPtr)_fileGraphID, (IntPtr)outputIndex, out IntPtr outputUID);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            return (uint)outputUID;
        }

        public void AssignOutputToAlphaChannel(int targetOutputID, int alphaChannelID, bool invert = false)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            float minValue = invert ? 1f : 0f;
            float maxValue = invert ? 0f : 1f;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_assign_as_alpha_channel(_handler, (IntPtr)_fileGraphID, (IntPtr)targetOutputID, (IntPtr)alphaChannelID, minValue, maxValue);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);
        }

        public void ResetAlphaChannelAssignment(int targetOutputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_output_desc(_handler, (IntPtr)_fileGraphID, (IntPtr)targetOutputID, out NativeOutputDesc outputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            result = (ErrorCode)NativeMethods.sbsario_sbsar_get_output_format_override(_handler, (IntPtr)_fileGraphID, (IntPtr)targetOutputID, out IntPtr isFormatOverridenPtr, out NativeOutputFormat oldFormat);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            bool isFormatOverriden = (int)isFormatOverridenPtr != 0;

            if (!isFormatOverriden)
                return;

            if (oldFormat.format == NativeConsts.UseDefault)
                return;

            NativeOutputFormat newFormat = oldFormat;
            newFormat.format = NativeConsts.UseDefault;

            result = (ErrorCode)NativeMethods.sbsario_sbsar_set_output_format_override(_handler, (IntPtr)_fileGraphID, (IntPtr)targetOutputID, ref newFormat);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);
        }

        public void ChangeOutputRBChannels(int targetOutputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_output_format_override(_handler, (IntPtr)_fileGraphID, (IntPtr)targetOutputID, out IntPtr isFormatOverridenPtr, out NativeOutputFormat oldFormat);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            bool isFormatOverriden = (int)isFormatOverridenPtr != 0;

            NativeOutputFormat newFormat = NativeOutputFormat.CreateDefault();

            if (isFormatOverriden)
                newFormat = oldFormat;

            var redChannel = newFormat.ChannelComponent0;
            var blueChannel = newFormat.ChannelComponent2;

            if (blueChannel.outputIndex == NativeConsts.UseDefault || redChannel.outputIndex == NativeConsts.UseDefault)
                return;

            if (newFormat.ChannelComponent0.ShuffleIndex == ShuffleIndex.Blue &&
            newFormat.ChannelComponent2.ShuffleIndex == ShuffleIndex.Red)
            {
                return;
            }

            newFormat.ChannelComponent0.ShuffleIndex = ShuffleIndex.Blue;
            newFormat.ChannelComponent2.ShuffleIndex = ShuffleIndex.Red;

            result = (ErrorCode)NativeMethods.sbsario_sbsar_set_output_format_override(_handler, (IntPtr)_fileGraphID, (IntPtr)targetOutputID, ref newFormat);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);
        }

        #endregion Output

        #region Input

        /// <summary>
        /// Get the total input count for a given graph in the substance object.
        /// </summary>
        /// <returns>Total graph input count.</returns>
        public int GetInputCount()
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            return (int)NativeMethods.sbsario_sbsar_get_input_count(_handler, (IntPtr)_fileGraphID);
        }

        /// <summary>
        /// Get the input object for a given graph in the substance object.
        /// </summary>
        /// <param name="inputID">Input index.</param>
        /// <returns>Substance input object.</returns>
        public SubstanceInputBase GetInputObject(int inputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)_fileGraphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                if (result == ErrorCode.SBSARIO_ERROR_FAILURE)
                {
                    Debug.LogWarning($"Unable to load input from graphID: {_fileGraphID} and inputID:{inputID}. The input type is not supported.");
                    return SubstanceInputFactory.CreateInvalidInput(inputID);
                }

                throw new SubstanceException(result);
            }

            var input = SubstanceInputFactory.CreateInput(inputDesc);
            AssignInputDescription(inputID, input);
            return input;
        }

        /// <summary>
        /// Returns true if the target input should be visible in the editor.
        /// </summary>
        /// <param name="inputID">Target input ID.</param>
        /// <returns>True if the target input should be visible in the editor.</returns>
        public bool IsInputVisible(int inputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input_visibility(_handler, (IntPtr)_fileGraphID, (IntPtr)inputID, out NativeInputVisibility visibility);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            return (int)visibility.IsVisible == 1;
        }

        /// <summary>
        /// Sets a float input.
        /// </summary>
        /// <param name="inputID">Input index.</param>
        /// <param name="value">Input value.</param>
        public void SetInputFloat(int inputID, float value)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_FLOAT;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mFloatData0 = value;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)_fileGraphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }
        }

        /// <summary>
        /// Sets a float2 input.
        /// </summary>
        /// <param name="inputID">Input index.</param>
        /// <param name="value">Input value.</param>
        public void SetInputFloat2(int inputID, Vector2 value)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_FLOAT2;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mFloatData0 = value.x;
            inputData.Data.mFloatData1 = value.y;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)_fileGraphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }
        }

        /// <summary>
        /// Sets a float3 input.
        /// </summary>
        /// <param name="inputID">Input index.</param>
        /// <param name="value">Input value.</param>
        public void SetInputFloat3(int inputID, Vector3 value)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_FLOAT3;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mFloatData0 = value.x;
            inputData.Data.mFloatData1 = value.y;
            inputData.Data.mFloatData2 = value.z;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)_fileGraphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }
        }

        /// <summary>
        /// Sets a float4 input.
        /// </summary>
        /// <param name="inputID">Input index.</param>
        /// <param name="value">Input value.</param>
        public void SetInputFloat4(int inputID, Vector4 value)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_FLOAT4;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mFloatData0 = value.x;
            inputData.Data.mFloatData1 = value.y;
            inputData.Data.mFloatData2 = value.z;
            inputData.Data.mFloatData3 = value.w;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)_fileGraphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }
        }

        /// <summary>
        /// Sets a int input.
        /// </summary>
        /// <param name="inputID">Input index.</param>
        /// <param name="value">Input value.</param>
        public void SetInputInt(int inputID, int value)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_INT;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mIntData0 = value;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)_fileGraphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }
        }

        /// <summary>
        /// Sets a int2 input.
        /// </summary>
        /// <param name="inputID">Input index.</param>
        /// <param name="value">Input value.</param>
        public void SetInputInt2(int inputID, Vector2Int value)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_INT2;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mIntData0 = value.x;
            inputData.Data.mIntData1 = value.y;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)_fileGraphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }
        }

        /// <summary>
        /// Sets a int3 input.
        /// </summary>
        /// <param name="inputID">Input index.</param>
        /// <param name="value">Input value.</param>
        public void SetInputInt3(int inputID, Vector3Int value)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_INT3;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mIntData0 = value.x;
            inputData.Data.mIntData1 = value.y;
            inputData.Data.mIntData2 = value.z;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)_fileGraphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }
        }

        /// <summary>
        /// Sets a int4 input.
        /// </summary>
        /// <param name="inputID">Input index.</param>
        /// <param name="x">Input x value.</param>
        /// <param name="y">Input y value.</param>
        /// <param name="z">Input z value.</param>
        /// <param name="w">Input w value.</param>
        public void SetInputInt4(int inputID, int x, int y, int z, int w)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_INT4;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mIntData0 = x;
            inputData.Data.mIntData1 = y;
            inputData.Data.mIntData2 = z;
            inputData.Data.mIntData3 = w;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)_fileGraphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }
        }

        public void SetInputString(int inputID, string value)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_STRING;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mPtr = Marshal.StringToHGlobalAnsi(value);

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)_fileGraphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }
        }

        public void SetInputTexture2DNull(int inputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            var imageData = new NativeDataImage
            {
                channel_order = ChannelOrder.SBSARIO_CHANNEL_ORDER_RGBA,
                height = IntPtr.Zero,
                width = IntPtr.Zero
            };

            var numericData = new DataInternalNumeric
            {
                ImageData = imageData
            };

            NativeData inputData = new NativeData
            {
                DataType = DataType.SBSARIO_DATA_INPUT,
                ValueType = ValueType.SBSARIO_VALUE_IMAGE,
                Index = (IntPtr)inputID,
                Data = numericData
            };

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)_fileGraphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }
        }

        public void SetInputTexture2D(int inputID, Texture2D texture)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            if (texture.format == TextureFormat.RGBA64 || texture.format == TextureFormat.R16 || texture.format == TextureFormat.RGB48)
            {
                var data = texture.GetPixels();
                SetInputTexture2D_RGBA64(inputID, data, texture.width, texture.height);
                return;
            }
            else
            {
                var data = texture.GetPixels32();
                SetInputTexture2D_RGBA32(inputID, data, texture.width, texture.height);
            }
        }

        private void SetInputTexture2D_RGBA64(int inputID, Color[] pixelData, int width, int height)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            if (pixelData == null)
                return;

            pixelData = TextureExtensions.FlipY(pixelData, width, height);
            short[] textureBytes = TextureExtensions.Color64ArrayToShortArray(pixelData);
            IntPtr tempNativeMemory = Marshal.AllocHGlobal(textureBytes.Length * 2);
            Marshal.Copy(textureBytes, 0, tempNativeMemory, textureBytes.Length);

            var imageData = new NativeDataImage
            {
                channel_order = ChannelOrder.SBSARIO_CHANNEL_ORDER_RGBA,
                height = (IntPtr)width,
                width = (IntPtr)height,
                mipmaps = (IntPtr)0,
                image_format = TextureFormat.RGBA64.ToSubstance(),
                data = tempNativeMemory
            };

            var numericData = new DataInternalNumeric
            {
                ImageData = imageData
            };

            NativeData inputData = new NativeData
            {
                DataType = DataType.SBSARIO_DATA_INPUT,
                ValueType = ValueType.SBSARIO_VALUE_IMAGE,
                Index = (IntPtr)inputID,
                Data = numericData
            };

            try
            {
                ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)_fileGraphID, ref inputData);

                if (result != ErrorCode.SBSARIO_ERROR_OK)
                {
                    Debug.LogError($"Fail to update Substance input {inputID} for graph {_fileGraphID}");
                    throw new SubstanceException(result);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Marshal.FreeHGlobal(tempNativeMemory);
            }
        }

        private void SetInputTexture2D_RGBA32(int inputID, Color32[] pixelData, int width, int height)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            if (pixelData == null)
                return;

            pixelData = TextureExtensions.FlipY(pixelData, width, height);
            byte[] textureBytes = TextureExtensions.Color32ArrayToByteArray(pixelData);
            int textureSize = Marshal.SizeOf(textureBytes[0]) * textureBytes.Length;
            IntPtr tempNativeMemory = Marshal.AllocHGlobal(textureSize);
            Marshal.Copy(textureBytes, 0, tempNativeMemory, textureBytes.Length);

            var imageData = new NativeDataImage
            {
                channel_order = ChannelOrder.SBSARIO_CHANNEL_ORDER_RGBA,
                height = (IntPtr)width,
                width = (IntPtr)height,
                mipmaps = (IntPtr)0,
                image_format = TextureFormat.RGBA32.ToSubstance(),
                data = tempNativeMemory
            };

            var numericData = new DataInternalNumeric
            {
                ImageData = imageData
            };

            NativeData inputData = new NativeData
            {
                DataType = DataType.SBSARIO_DATA_INPUT,
                ValueType = ValueType.SBSARIO_VALUE_IMAGE,
                Index = (IntPtr)inputID,
                Data = numericData
            };

            try
            {
                ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)_fileGraphID, ref inputData);

                if (result != ErrorCode.SBSARIO_ERROR_OK)
                {
                    Debug.LogError($"Fail to update Substance input {inputID} for graph {_fileGraphID}");
                    throw new SubstanceException(result);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Marshal.FreeHGlobal(tempNativeMemory);
            }
        }

        public float GetInputFloat(int inputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)_fileGraphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }

            return inputDesc.Data.mFloatData0;
        }

        public Vector2 GetInputFloat2(int inputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)_fileGraphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }

            return new Vector2(inputDesc.Data.mFloatData0, inputDesc.Data.mFloatData1);
        }

        public Vector3 GetInputFloat3(int inputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)_fileGraphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }

            return new Vector3(inputDesc.Data.mFloatData0, inputDesc.Data.mFloatData1, inputDesc.Data.mFloatData2);
        }

        public Vector4 GetInputFloat4(int inputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)_fileGraphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }

            return new Vector4(inputDesc.Data.mFloatData0, inputDesc.Data.mFloatData1, inputDesc.Data.mFloatData2, inputDesc.Data.mFloatData2);
        }

        public int GetInputInt(int inputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)_fileGraphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }

            return inputDesc.Data.mIntData0;
        }

        public Vector2Int GetInputInt2(int inputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)_fileGraphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }

            return new Vector2Int(inputDesc.Data.mIntData0, inputDesc.Data.mIntData1);
        }

        public Vector3Int GetInputInt3(int inputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)_fileGraphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }

            return new Vector3Int(inputDesc.Data.mIntData0, inputDesc.Data.mIntData1, inputDesc.Data.mIntData2);
        }

        public int[] GetInputInt4(int inputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)_fileGraphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }

            return new int[] { inputDesc.Data.mIntData0, inputDesc.Data.mIntData1, inputDesc.Data.mIntData2, inputDesc.Data.mIntData2 };
        }

        public string GetInputString(int inputID)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)_fileGraphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }

            return Marshal.PtrToStringAnsi(inputDesc.Data.mPtr);
        }

        #endregion Input

        public IntPtr GetNativeHandle()
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            return _handler;
        }

        public int GetFileGraphID()
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            return _fileGraphID;
        }

        #region IDisposable

        public void Dispose()
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (_handler != default)
                {
                    NativeMethods.sbsario_sbsar_close(_handler);
                    _handler = default;
                }

                _disposedValue = true;
            }
        }

        public bool IsDisposed()
        {
            return _disposedValue;
        }

        ~SubstanceNativeGraph()
        {
            Dispose(disposing: false);
        }

        #endregion IDisposable

        private void AssignInputDescription(int inputID, SubstanceInputBase input)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input_desc(_handler, (IntPtr)_fileGraphID, (IntPtr)inputID, out NativeInputDesc inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            var identifier = inputDesc.mIdentifier == default ? null : Marshal.PtrToStringAnsi(inputDesc.mIdentifier);
            var label = inputDesc.mLabel == default ? null : Marshal.PtrToStringAnsi(inputDesc.mLabel);
            var guiGroup = inputDesc.GuiGroup == default ? null : Marshal.PtrToStringAnsi(inputDesc.GuiGroup);
            var guiDescription = inputDesc.GuiDescription == default ? null : Marshal.PtrToStringAnsi(inputDesc.GuiDescription);

            input.Description = new SubstanceInputDescription()
            {
                Identifier = identifier,
                Label = label,
                GuiGroup = guiGroup,
                GuiDescription = guiDescription,
                Type = ((ValueType)inputDesc.mValueType).ToUnity(),
                WidgetType = ((WidgetType)inputDesc.inputWidgetType).ToUnity()
            };

            if (input.IsNumeric)
                AssignNumericInputDescription(inputID, input);
        }

        private void AssignNumericInputDescription(int inputID, SubstanceInputBase substanceInput)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_numeric_input_desc(_handler, (IntPtr)_fileGraphID, (IntPtr)inputID, out NativeNumericInputDesc inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            substanceInput.SetNumericDescription(inputDesc);

            if ((int)(inputDesc.enumValueCount) != 0)
            {
                var count = (int)inputDesc.enumValueCount;

                NativeEnumInputDesc[] buffer = new NativeEnumInputDesc[count];
                GCHandle gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                IntPtr pointer = gcHandle.AddrOfPinnedObject();

                try
                {
                    result = (ErrorCode)NativeMethods.sbsario_sbsar_get_enum_input_desc(_handler, (IntPtr)_fileGraphID, (IntPtr)inputID, pointer, inputDesc.enumValueCount);
                    substanceInput.SetEnumOptions(buffer);

                    if (result != ErrorCode.SBSARIO_ERROR_OK)
                    {
                        Debug.LogError("Unable to get data for enum.");
                        return;
                    }
                }
                finally
                {
                    gcHandle.Free();
                }
            }
        }

        public Vector3 GetPhysicalSize()
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(SubstanceNativeGraph));

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_physical_size(_handler, (IntPtr)_fileGraphID, out NativePhysicalSize nativePhysicalSize);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input physical size for graph {_fileGraphID}");
                throw new SubstanceException(result);
            }

            return new Vector3(nativePhysicalSize.X, nativePhysicalSize.Y, nativePhysicalSize.Z);
        }
    }
} // namespace Adobe.Substance