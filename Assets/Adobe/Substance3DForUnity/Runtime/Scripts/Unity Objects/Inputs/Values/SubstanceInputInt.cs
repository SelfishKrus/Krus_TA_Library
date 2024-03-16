using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adobe.Substance.Input.Description;
using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance.Input
{
    [System.Serializable]
    public class SubstanceInputInt : SubstanceInputBase
    {
        public int Data;

        public SubstanceInputDescNumericalInt NumericalDescription;

        public override bool IsValid => true;
        public override SubstanceValueType ValueType => SubstanceValueType.Int;
        public override bool IsNumeric => true;

        internal SubstanceInputInt(int index, DataInternalNumeric data)
        {
            Index = index;
            Data = data.mIntData0;
        }

        public override void UpdateNativeHandle(SubstanceNativeGraph handler)
        {
            handler.SetInputInt(Index, Data);
        }

        internal override void SetNumericDescription(NativeNumericInputDesc desc)
        {
            NumericalDescription = new SubstanceInputDescNumericalInt
            {
                DefaultValue = desc.default_value.mIntData0,
                MaxValue = desc.max_value.mIntData0,
                MinValue = desc.min_value.mIntData0,
                SliderClamp = Convert.ToBoolean(desc.sliderClamp.ToInt32()),
                SliderStep = desc.sliderStep,
                EnumValueCount = desc.enumValueCount.ToInt32()
            };
        }

        internal override void SetEnumOptions(NativeEnumInputDesc[] options)
        {
            NumericalDescription.EnumValues = new SubstanceIntEnumOption[options.Length];

            for (int i = 0; i < NumericalDescription.EnumValues.Length; i++)
            {
                var option = new SubstanceIntEnumOption
                {
                    Label = Marshal.PtrToStringAnsi(options[i].label),
                    Value = options[i].value.mIntData0
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