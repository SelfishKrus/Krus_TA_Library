using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance.Input.Description
{
    [System.Serializable]
    public class SubstanceInputDescNumericalInt4 : ISubstanceInputDescNumerical
    {
        public int DefaultValue0;
        public int DefaultValue1;
        public int DefaultValue2;
        public int DefaultValue3;

        public int MinValue0;
        public int MinValue1;
        public int MinValue2;
        public int MinValue3;

        public int MaxValue0;
        public int MaxValue1;
        public int MaxValue2;
        public int MaxValue3;

        public int SliderStep0;
        public int SliderStep1;
        public int SliderStep2;
        public int SliderStep3;

        public bool SliderClamp;

        public int EnumValueCount;

        public SubstanceInt4EnumOption[] EnumValues;
    }

    [System.Serializable]
    public class SubstanceInt4EnumOption
    {
        public int Value0;
        public int Value1;
        public int Value2;
        public int Value3;

        public string Label;
    }
}