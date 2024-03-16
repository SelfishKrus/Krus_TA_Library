using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance
{
    //! @brief Numeric type union
    //! @note The size will need to be changed if the API
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal struct DataInternalNumeric
    {
        [FieldOffset(0)]
        public int mIntData0;

        [FieldOffset(4)]
        public int mIntData1;

        [FieldOffset(8)]
        public int mIntData2;

        [FieldOffset(12)]
        public int mIntData3;

        [FieldOffset(0)]
        public float mFloatData0;

        [FieldOffset(4)]
        public float mFloatData1;

        [FieldOffset(8)]
        public float mFloatData2;

        [FieldOffset(12)]
        public float mFloatData3;

        [FieldOffset(0)]
        public IntPtr mPtr;

        [FieldOffset(0)]
        public NativeDataImage ImageData;
    }

    //! @brief Separate type for outputs
} // namespace Alg.Sbsario