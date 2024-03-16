using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance.Input.Description
{
    /// <summary>
    /// Numeric input description for input of type float4.
    /// </summary>
    [System.Serializable]
    public class SubstanceInputDescNumericalFloat4 : ISubstanceInputDescNumerical
    {
        /// <summary>
        /// Default input value
        /// </summary>
        public Vector4 DefaultValue;

        /// <summary>
        /// Minimum value (UI hint only)
        /// </summary>
        public Vector4 MinValue;

        /// <summary>
        /// Maximum value (UI hint only) (Only relevant if widget is Input_Slider)
        /// </summary>
        public Vector4 MaxValue;

        /// <summary>
        /// Slider step size (UI hint only) (Only relevant if widget is Input_Slider)
        /// </summary>
        public float SliderStep;

        /// <summary>
        /// Should the slider clamp the value? (UI hint only) (Only relevant if widget is Input_Slider)
        /// </summary>
        public bool SliderClamp;

        /// <summary>
        /// Number of enum option for this value.
        /// </summary>
        public int EnumValueCount;

        /// <summary>
        /// Array of enum values for this property. Only relevant if widget is SBSARIO_WIDGET_COMBOBOX (UI hint only).
        /// </summary>
        public SubstanceFloat4EnumOption[] EnumValues;
    }

    [System.Serializable]
    public class SubstanceFloat4EnumOption
    {
        public Vector4 Value;

        public string Label;
    }
}