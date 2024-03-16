using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance
{
    /// <summary>
    /// Represents different types of widgets used to assign input values.
    /// </summary>
    public enum SubstanceWidgetType
    {
        NoWidget,
        Slider,
        Angle,
        Color,
        ToggleButton,
        EnumButton,
        ComboBox,
        Image,
        Position
    }
}