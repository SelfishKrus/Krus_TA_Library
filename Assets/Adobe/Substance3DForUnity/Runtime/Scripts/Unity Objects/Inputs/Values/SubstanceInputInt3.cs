using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Adobe.Substance.Input.Description;

namespace Adobe.Substance.Input
{
    [System.Serializable]
    public class SubstanceInputInt3 : SubstanceInputBase
    {
        public Vector3Int Data;

        public SubstanceInputDescNumericalInt3 NumericalDescription;

        public override bool IsValid => true;
        public override SubstanceValueType ValueType => SubstanceValueType.Int3;
        public override bool IsNumeric => true;

        internal SubstanceInputInt3(int index, DataInternalNumeric data)
        {
            Index = index;
            Data = new Vector3Int(data.mIntData0, data.mIntData1, data.mIntData2);
        }

        public override void UpdateNativeHandle(SubstanceNativeGraph handler)
        {
            handler.SetInputInt3(Index, Data);
        }

        internal override void SetNumericDescription(NativeNumericInputDesc desc)
        {
            NumericalDescription = new SubstanceInputDescNumericalInt3
            {
                DefaultValue = new Vector3Int(desc.default_value.mIntData0, desc.default_value.mIntData1, desc.default_value.mIntData2),
                MaxValue = new Vector3Int(desc.max_value.mIntData0, desc.max_value.mIntData1, desc.max_value.mIntData2),
                MinValue = new Vector3Int(desc.min_value.mIntData0, desc.min_value.mIntData1, desc.min_value.mIntData2),
                SliderClamp = Convert.ToBoolean(desc.sliderClamp.ToInt32()),
                EnumValueCount = desc.enumValueCount.ToInt32()
            };
        }

        internal override void SetEnumOptions(NativeEnumInputDesc[] options)
        {
            NumericalDescription.EnumValues = new SubstanceInt3EnumOption[options.Length];

            for (int i = 0; i < NumericalDescription.EnumValues.Length; i++)
            {
                var option = new SubstanceInt3EnumOption
                {
                    Label = Marshal.PtrToStringAnsi(options[i].label),
                    Value = new Vector3Int(options[i].value.mIntData0, options[i].value.mIntData1, options[i].value.mIntData2)
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