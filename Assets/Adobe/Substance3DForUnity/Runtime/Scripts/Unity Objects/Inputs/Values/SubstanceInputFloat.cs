using Adobe.Substance.Input.Description;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Adobe.Substance.Input
{
    [Serializable]
    public class SubstanceInputFloat : SubstanceInputBase
    {
        public float Data;

        public SubstanceInputDescNumericalFloat NumericalDescription;

        public override bool IsNumeric => true;
        public override bool IsValid => true;
        public override SubstanceValueType ValueType => SubstanceValueType.Float;

        internal SubstanceInputFloat(int index, DataInternalNumeric data)
        {
            Index = index;
            Data = data.mFloatData0;
        }

        public override void UpdateNativeHandle(SubstanceNativeGraph handler)
        {
            handler.SetInputFloat(Index, Data);
        }

        internal override void SetNumericDescription(NativeNumericInputDesc desc)
        {
            NumericalDescription = new SubstanceInputDescNumericalFloat
            {
                DefaultValue = desc.default_value.mFloatData0,
                MaxValue = desc.max_value.mFloatData0,
                MinValue = desc.min_value.mFloatData0,
                SliderClamp = Convert.ToBoolean(desc.sliderClamp.ToInt32()),
                SliderStep = desc.sliderStep,
                EnumValueCount = desc.enumValueCount.ToInt32()
            };

            return;
        }

        internal override void SetEnumOptions(NativeEnumInputDesc[] options)
        {
            NumericalDescription.EnumValues = new SubstanceFloatEnumOption[options.Length];

            for (int i = 0; i < NumericalDescription.EnumValues.Length; i++)
            {
                var option = new SubstanceFloatEnumOption
                {
                    Label = Marshal.PtrToStringAnsi(options[i].label),
                    Value = options[i].value.mFloatData0
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