using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adobe.Substance.Input.Description;
using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance.Input
{
    [System.Serializable]
    public class SubstanceInputFloat4 : SubstanceInputBase
    {
        public Vector4 Data;

        public SubstanceInputDescNumericalFloat4 NumericalDescription;

        public override bool IsNumeric => true;
        public override bool IsValid => true;
        public override SubstanceValueType ValueType => SubstanceValueType.Float4;

        internal SubstanceInputFloat4(int index, DataInternalNumeric data)
        {
            Index = index;
            Data = new Vector4(data.mFloatData0, data.mFloatData1, data.mFloatData2, data.mFloatData3);
        }

        public override void UpdateNativeHandle(SubstanceNativeGraph handler)
        {
            handler.SetInputFloat4(Index, Data);
        }

        internal override void SetNumericDescription(NativeNumericInputDesc desc)
        {
            NumericalDescription = new SubstanceInputDescNumericalFloat4
            {
                DefaultValue = new Vector4(desc.default_value.mFloatData0, desc.default_value.mFloatData1, desc.default_value.mFloatData2, desc.default_value.mFloatData3),
                MaxValue = new Vector4(desc.max_value.mFloatData0, desc.max_value.mFloatData1, desc.max_value.mFloatData2, desc.max_value.mFloatData3),
                MinValue = new Vector4(desc.min_value.mFloatData0, desc.min_value.mFloatData1, desc.min_value.mFloatData2, desc.min_value.mFloatData3),
                SliderClamp = Convert.ToBoolean(desc.sliderClamp.ToInt32()),
                SliderStep = desc.sliderStep,
                EnumValueCount = desc.enumValueCount.ToInt32()
            };

            return;
        }

        internal override void SetEnumOptions(NativeEnumInputDesc[] options)
        {
            NumericalDescription.EnumValues = new SubstanceFloat4EnumOption[options.Length];

            for (int i = 0; i < NumericalDescription.EnumValues.Length; i++)
            {
                var option = new SubstanceFloat4EnumOption
                {
                    Label = Marshal.PtrToStringAnsi(options[i].label),
                    Value = new Vector4(options[i].value.mFloatData0, options[i].value.mFloatData1, options[i].value.mFloatData2, options[i].value.mFloatData3)
                };

                NumericalDescription.EnumValues[i] = option;
            }

            return;
        }

        public override bool TryGetNumericalDescription(out ISubstanceInputDescNumerical description)
        {
            description = NumericalDescription;
            return true;
        }
    }
}