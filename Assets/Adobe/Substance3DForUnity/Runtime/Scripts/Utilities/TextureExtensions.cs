using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Adobe.Substance
{
    /// <summary>
    /// Provides utility extensions to copy data from substance to unity textures.
    /// </summary>
    internal static class TextureExtensions
    {
        internal static byte[] Color32ArrayToByteArray(Color32[] colors)
        {
            if (colors == null || colors.Length == 0)
                return null;

            int length = Marshal.SizeOf(typeof(Color32)) * colors.Length;
            byte[] bytes = new byte[length];

            GCHandle handle = default(GCHandle);

            try
            {
                handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(ptr, bytes, 0, length);
            }
            finally
            {
                if (handle != default(GCHandle))
                    handle.Free();
            }

            return bytes;
        }

        internal static short[] Color64ArrayToShortArray(Color[] colors)
        {
            if (colors == null || colors.Length == 0)
                return null;

            short[] lonadArray = new short[colors.Length * 4];

            for (int i = 0; i < colors.Length; i++)
            {
                lonadArray[i * 4] = (short)(colors[i].r * (float)UInt16.MaxValue);
                lonadArray[(i * 4) + 1] = (short)(colors[i].g * (float)UInt16.MaxValue);
                lonadArray[(i * 4) + 2] = (short)(colors[i].b * (float)UInt16.MaxValue);
                lonadArray[(i * 4) + 3] = (short)(colors[i].a * (float)UInt16.MaxValue);
            }

            return lonadArray;
        }

        internal static T[] FlipY<T>(T[] input, int width, int height)
        {
            T[] output = new T[input.Length];

            for (int y = 0, i = 0, o = output.Length - width; y < height; y++, i += width, o -= width)
                Array.Copy(input, i, output, o, width);

            return output;
        }
    }
}