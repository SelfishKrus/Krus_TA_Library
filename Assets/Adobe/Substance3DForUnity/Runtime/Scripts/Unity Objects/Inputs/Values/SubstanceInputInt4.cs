using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adobe.Substance.Input.Description;
using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance.Input
{
    [Serializable]
    public class SubstanceInputInt4 : SubstanceInputBase
    {
        public int Data0;

        public int Data1;

        public int Data2;

        public int Data3;

        public SubstanceInputDescNumericalInt4 NumericalDescription;

        public override bool IsValid => true;
        public override SubstanceValueType ValueType => SubstanceValueType.Int4;
        public override bool IsNumeric => true;

        internal SubstanceInputInt4(int index, DataInternalNumeric data)
        {
            Index = index;
            Data0 = data.mIntData0;
            Data1 = data.mIntData1;
            Data2 = data.mIntData2;
            Data3 = data.mIntData3;
        }

        public override void UpdateNativeHandle(SubstanceNativeGraph handler)
        {
            handler.SetInputInt4(Index, Data0, Data1, Data2, Data3);
        }

        internal override void SetNumericDescription(NativeNumericInputDesc desc)
        {
            NumericalDescription = new SubstanceInputDescNumericalInt4
            {
                DefaultValue0 = desc.default_value.mIntData0,
                DefaultValue1 = desc.default_value.mIntData1,
                DefaultValue2 = desc.default_value.mIntData2,
                DefaultValue3 = desc.default_value.mIntData3,

                MaxValue0 = desc.max_value.mIntData0,
                MaxValue1 = desc.max_value.mIntData1,
                MaxValue2 = desc.max_value.mIntData2,
                MaxValue3 = desc.max_value.mIntData3,

                MinValue0 = desc.min_value.mIntData0,
                MinValue1 = desc.min_value.mIntData1,
                MinValue2 = desc.min_value.mIntData2,
                MinValue3 = desc.min_value.mIntData3,

                SliderClamp = Convert.ToBoolean(desc.sliderClamp.ToInt32()),
                EnumValueCount = desc.enumValueCount.ToInt32()
            };
        }

        internal override void SetEnumOptions(NativeEnumInputDesc[] options)
        {
            NumericalDescription.EnumValues = new SubstanceInt4EnumOption[options.Length];

            for (int i = 0; i < NumericalDescription.EnumValues.Length; i++)
            {
                var option = new SubstanceInt4EnumOption
                {
                    Label = Marshal.PtrToStringAnsi(options[i].label),
                    Value0 = options[i].value.mIntData0,
                    Value1 = options[i].value.mIntData1,
                    Value2 = options[i].value.mIntData2,
                    Value3 = options[i].value.mIntData3
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