using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adobe.Substance.Input.Description;
using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance.Input
{
    [System.Serializable]
    public class SubstanceInputFloat3 : SubstanceInputBase
    {
        public Vector3 Data;

        public SubstanceInputDescNumericalFloat3 NumericalDescription;

        public override SubstanceValueType ValueType => SubstanceValueType.Float3;
        public override bool IsNumeric => true;
        public override bool IsValid => true;

        internal SubstanceInputFloat3(int index, DataInternalNumeric data)
        {
            Index = index;
            Data = new Vector3(data.mFloatData0, data.mFloatData1, data.mFloatData2);
        }

        public override void UpdateNativeHandle(SubstanceNativeGraph handler)
        {
            handler.SetInputFloat3(Index, Data);
        }

        internal override void SetNumericDescription(NativeNumericInputDesc desc)
        {
            NumericalDescription = new SubstanceInputDescNumericalFloat3
            {
                DefaultValue = new Vector3(desc.default_value.mFloatData0, desc.default_value.mFloatData1, desc.default_value.mFloatData2),
                MaxValue = new Vector3(desc.max_value.mFloatData0, desc.max_value.mFloatData1, desc.max_value.mFloatData2),
                MinValue = new Vector3(desc.min_value.mFloatData0, desc.min_value.mFloatData1, desc.min_value.mFloatData2),
                SliderClamp = Convert.ToBoolean(desc.sliderClamp.ToInt32()),
                SliderStep = desc.sliderStep,
                EnumValueCount = desc.enumValueCount.ToInt32()
            };

            return;
        }

        internal override void SetEnumOptions(NativeEnumInputDesc[] options)
        {
            NumericalDescription.EnumValues = new SubstanceFloat3EnumOption[options.Length];

            for (int i = 0; i < NumericalDescription.EnumValues.Length; i++)
            {
                var option = new SubstanceFloat3EnumOption
                {
                    Label = Marshal.PtrToStringAnsi(options[i].label),
                    Value = new Vector3(options[i].value.mFloatData0, options[i].value.mFloatData1, options[i].value.mFloatData2)
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