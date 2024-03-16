using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance
{
    //! @brief Managed representation of the native sbsario output desc type
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeOutputDesc
    {
        //! @brief Unique string identifier of the output
        public IntPtr mIdentifier;

        //! @brief Display label for the output
        public IntPtr mLabel;

        //! @brief Index of the output
        public UIntPtr mIndex;

        //! @brief Image output format.
        public IntPtr mFormat;

        //! @brief Type of the output
        public ValueType mValueType;

        //! @brief Default usage for the output
        public IntPtr mChannelUsage;
    }
} // namespace Alg.Sbsario