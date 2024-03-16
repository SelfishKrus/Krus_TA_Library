using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance.Input.Description
{
    [System.Serializable]
    public class SubstanceInputDescNumericalInt3 : ISubstanceInputDescNumerical
    {
        public Vector3Int DefaultValue;

        public Vector3Int MinValue;

        public Vector3Int MaxValue;

        public Vector3Int SliderStep;

        public bool SliderClamp;

        public int EnumValueCount;

        public SubstanceInt3EnumOption[] EnumValues;
    }

    [System.Serializable]
    public class SubstanceInt3EnumOption
    {
        public Vector3Int Value;

        public string Label;
    }
}