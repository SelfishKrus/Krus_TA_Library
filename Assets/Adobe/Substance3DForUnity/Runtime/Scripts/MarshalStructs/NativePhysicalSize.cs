using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativePhysicalSize
    {
        public float X;
        public float Y;
        public float Z;
    }
} // namespace Adobe.Substance