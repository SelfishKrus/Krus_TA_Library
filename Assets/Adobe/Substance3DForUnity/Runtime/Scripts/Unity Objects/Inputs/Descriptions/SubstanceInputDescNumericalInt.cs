using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance.Input.Description
{
    /// <summary>
    /// Numeric input description for input of type int.
    /// </summary>
    [System.Serializable]
    public class SubstanceInputDescNumericalInt : ISubstanceInputDescNumerical
    {
        /// <summary>
        /// Default input value
        /// </summary>
        public int DefaultValue;

        /// <summary>
        /// Minimum value (UI hint only)
        /// </summary>
        public int MinValue;

        /// <summary>
        /// Maximum value. Only relevant if widget is Slider (UI hint only)
        /// </summary>
        public int MaxValue;

        /// <summary>
        /// Slider step size. Only relevant if widget is Slider (UI hint only).
        /// </summary>
        public float SliderStep;

        /// <summary>
        /// True if the slider value is clamped. Only relevant if widget is Slider (UI hint only)
        /// </summary>
        public bool SliderClamp;

        /// <summary>
        /// If non-empty, the labels to use for False (unchecked) and True (checked) values. Only relevant if widget is Input_Togglebutton
        /// </summary>
        public string LabelFalse;

        public string LabelTrue;

        /// <summary>
        /// Number of enum option for this value.
        /// </summary>
        public int EnumValueCount;

        /// <summary>
        /// Array of enum values for this property. Only relevant if widget is SBSARIO_WIDGET_COMBOBOX (UI hint only).
        /// </summary>
        public SubstanceIntEnumOption[] EnumValues;
    }

    [System.Serializable]
    public class SubstanceIntEnumOption
    {
        public int Value;

        public string Label;
    }
}