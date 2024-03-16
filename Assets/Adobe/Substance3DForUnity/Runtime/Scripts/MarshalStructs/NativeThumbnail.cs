using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Adobe.Substance
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeThumbnail
    {
        public IntPtr Size;
        public IntPtr Data;
    }
}