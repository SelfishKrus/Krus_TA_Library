using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Adobe.Substance
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeInputVisibility
    {
        public IntPtr Index;

        public IntPtr IsVisible;
    }
}