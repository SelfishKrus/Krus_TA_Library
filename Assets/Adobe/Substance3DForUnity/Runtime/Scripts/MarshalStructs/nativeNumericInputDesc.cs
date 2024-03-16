using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Adobe.Substance
{
    //! @brief Numeric type union
    //! @note The size will need to be changed if the API
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal struct NumericDescriptValue
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
    }

    //! @brief Managed representation of the native sbsario input desc type
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeNumericInputDesc
    {
        public IntPtr index;

        /** @brief Unique string identifier for x axis of a vector input. ('X' if nullptr) */
        public IntPtr xLabel;

        /** @brief Unique string identifier for y axis of a vector input. ('Y' if nullptr) */
        public IntPtr yLabel;

        /** @brief Unique string identifier for z axis of a vector input. ('Z' if nullptr) */
        public IntPtr zLabel;

        /** @brief Unique string identifier for w axis of a vector input. ('W' if nullptr) */
        public IntPtr wLabel;

        /** @brief Unique string identifier for the false state of a bool int. */
        public IntPtr LabelFalse;

        /** @brief Unique string identifier for the true state of a bool int. */
        public IntPtr LabelTrue;

        /** @brief Step to be used for a slider input. */
        public float sliderStep;

        /** @brief Bool value that determs if the slider must clamp. */
        public IntPtr sliderClamp;

        /** @brief Internal data, of which the valid type is determined by
                   the value_type member.
        */
        public NumericDescriptValue default_value;

        public NumericDescriptValue min_value;

        public NumericDescriptValue max_value;

        public IntPtr enumValueCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeEnumInputDesc
    {
        public IntPtr label;

        public NumericDescriptValue value;
    }
}