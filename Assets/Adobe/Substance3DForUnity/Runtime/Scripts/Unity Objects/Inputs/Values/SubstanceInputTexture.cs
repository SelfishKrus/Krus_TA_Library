using System;
using UnityEngine;
using Adobe.Substance.Input.Description;
using System.Runtime.InteropServices;

namespace Adobe.Substance.Input
{
    [System.Serializable]
    public class SubstanceInputTexture : SubstanceInputBase
    {
        [SerializeField]
        private Texture2D Data;

        public override bool IsValid => true;
        public override SubstanceValueType ValueType => SubstanceValueType.Image;
        public override bool IsNumeric => false;

        internal SubstanceInputTexture(int index, DataInternalNumeric data)
        {
            Index = index;
            Data = null;
        }

        public override void UpdateNativeHandle(SubstanceNativeGraph handler)
        {
            if (Data == null)
                return;

            if (!Data.isReadable)
            {
                Debug.LogWarning($"Input textures must be set as readable. Texture assigned to {Description.Identifier} will have no effect.");
                return;
            }

            handler.SetInputTexture2D(Index, Data);
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