using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance
{
    /// <summary>
    /// Extensions for handling enum type convertion.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Transforms a unity value type enum into an enum used by native code.
        /// </summary>
        /// <param name="unityType">Unity enum.</param>
        /// <returns>Native enum.</returns>
        internal static ValueType ToNative(this SubstanceValueType unityType)
        {
            switch (unityType)
            {
                case SubstanceValueType.Float:
                    return ValueType.SBSARIO_VALUE_FLOAT;

                case SubstanceValueType.Float2:
                    return ValueType.SBSARIO_VALUE_FLOAT2;

                case SubstanceValueType.Float3:
                    return ValueType.SBSARIO_VALUE_FLOAT3;

                case SubstanceValueType.Float4:
                    return ValueType.SBSARIO_VALUE_FLOAT4;

                case SubstanceValueType.Int:
                    return ValueType.SBSARIO_VALUE_INT;

                case SubstanceValueType.Int2:
                    return ValueType.SBSARIO_VALUE_INT2;

                case SubstanceValueType.Int3:
                    return ValueType.SBSARIO_VALUE_INT3;

                case SubstanceValueType.Int4:
                    return ValueType.SBSARIO_VALUE_INT4;

                case SubstanceValueType.Image:
                    return ValueType.SBSARIO_VALUE_IMAGE;

                case SubstanceValueType.String:
                    return ValueType.SBSARIO_VALUE_STRING;

                case SubstanceValueType.Font:
                    return ValueType.SBSARIO_VALUE_FONT;

                default:
                    throw new System.InvalidOperationException($"Value type {unityType} can not be converted to native enum.");
            }
        }

        /// <summary>
        /// Transforms a native value type enum into an enum used by the unity plugin.
        /// </summary>
        /// <param name="nativeType">Native enum.</param>
        /// <returns>Unity enum.</returns>
        internal static SubstanceValueType ToUnity(this ValueType nativeType)
        {
            switch (nativeType)
            {
                case ValueType.SBSARIO_VALUE_FLOAT:
                    return SubstanceValueType.Float;

                case ValueType.SBSARIO_VALUE_FLOAT2:
                    return SubstanceValueType.Float2;

                case ValueType.SBSARIO_VALUE_FLOAT3:
                    return SubstanceValueType.Float3;

                case ValueType.SBSARIO_VALUE_FLOAT4:
                    return SubstanceValueType.Float4;

                case ValueType.SBSARIO_VALUE_INT:
                    return SubstanceValueType.Int;

                case ValueType.SBSARIO_VALUE_INT2:
                    return SubstanceValueType.Int2;

                case ValueType.SBSARIO_VALUE_INT3:
                    return SubstanceValueType.Int3;

                case ValueType.SBSARIO_VALUE_INT4:
                    return SubstanceValueType.Int4;

                case ValueType.SBSARIO_VALUE_IMAGE:
                    return SubstanceValueType.Image;

                case ValueType.SBSARIO_VALUE_STRING:
                    return SubstanceValueType.String;

                case ValueType.SBSARIO_VALUE_FONT:
                    return SubstanceValueType.Font;

                default:
                    throw new System.InvalidOperationException($"Value type {nativeType} can not be converted to unity enum.");
            }
        }

        internal static SubstanceWidgetType ToUnity(this WidgetType nativeType)
        {
            switch (nativeType)
            {
                case WidgetType.SBSARIO_WIDGET_NOWIDGET:
                    return SubstanceWidgetType.NoWidget;

                case WidgetType.SBSARIO_WIDGET_SLIDER:
                    return SubstanceWidgetType.Slider;

                case WidgetType.SBSARIO_WIDGET_ANGLE:
                    return SubstanceWidgetType.Angle;

                case WidgetType.SBSARIO_WIDGET_COLOR:
                    return SubstanceWidgetType.Color;

                case WidgetType.SBSARIO_WIDGET_TOGGLEBUTTON:
                    return SubstanceWidgetType.ToggleButton;

                case WidgetType.SBSARIO_WIDGET_ENUMBUTTONS:
                    return SubstanceWidgetType.EnumButton;

                case WidgetType.SBSARIO_WIDGET_COMBOBOX:
                    return SubstanceWidgetType.ComboBox;

                case WidgetType.SBSARIO_WIDGET_IMAGE:
                    return SubstanceWidgetType.Image;

                case WidgetType.SBSARIO_WIDGET_POSITION:
                    return SubstanceWidgetType.Position;

                default:
                    throw new System.InvalidOperationException($"Value type {nativeType} can not be converted to unity enum.");
            }
        }

        internal static string GetMessage(this ErrorCode code)
        {
            switch (code)
            {
                case ErrorCode.SBSARIO_ERROR_OK:
                    return "No error has occurred";

                case ErrorCode.SBSARIO_ERROR_STATE:
                    return "Call made with an invalid state";

                case ErrorCode.SBSARIO_ERROR_INVALID:
                    return "An invalid argument was given to the api";

                case ErrorCode.SBSARIO_ERROR_UNKNOWN:
                    return "An unspecified error has occurred";

                case ErrorCode.SBSARIO_ERROR_FAILURE:
                    return "The operation failed to complete";

                default:
                    return "No error has occurred";
            }
        }

        public static HVFlip ToSubstance(this TextureFlip flip)
        {
            switch (flip)
            {
                case TextureFlip.None:
                    return HVFlip.SBSARIO_HVFLIP_NO;

                case TextureFlip.Horizontal:
                    return HVFlip.SBSARIO_HVFLIP_HORIZONTAL;

                case TextureFlip.Vertical:
                    return HVFlip.SBSARIO_HVFLIP_VERTICAL;

                case TextureFlip.Both:
                    return HVFlip.SBSARIO_HVFLIP_BOTH;

                default:
                    throw new System.InvalidOperationException($"Unable to convert {flip} to native enum.");
            }
        }

        public static ChannelOrder GetChannelOrder(this TextureFormat unityFormat)
        {
            switch (unityFormat)
            {
                case TextureFormat.RGB24:
                case TextureFormat.RGBA32:
                case TextureFormat.RGBA64:
                case TextureFormat.RGB48:
                    return ChannelOrder.SBSARIO_CHANNEL_ORDER_RGBA;

                case TextureFormat.BGRA32:
                    return ChannelOrder.SBSARIO_CHANNEL_ORDER_BGRA;

                default:
                    return ChannelOrder.SBSARIO_CHANNEL_ORDER_INVALID;
            }
        }

        #region Image Format

        public static ImageFormat ToSubstance(this TextureFormat unityFormat)
        {
            switch (unityFormat)
            {
                //Byte types.
                case TextureFormat.BGRA32:
                case TextureFormat.RGBA32:
                    return ImageFormat.SBSARIO_IMAGE_FORMAT_8B | ImageFormat.SBSARIO_IMAGE_FORMAT_RGBA | ImageFormat.SBSARIO_IMAGE_FORMAT_INT;

                case TextureFormat.RGB24:
                    return ImageFormat.SBSARIO_IMAGE_FORMAT_8B | ImageFormat.SBSARIO_IMAGE_FORMAT_RGB | ImageFormat.SBSARIO_IMAGE_FORMAT_INT;

                case TextureFormat.R8:
                    return ImageFormat.SBSARIO_IMAGE_FORMAT_8B | ImageFormat.SBSARIO_IMAGE_FORMAT_L | ImageFormat.SBSARIO_IMAGE_FORMAT_INT;

                //Short types
                case TextureFormat.R16:
                    return ImageFormat.SBSARIO_IMAGE_FORMAT_16B | ImageFormat.SBSARIO_IMAGE_FORMAT_L | ImageFormat.SBSARIO_IMAGE_FORMAT_INT;

                case TextureFormat.RGBA64:
                    return ImageFormat.SBSARIO_IMAGE_FORMAT_16B | ImageFormat.SBSARIO_IMAGE_FORMAT_RGBA | ImageFormat.SBSARIO_IMAGE_FORMAT_INT;

                //Float types
                case TextureFormat.RGBAFloat:
                    return ImageFormat.SBSARIO_IMAGE_FORMAT_32B | ImageFormat.SBSARIO_IMAGE_FORMAT_RGBA | ImageFormat.SBSARIO_IMAGE_FORMAT_FLOAT; 

                case TextureFormat.RFloat:
                    return ImageFormat.SBSARIO_IMAGE_FORMAT_32B | ImageFormat.SBSARIO_IMAGE_FORMAT_L | ImageFormat.SBSARIO_IMAGE_FORMAT_FLOAT;

                //Half types
                case TextureFormat.RGBAHalf:
                    return ImageFormat.SBSARIO_IMAGE_FORMAT_16B | ImageFormat.SBSARIO_IMAGE_FORMAT_RGBA | ImageFormat.SBSARIO_IMAGE_FORMAT_FLOAT; 

                case TextureFormat.RHalf:
                    return ImageFormat.SBSARIO_IMAGE_FORMAT_16B | ImageFormat.SBSARIO_IMAGE_FORMAT_L | ImageFormat.SBSARIO_IMAGE_FORMAT_FLOAT; 

                default:
                    throw new ArgumentException($"Texture format {unityFormat} not supported.");
            }
        }

        /// <summary>
        /// Convert Substance Texture Format to Unity Texture Format
        /// </summary>
        internal static TextureFormat ToUnityFormat(this ImageFormat imageFormat)
        {
            var channelCount = ChannelCount(imageFormat);
            var bytesPerChannel = GetBytesPerChannel(imageFormat);

            if (imageFormat.HasMask(ImageFormat.SBSARIO_IMAGE_FORMAT_FLOAT))
            {
                if (channelCount == 1)
                {
                    if (bytesPerChannel == 2)
                    {
                        return TextureFormat.RHalf;
                    }
                    else if (bytesPerChannel == 4)
                    {
                        return TextureFormat.RFloat;
                    }
                }
                else if (channelCount == 2)
                {
                    if (bytesPerChannel == 2)
                    {
                        return TextureFormat.RGHalf;
                    }
                    else if (bytesPerChannel == 4)
                    {
                        return TextureFormat.RGFloat;
                    }
                }
                else if (channelCount == 4)
                {
                    if (bytesPerChannel == 2)
                    {
                        return TextureFormat.RGBAHalf;
                    }
                    else if (bytesPerChannel == 4)
                    {
                        return TextureFormat.RGBAFloat;
                    }
                }
            }
            else
            {
                if (channelCount == 1)
                {
                    if (bytesPerChannel == 1)
                    {
                        return TextureFormat.R8;
                    }
                    else if (bytesPerChannel == 2)
                    {
                        return TextureFormat.R16;
                    }
                }
                else if (channelCount == 2)
                {
                    if (bytesPerChannel == 1)
                    {
                        return TextureFormat.RG16;
                    }
                    else if (bytesPerChannel == 2)
                    {
                        return TextureFormat.RG32;
                    }
                }
                else if (channelCount == 3)
                {
                    if (bytesPerChannel == 1)
                    {
                        return TextureFormat.RGB24;
                    }
                    else if (bytesPerChannel == 2)
                    {
                        return TextureFormat.RGB48;
                    }
                }
                else if (channelCount == 4)
                {
                    if (bytesPerChannel == 1)
                    {
                        return TextureFormat.RGBA32;
                    }
                    else if (bytesPerChannel == 2)
                    {
                        return TextureFormat.RGBA64;
                    }
                }
            }

            Debug.LogError($"Not supported image format with channel count = {channelCount} and bytes per channel = {bytesPerChannel}");
            throw new FormatException();
        }

        internal static int GetSizeWithMipMaps(this NativeDataImage imageData)
        {
            int width = (int)imageData.width;
            int height = (int)imageData.height;
            int mipCount = (int)imageData.mipmaps;
            ImageFormat imageFormat = imageData.image_format;

            var bytesPerPixel = GetBytesPerChannel(imageFormat) * ChannelCount(imageFormat);

            if (width == 0 || height == 0)
                return 0;

            int size = 0;
            for (int i = 0; i < mipCount; i++)
            {
                size += width * height * bytesPerPixel;
                width >>= 1;
                height >>= 1;

                if (width < 1) width = 1;
                if (height < 1) height = 1;
            }

            return size;
        }

        internal static int GetSize(this NativeDataImage imageData)
        {
            int width = (int)imageData.width;
            int height = (int)imageData.height;
            ImageFormat imageFormat = imageData.image_format;

            if (width <= 0 || height <= 0)
                return 0;

            var bytesPerPixel = GetBytesPerChannel(imageFormat) * ChannelCount(imageFormat);
            return width * height * bytesPerPixel;
        }

        internal static int GetBytesPerChannel(this ImageFormat imageFormat)
        {
            switch (imageFormat & ImageFormat.SBSARIO_IMAGE_FORMAT_BITDEPTH_MASK)
            {
                case ImageFormat.SBSARIO_IMAGE_FORMAT_32B:
                    return 4;

                case ImageFormat.SBSARIO_IMAGE_FORMAT_16B:
                    return 2;

                case ImageFormat.SBSARIO_IMAGE_FORMAT_8B:
                    return 1;

                default:
                    return 1;
            }
        }

        internal static int ChannelCount(this ImageFormat imageFormat)
        {
            switch (imageFormat & ImageFormat.SBSARIO_IMAGE_FORMAT_CHANNELS_MASK)
            {
                case ImageFormat.SBSARIO_IMAGE_FORMAT_RGBA:
                case ImageFormat.SBSARIO_IMAGE_FORMAT_RGBX:
                    return 4;

                case ImageFormat.SBSARIO_IMAGE_FORMAT_RGB:
                    return 3;

                case ImageFormat.SBSARIO_IMAGE_FORMAT_L:
                    return 1;

                default:
                    return 4;
            }
        }

        private static bool HasMask(this ImageFormat imageFormat, ImageFormat mask)
        {
            return ((imageFormat & mask) != 0);
        }

        #endregion Image Format
    }
}