using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance.Input
{
    /// <summary>
    /// Static factory for creating Unity input object from native data.
    /// </summary>
    public static class SubstanceInputFactory
    {
        /// <summary>
        /// Create a unity data object from native substance data.
        /// </summary>
        /// <param name="nativeData">Native data.</param>
        /// <returns>Instance of a substance input interface object.</returns>
        internal static SubstanceInputBase CreateInput(NativeData nativeData)
        {
            int index = (int)nativeData.Index;
            DataInternalNumeric data = nativeData.Data;

            switch (nativeData.ValueType)
            {
                case ValueType.SBSARIO_VALUE_FLOAT:
                    return new SubstanceInputFloat(index, data);

                case ValueType.SBSARIO_VALUE_FLOAT2:
                    return new SubstanceInputFloat2(index, data);

                case ValueType.SBSARIO_VALUE_FLOAT3:
                    return new SubstanceInputFloat3(index, data);

                case ValueType.SBSARIO_VALUE_FLOAT4:
                    return new SubstanceInputFloat4(index, data);

                case ValueType.SBSARIO_VALUE_INT:
                    return new SubstanceInputInt(index, data);

                case ValueType.SBSARIO_VALUE_INT2:
                    return new SubstanceInputInt2(index, data);

                case ValueType.SBSARIO_VALUE_INT3:
                    return new SubstanceInputInt3(index, data);

                case ValueType.SBSARIO_VALUE_INT4:
                    return new SubstanceInputInt4(index, data);

                case ValueType.SBSARIO_VALUE_IMAGE:
                    return new SubstanceInputTexture(index, data);

                case ValueType.SBSARIO_VALUE_STRING:
                    return new SubstanceInputString(index, data);

                case ValueType.SBSARIO_VALUE_FONT:
                    return new SubstanceInputFont(index, data);

                default:
                    throw new System.InvalidOperationException($"Can not create unity type from native data for type {nativeData.ValueType}.");
            }
        }

        internal static SubstanceInputBase CreateInvalidInput(int inputID)
        {
            return new SubstanceInvalidInput(inputID);
        }
    }
}