using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adobe.Substance.Input.Description;
using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance.Input
{
    [System.Serializable]
    public class SubstanceInputFloat2 : SubstanceInputBase
    {
        public Vector2 Data;

        public SubstanceInputDescNumericalFloat2 NumericalDescription;

        public override SubstanceValueType ValueType => SubstanceValueType.Float2;
        public override bool IsNumeric => true;
        public override bool IsValid => true;

        internal SubstanceInputFloat2(int index, DataInternalNumeric data)
        {
            Index = index;
            Data = new Vector2(data.mFloatData0, data.mFloatData1);
        }

        public override void UpdateNativeHandle(SubstanceNativeGraph handler)
        {
            handler.SetInputFloat2(Index, Data);
        }

        internal override void SetNumericDescription(NativeNumericInputDesc desc)
        {
            NumericalDescription = new SubstanceInputDescNumericalFloat2
            {
                DefaultValue = new Vector2(desc.default_value.mFloatData0, desc.default_value.mFloatData1),
                MaxValue = new Vector2(desc.max_value.mFloatData0, desc.max_value.mFloatData1),
                MinValue = new Vector2(desc.min_value.mFloatData0, desc.min_value.mFloatData1),
                SliderClamp = Convert.ToBoolean(desc.sliderClamp.ToInt32()),
                SliderStep = desc.sliderStep,
                EnumValueCount = desc.enumValueCount.ToInt32()
            };

            return;
        }

        internal override void SetEnumOptions(NativeEnumInputDesc[] options)
        {
            NumericalDescription.EnumValues = new SubstanceFloat2EnumOption[options.Length];

            for (int i = 0; i < NumericalDescription.EnumValues.Length; i++)
            {
                var option = new SubstanceFloat2EnumOption
                {
                    Label = Marshal.PtrToStringAnsi(options[i].label),
                    Value = new Vector2(options[i].value.mFloatData0, options[i].value.mFloatData1)
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