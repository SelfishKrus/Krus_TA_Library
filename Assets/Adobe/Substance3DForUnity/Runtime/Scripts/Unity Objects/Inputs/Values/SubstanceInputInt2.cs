using System;
using UnityEngine;
using System.Runtime.InteropServices;
using Adobe.Substance.Input.Description;

namespace Adobe.Substance.Input
{
    [System.Serializable]
    public class SubstanceInputInt2 : SubstanceInputBase
    {
        public Vector2Int Data;

        public SubstanceInputDescNumericalInt2 NumericalDescription;

        public override bool IsValid => true;
        public override SubstanceValueType ValueType => SubstanceValueType.Int2;
        public override bool IsNumeric => true;

        internal SubstanceInputInt2(int index, DataInternalNumeric data)
        {
            Index = index;
            Data = new Vector2Int(data.mIntData0, data.mIntData1);
        }

        public override void UpdateNativeHandle(SubstanceNativeGraph handler)
        {
            handler.SetInputInt2(Index, Data);
        }

        internal override void SetNumericDescription(NativeNumericInputDesc desc)
        {
            NumericalDescription = new SubstanceInputDescNumericalInt2
            {
                DefaultValue = new Vector2Int(desc.default_value.mIntData0, desc.default_value.mIntData1),
                MaxValue = new Vector2Int(desc.max_value.mIntData0, desc.max_value.mIntData1),
                MinValue = new Vector2Int(desc.min_value.mIntData0, desc.min_value.mIntData1),
                SliderClamp = Convert.ToBoolean(desc.sliderClamp.ToInt32()),
                EnumValueCount = desc.enumValueCount.ToInt32()
            };
        }

        internal override void SetEnumOptions(NativeEnumInputDesc[] options)
        {
            NumericalDescription.EnumValues = new SubstanceInt2EnumOption[options.Length];

            for (int i = 0; i < NumericalDescription.EnumValues.Length; i++)
            {
                var option = new SubstanceInt2EnumOption
                {
                    Label = Marshal.PtrToStringAnsi(options[i].label),
                    Value = new Vector2Int(options[i].value.mIntData0, options[i].value.mIntData1)
                };

                NumericalDescription.EnumValues[i] = option;
            }
        }

        public override bool TryGetNumericalDescription(out ISubstanceInputDescNumerical description)
        {
            description = NumericalDescription;
            return true;
        }
    }
}