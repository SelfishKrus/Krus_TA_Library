using System;
using UnityEngine;
using Adobe.Substance.Input.Description;

namespace Adobe.Substance.Input
{
    [System.Serializable]
    public class SubstanceInputFont : SubstanceInputBase
    {
        public Font Data;
        public override bool IsValid => true;
        public override SubstanceValueType ValueType => SubstanceValueType.Font;
        public override bool IsNumeric => false;
        public ISubstanceInputDescNumerical NumericalDescription => null;

        internal SubstanceInputFont(int index, DataInternalNumeric data)
        {
            Index = index;
            Data = null;
        }

        public override void UpdateNativeHandle(SubstanceNativeGraph handler)
        {
            return;
        }

        internal override void SetNumericDescription(NativeNumericInputDesc desc)
        {
            return;
        }

        internal override void SetEnumOptions(NativeEnumInputDesc[] options)
        {
            return;
        }
    }
}