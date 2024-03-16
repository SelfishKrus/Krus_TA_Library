using Adobe.Substance;
using Adobe.Substance.Input.Description;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    internal class SubstanceInt4GUIContent : SubstanceInputGUIContent
    {
        /// <summary>
        /// Numerical input description for the target SerializedProperty.
        /// </summary>
        public SubstanceInputDescNumericalInt4 NumericalDescription;

        public SubstanceInt4GUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalInt4 numDescription) : base(description, dataProp)
        {
            NumericalDescription = numDescription;
        }

        public SubstanceInt4GUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalInt4 numDescription, string text) : base(description, dataProp, text)
        {
            NumericalDescription = numDescription;
        }
    }

    internal class SubstanceInt3GUIContent : SubstanceInputGUIContent
    {
        /// <summary>
        /// Numerical input description for the target SerializedProperty.
        /// </summary>
        public SubstanceInputDescNumericalInt3 NumericalDescription;

        public SubstanceInt3GUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalInt3 numDescription) : base(description, dataProp)
        {
            NumericalDescription = numDescription;
        }

        public SubstanceInt3GUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalInt3 numDescription, string text) : base(description, dataProp, text)
        {
            NumericalDescription = numDescription;
        }
    }

    internal class SubstanceInt2GUIContent : SubstanceInputGUIContent
    {
        /// <summary>
        /// Numerical input description for the target SerializedProperty.
        /// </summary>
        public SubstanceInputDescNumericalInt2 NumericalDescription;

        public SubstanceInt2GUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalInt2 numDescription) : base(description, dataProp)
        {
            NumericalDescription = numDescription;
        }

        public SubstanceInt2GUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalInt2 numDescription, string text) : base(description, dataProp, text)
        {
            NumericalDescription = numDescription;
        }
    }

    internal class SubstanceIntGUIContent : SubstanceInputGUIContent
    {
        /// <summary>
        /// Numerical input description for the target SerializedProperty.
        /// </summary>
        public SubstanceInputDescNumericalInt NumericalDescription;

        public SubstanceIntGUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalInt numDescription) : base(description, dataProp)
        {
            NumericalDescription = numDescription;
        }

        public SubstanceIntGUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalInt numDescription, string text) : base(description, dataProp, text)
        {
            NumericalDescription = numDescription;
        }
    }

    internal class SubstanceFloat4GUIContent : SubstanceInputGUIContent
    {
        /// <summary>
        /// Numerical input description for the target SerializedProperty.
        /// </summary>
        public SubstanceInputDescNumericalFloat4 NumericalDescription;

        public SubstanceFloat4GUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalFloat4 numDescription) : base(description, dataProp)
        {
            NumericalDescription = numDescription;
        }

        public SubstanceFloat4GUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalFloat4 numDescription, string text) : base(description, dataProp, text)
        {
            NumericalDescription = numDescription;
        }
    }

    internal class SubstanceFloat3GUIContent : SubstanceInputGUIContent
    {
        /// <summary>
        /// Numerical input description for the target SerializedProperty.
        /// </summary>
        public SubstanceInputDescNumericalFloat3 NumericalDescription;

        public SubstanceFloat3GUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalFloat3 numDescription) : base(description, dataProp)
        {
            NumericalDescription = numDescription;
        }

        public SubstanceFloat3GUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalFloat3 numDescription, string text) : base(description, dataProp, text)
        {
            NumericalDescription = numDescription;
        }
    }

    internal class SubstanceFloat2GUIContent : SubstanceInputGUIContent
    {
        /// <summary>
        /// Numerical input description for the target SerializedProperty.
        /// </summary>
        public SubstanceInputDescNumericalFloat2 NumericalDescription;

        public SubstanceFloat2GUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalFloat2 numDescription) : base(description, dataProp)
        {
            NumericalDescription = numDescription;
        }

        public SubstanceFloat2GUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalFloat2 numDescription, string text) : base(description, dataProp, text)
        {
            NumericalDescription = numDescription;
        }
    }

    internal class SubstanceFloatGUIContent : SubstanceInputGUIContent
    {
        /// <summary>
        /// Numerical input description for the target SerializedProperty.
        /// </summary>
        public SubstanceInputDescNumericalFloat NumericalDescription;

        public SubstanceFloatGUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalFloat numDescription) : base(description, dataProp)
        {
            NumericalDescription = numDescription;
        }

        public SubstanceFloatGUIContent(SubstanceInputDescription description, SerializedProperty dataProp, SubstanceInputDescNumericalFloat numDescription, string text) : base(description, dataProp, text)
        {
            NumericalDescription = numDescription;
        }
    }

    internal class SubstanceIntComboBoxGUIContent : SubstanceIntGUIContent
    {
        public GUIContent[] EnumValuesGUI { get; }

        public int[] EnumValues { get; }

        public SubstanceIntComboBoxGUIContent(SubstanceInputDescription description, SubstanceInputDescNumericalInt intDescription, SerializedProperty dataProp) : base(description, dataProp, intDescription)
        {
            var enumValues = intDescription.EnumValues;

            EnumValuesGUI = new GUIContent[enumValues.Length];
            EnumValues = new int[enumValues.Length];

            for (int i = 0; i < EnumValuesGUI.Length; i++)
            {
                var enumElement = enumValues[i];
                EnumValuesGUI[i] = new GUIContent(enumElement.Label);
                EnumValues[i] = enumElement.Value;
            }
        }
    }

    internal class SubstanceIntEnumButtonGUIContent : SubstanceIntGUIContent
    {
        public GUIContent[] EnumValuesGUI { get; }

        public int[] EnumValues { get; }

        public SubstanceIntEnumButtonGUIContent(SubstanceInputDescription description, SubstanceInputDescNumericalInt intDescription, SerializedProperty dataProp) : base(description, dataProp, intDescription)
        {
            var enumValues = intDescription.EnumValues;

            EnumValuesGUI = new GUIContent[enumValues.Length];
            EnumValues = new int[enumValues.Length];

            for (int i = 0; i < EnumValuesGUI.Length; i++)
            {
                var enumElement = enumValues[i];
                EnumValuesGUI[i] = new GUIContent(enumElement.Label);
                EnumValues[i] = enumElement.Value;
            }
        }
    }
}