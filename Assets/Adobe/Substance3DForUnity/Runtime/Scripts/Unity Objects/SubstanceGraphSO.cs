using Adobe.Substance.Input;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

#if UNITY_EDITOR

using Unity.Collections.LowLevel.Unsafe;

#endif

namespace Adobe.Substance
{
    [System.Serializable]
    public class SubstanceEditorPreset
    {
        public Substance.SubstancePresetInfo Info;

        public int PresetIndex;

        public string PresetValue;

        public bool NativePreset;
    }

    /// <summary>
    /// Scriptable object to store information about an instance of a sbsar file. Each instance has it own input values and output textures.
    /// </summary>
    public class SubstanceGraphSO : ScriptableObject
    {
        /// <summary>
        /// Path to the sbsar file that owns this instance. (Editor only)
        /// </summary>
        [SerializeField]
        public string AssetPath = default;

        /// <summary>
        /// Scriptable object that holds sbsar file binary data.
        /// </summary>
        [SerializeField]
        public SubstanceFileRawData RawData = default;

        /// <summary>
        /// Name for the instance.
        /// </summary>
        [SerializeField]
        public string Name = default;

        /// <summary>
        /// Is root instance.
        /// </summary>
        [SerializeField]
        public bool IsRoot = false;

        /// <summary>
        /// Signalized for the API that this instance should be deleted. (Editor only)
        /// </summary>
        [SerializeField]
        public bool FlagedForDelete = false;

        /// <summary>
        /// GUI to uniquely identify this instance during runtime.
        /// </summary>
        [SerializeField]
        public string GUID = default;

        /// <summary>
        /// Graph index in the sbsar file. This is a property that is imported from the sbsar file and should not be assigned in Unity.
        /// </summary>
        [SerializeField]
        public int Index = -1;

        /// <summary>
        /// Input list.
        /// </summary>
        [SerializeReference]
        public List<ISubstanceInput> Input = default;

        /// <summary>
        /// Output list.
        /// </summary>
        [SerializeField]
        public List<SubstanceOutputTexture> Output = default;

        /// <summary>
        /// True if this graph has physical size.
        /// </summary>
        [SerializeField]
        public bool HasPhysicalSize;

        /// <summary>
        /// Graph physical size. If HasPhysicalSize is false this will be Vector3.zero.
        /// </summary>
        [SerializeField]
        public Vector3 PhysicalSize;

        /// <summary>
        /// If se to true physical size will be applyed to the material.
        /// </summary>
        [SerializeField]
        public bool EnablePhysicalSize;

        /// <summary>
        /// Sbsar file thumbnail data.
        /// </summary>
        [SerializeField]
        public byte[] Thumbnail = default;

        /// <summary>
        /// True if sbsar file has thumbnail.
        /// </summary>
        [SerializeField]
        public bool HasThumbnail = false;

        /// <summary>
        /// Preset that holds the current state of the inputs. (Editor only)
        /// </summary>
        [SerializeField]
        public string CurrentStatePreset = default;

        /// <summary>
        /// If true, this asset will not generate tga files for the output textures.
        /// </summary>
        [SerializeField]
        public bool IsRuntimeOnly = false;

        /// <summary>
        /// True if this graph should generate all outputs.
        /// </summary>
        [SerializeField]
        public bool GenerateAllOutputs = false;

        /// <summary>
        /// True if this graph should generate mipmap chain.
        /// </summary>
        [SerializeField]
        public bool GenerateAllMipmaps = false;

        /// <summary>
        /// Output material.
        /// </summary>
        [SerializeField]
        public Material OutputMaterial = default;

        /// <summary>
        /// Default preset for the sbsar file.
        /// </summary>
        [SerializeField]
        public string DefaultPreset = default;

        /// <summary>
        /// Cached material shader name. (Editor only)
        /// </summary>
        [SerializeField]
        public string MaterialShader = default;

        /// <summary>
        /// Flags that the substance native inputs should be updated and material should be rendered. (Editor only)
        /// </summary>
        [SerializeField, HideInInspector]
        public bool RenderTextures = false;

        /// <summary>
        /// Flags that the current generated textures should be deleted. (Editor only)
        /// </summary>
        [SerializeField, HideInInspector]
        public bool OutputRemaped = false;

        [SerializeField, HideInInspector]
        public List<SubstanceEditorPreset> Presets;

        /// <summary>
        /// Initialized the substance graph. Uses the native handle to set all the input parameters, configure output textures, create Unity Texture2D objects for each output and properly assign them to the target material.
        /// This must be called if the substance graph was flagged as Runtime only and will require its assets be generated at runtime.
        /// </summary>
        /// <param name="handler">Handle to a native substance object</param>
        public void RuntimeInitialize(SubstanceNativeGraph handler, bool isRuntime = false)
        {
            if (handler.IsInitialized)
                return;

            foreach (var input in Input)
                input.UpdateNativeHandle(handler);

            RenderingUtils.ConfigureOutputTextures(handler, this, isRuntime);

            if (isRuntime)
            {
                var result = handler.Render();
                CreateAndUpdateOutputTextures(result, isRuntime);
                MaterialUtils.AssignOutputTexturesToMaterial(this);
            }

            handler.IsInitialized = true;
        }

        public Texture2D GetThumbnailTexture()
        {
            if (!HasThumbnail)
                return null;

            Texture2D thumbnailTexture = new Texture2D(0, 0);
            thumbnailTexture.LoadImage(Thumbnail);
            return thumbnailTexture;
        }

        public void CreateAndUpdateOutputTextures(IntPtr resultPtr, bool runtimeUsage = false)
        {
            unsafe
            {
                for (int i = 0; i < Output.Count; i++)
                {
                    var output = Output[i];

                    if (!output.IsStandardOutput(OutputMaterial) && !GenerateAllOutputs)
                        continue;

                    var index = output.VirtualOutputIndex;
                    IntPtr pI = resultPtr + (index * sizeof(NativeData));
                    NativeData data = Marshal.PtrToStructure<NativeData>(pI);

                    if (data.ValueType != ValueType.SBSARIO_VALUE_IMAGE)
                    {
                        Debug.LogError($"Skiping render index #{index} of {output.Description.Channel} because it was not an image");
                        continue;
                    }

                    if (data.ValueType == ValueType.SBSARIO_VALUE_IMAGE)
                    {
                        NativeDataImage imgData = data.Data.ImageData;

                        if (TryGetUnityTextureFormat(imgData, runtimeUsage, out int width, out int height, out int imageSize, out TextureFormat format, out int mipsCount))
                        {
                            var texture = new Texture2D(width, height, format, GenerateAllMipmaps, IsRuntimeOnly ? !output.sRGB : output.sRGB);
#if UNITY_EDITOR
                            texture.alphaIsTransparency = imgData.image_format.ChannelCount() == 4;
#endif
                            output.OutputTexture = texture;
                            texture.Apply();
                        }
                    }
                }
            }

            UpdateOutputTextures(resultPtr);
        }

        public void UpdateOutputTextures(IntPtr renderResultPtr)
        {
            unsafe
            {
                foreach (var output in Output)
                {
                    var texture = output.OutputTexture;

                    if (texture == null)
                    {
                        continue;
                    }

                    var index = output.VirtualOutputIndex;
                    IntPtr dataPtr = renderResultPtr + (index * sizeof(NativeData));
                    NativeData data = Marshal.PtrToStructure<NativeData>(dataPtr);

                    if (data.ValueType != ValueType.SBSARIO_VALUE_IMAGE)
                    {
                        Debug.LogError($"Fail to update substance output: output is not an image.");
                        continue;
                    }

                    NativeDataImage srcImage = data.Data.ImageData;

                    if ((srcImage.height.ToInt32() != texture.height) ||
                        (srcImage.width.ToInt32() != texture.width))
                    {
#if UNITY_2021_2_OR_NEWER
                        texture.Reinitialize(srcImage.width.ToInt32(), srcImage.height.ToInt32());
#else
                        texture.Resize(srcImage.width.ToInt32(), srcImage.height.ToInt32());
#endif
                    }

                    if (texture.format != TextureFormat.RGBA32 && texture.format != TextureFormat.BGRA32)
                    {
                        var size = GenerateAllMipmaps ? srcImage.GetSizeWithMipMaps() : srcImage.GetSize();

                        byte[] managedArray = new byte[size];
                        Marshal.Copy(srcImage.data, managedArray, 0, size);

                        var pixels32 = texture.GetPixels32();

                        for (int i = 0; i < pixels32.Length; i++)
                        {
                            pixels32[i].r = managedArray[(i * 4)];
                            pixels32[i].g = managedArray[(i * 4) + 1];
                            pixels32[i].b = managedArray[(i * 4) + 2];
                            pixels32[i].a = managedArray[(i * 4) + 3];
                        }

                        texture.Apply();
                    }
                    else
                    {
                        var size = GenerateAllMipmaps ? srcImage.GetSizeWithMipMaps() : srcImage.GetSize();
                        texture.LoadRawTextureData(srcImage.data, size);
                        texture.Apply();
                    }
                    
                }
            }
        }

        /// <summary>
        /// Returns a list of texture outputs that have resized since last render.
        /// </summary>
        /// <param name="resultPtr">Render result.</param>
        /// <returns>List of pair with output ID and new size.</returns>
        public List<(int, Vector2Int)> GetResizedOutputs(IntPtr resultPtr)
        {
            List<(int, Vector2Int)> textureSizes = new List<(int, Vector2Int)>();

            unsafe
            {
                foreach (var output in Output)
                {
                    var texture = output.OutputTexture;

                    if (texture == null)
                        continue;

                    var index = output.VirtualOutputIndex;
                    IntPtr pI = resultPtr + (index * sizeof(NativeData));
                    NativeData data = Marshal.PtrToStructure<NativeData>(pI);

                    if (data.ValueType != ValueType.SBSARIO_VALUE_IMAGE)
                    {
                        Debug.LogError($"Results fail for {output.Description.Label}");
                        continue;
                    }

                    NativeDataImage imgData = data.Data.ImageData;

                    if (texture.width != (int)imgData.width || texture.height != (int)imgData.height)
                        textureSizes.Add((output.Index, new Vector2Int((int)imgData.width, (int)imgData.height)));
                }
            }

            return textureSizes;
        }

        private static bool TryGetUnityTextureFormat(NativeDataImage nativeData, bool runtimeUsage, out int width, out int height, out int imageSize, out TextureFormat format, out int mipsCount)
        {
            width = (int)nativeData.width;
            height = (int)nativeData.height;
            mipsCount = (int)nativeData.mipmaps;

            imageSize = nativeData.GetSizeWithMipMaps();
            format = nativeData.image_format.ToUnityFormat();

            if ((nativeData.channel_order == ChannelOrder.SBSARIO_CHANNEL_ORDER_BGRA) && runtimeUsage)
                format = TextureFormat.BGRA32;

            //#endif
            return true;
        }

        /// <summary>
        /// Returns the native ID of this graph in the sbsar file.
        /// </summary>
        /// <returns>Graph native ID.</returns>
        public int GetNativeID()
        {
            return Index;
        }

        /// <summary>
        /// Sets the value for the native index of this graph. (This should only be assigned on object creation based on the values from the sbsar file.)
        /// </summary>
        public void SetNativeID(int index)
        {
            if (Index != -1)
                throw new ArgumentException("SubstanceGraphSO NativeID should not be changed after it has been assigned.");

            Index = index;
        }
    }
}