using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance.Input.Description
{
    [System.Serializable]
    public class SubstanceInputDescription
    {
        public string Identifier;

        public string Label;

        public string GuiGroup;

        public string GuiDescription;

        public SubstanceValueType Type;

        public SubstanceWidgetType WidgetType;
    }
}