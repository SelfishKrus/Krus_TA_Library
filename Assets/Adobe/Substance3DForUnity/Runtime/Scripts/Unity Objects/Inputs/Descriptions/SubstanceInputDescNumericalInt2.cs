using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance.Input.Description
{
    /// <summary>
    /// Numeric input description for input of type int2.
    /// </summary>
    [System.Serializable]
    public class SubstanceInputDescNumericalInt2 : ISubstanceInputDescNumerical
    {
        /// <summary>
        /// Default input value
        /// </summary>
        public Vector2Int DefaultValue;

        /// <summary>
        /// Minimum value (UI hint only)
        /// </summary>
        public Vector2Int MinValue;

        /// <summary>
        /// Maximum value. Only relevant if widget is Slider (UI hint only)
        /// </summary>
        public Vector2Int MaxValue;

        /// <summary>
        /// True if the slider value is clamped. Only relevant if widget is Slider (UI hint only)
        /// </summary>
        public bool SliderClamp;

        /// <summary>
        /// Number of enum option for this value.
        /// </summary>
        public int EnumValueCount;

        /// <summary>
        /// Array of enum values for this property. Only relevant if widget is ComboBox (UI hint only).
        /// </summary>
        public SubstanceInt2EnumOption[] EnumValues;
    }

    [System.Serializable]
    public class SubstanceInt2EnumOption
    {
        public Vector2Int Value;

        public string Label;
    }
}