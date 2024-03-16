using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeDataImage
    {
        /** @brief Pointer to the underlying image data */
        public IntPtr data;

        /** @brief The width in pixels of the larget mipmap */
        public IntPtr width;

        /** @brief The height in pixels of the largest mipmap */
        public IntPtr height;

        /** @brief The number of mipmaps in the chain. The largest map (level 0)
                   will be the first in memory, with width/height as its dimensions
        */
        public IntPtr mipmaps;

        /** @brief Channel order enum, describing the channel index order */
        public ChannelOrder channel_order;

        /** @brief Image format enum, describing the bitdepth and channel size
                   of the image in memory
        */
        public ImageFormat image_format;

        public override string ToString()
        {
            return $"width:{width} \n " +
                   $"height:{height}\n" +
                   $"mipmaps:{mipmaps}\n" +
                   $"image_format:{(int)image_format} \n" +
                   $"channel_order:{(int)channel_order}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeData
    {
        /** @brief Type descriptor of the data value */
        public ValueType ValueType;

        /** @brief Descriptor of whether the data is for inputs or outputs */
        public DataType DataType;

        /** @brief Data index that this is associated with, either of the input
                   if it is input data, or of the output if it is output/result
                   data. */
        public IntPtr Index;

        /** @brief Internal data, of which the valid type is determined by
                   the value_type member.*/
        public DataInternalNumeric Data;
    }
} // namespace Adobe.Substance