using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Adobe.Substance
{
    internal static class NativeConsts
    {
        public const uint UseDefault = ~0u;
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal struct NativeOutputFormatComponent
    {
        [FieldOffset(0)]
        public uint outputIndex;

        [FieldOffset(4)]
        public ShuffleIndex ShuffleIndex;

        [FieldOffset(8)]
        public float levelMin;

        [FieldOffset(12)]
        public float levelMax;

        public static NativeOutputFormatComponent CreateDefault()
        {
            return new NativeOutputFormatComponent
            {
                outputIndex = NativeConsts.UseDefault,
                levelMin = NativeConsts.UseDefault,
                levelMax = NativeConsts.UseDefault
            };
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 84)]
    internal struct NativeOutputFormat
    {
        [FieldOffset(0)]
        public uint format;

        [FieldOffset(4)]
        public uint mipmapLevelsCount;

        [FieldOffset(8)]
        public HVFlip hvFlip;

        [FieldOffset(12)]
        public uint forceWidth;

        [FieldOffset(16)]
        public uint forceHeight;

        [FieldOffset(20)]
        public NativeOutputFormatComponent ChannelComponent0;

        [FieldOffset(36)]
        public NativeOutputFormatComponent ChannelComponent1;

        [FieldOffset(52)]
        public NativeOutputFormatComponent ChannelComponent2;

        [FieldOffset(68)]
        public NativeOutputFormatComponent ChannelComponent3;

        public static NativeOutputFormat CreateDefault()
        {
            return new NativeOutputFormat
            {
                format = NativeConsts.UseDefault,
                mipmapLevelsCount = NativeConsts.UseDefault,
                hvFlip = HVFlip.SBSARIO_HVFLIP_NO,
                forceWidth = NativeConsts.UseDefault,
                forceHeight = NativeConsts.UseDefault,

                ChannelComponent0 = NativeOutputFormatComponent.CreateDefault(),
                ChannelComponent1 = NativeOutputFormatComponent.CreateDefault(),
                ChannelComponent2 = NativeOutputFormatComponent.CreateDefault(),
                ChannelComponent3 = NativeOutputFormatComponent.CreateDefault()
            };
        }
    }
}