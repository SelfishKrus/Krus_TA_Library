using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Adobe.Substance
{
    public class SubstanceVirtualOutputChannelInfo
    {
        public uint ChannelSource { get; }

        public ShuffleIndex Index { get; }

        public bool Invert { get; }

        private const uint SBSARIO_USE_DEFAULT = ~0u;

        private const uint SBSARIO_COMPONENT_EMPTY = 0;

        public SubstanceVirtualOutputChannelInfo(uint outputUID, ShuffleIndex index = ShuffleIndex.Red, bool invert = false)
        {
            ChannelSource = outputUID;
            Index = index;
            Invert = invert;
        }

        internal NativeOutputFormatComponent CreateNativeComponent()
        {
            return new NativeOutputFormatComponent()
            {
                levelMax = Invert ? 0 : 1,
                levelMin = Invert ? 1 : 0,
                outputIndex = ChannelSource,
                ShuffleIndex = Index
            };
        }

        public readonly static SubstanceVirtualOutputChannelInfo Default = new SubstanceVirtualOutputChannelInfo(SBSARIO_USE_DEFAULT);

        public readonly static SubstanceVirtualOutputChannelInfo Black = new SubstanceVirtualOutputChannelInfo(SBSARIO_COMPONENT_EMPTY, ShuffleIndex.Red);

        public static SubstanceVirtualOutputChannelInfo White = new SubstanceVirtualOutputChannelInfo(SBSARIO_COMPONENT_EMPTY, ShuffleIndex.Red, true);
    }

    public class SubstanceVirtualOutputCreateInfo
    {
        public TextureFormat Format { get; }

        public string Label { get; }

        public TextureFlip FlipOption { get; }

        public SubstanceVirtualOutputChannelInfo RedChannelSource { get; }

        public SubstanceVirtualOutputChannelInfo GreenChannelSource { get; }

        public SubstanceVirtualOutputChannelInfo BlueChannelSource { get; }

        public SubstanceVirtualOutputChannelInfo AlphaChannelSource { get; }

        public SubstanceVirtualOutputCreateInfo(TextureFormat format,
                                                string name,
                                                TextureFlip flip = TextureFlip.None,
                                                params SubstanceVirtualOutputChannelInfo[] channels)
        {
            Format = format;
            Label = name;
            FlipOption = flip;
            RedChannelSource = channels.Length > 0 ? channels[0] : SubstanceVirtualOutputChannelInfo.Default;
            GreenChannelSource = channels.Length > 1 ? channels[1] : SubstanceVirtualOutputChannelInfo.Default;
            BlueChannelSource = channels.Length > 2 ? channels[2] : SubstanceVirtualOutputChannelInfo.Default;
            AlphaChannelSource = channels.Length > 3 ? channels[3] : SubstanceVirtualOutputChannelInfo.Default;
        }

        internal NativeOutputDesc CreateOutputDesc()
        {
            var labelPtr = Marshal.StringToHGlobalAnsi(Label);

            return new NativeOutputDesc()
            {
                mLabel = labelPtr,
                mIdentifier = labelPtr,
                mValueType = ValueType.SBSARIO_VALUE_IMAGE,
            };
        }

        internal NativeOutputFormat CreateOutputFormat()
        {
            var format = new NativeOutputFormat
            {
                hvFlip = FlipOption.ToSubstance(),
                format = (uint)Format.ToSubstance()
            };

            format.ChannelComponent0 = RedChannelSource.CreateNativeComponent();
            format.ChannelComponent1 = GreenChannelSource.CreateNativeComponent();
            format.ChannelComponent2 = BlueChannelSource.CreateNativeComponent();
            format.ChannelComponent3 = AlphaChannelSource.CreateNativeComponent();
            return format;
        }
    }
}