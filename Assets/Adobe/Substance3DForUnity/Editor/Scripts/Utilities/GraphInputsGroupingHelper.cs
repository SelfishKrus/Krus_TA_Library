using Adobe.Substance;
using Adobe.Substance.Input;
using Adobe.Substance.Input.Description;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEditor;

namespace Adobe.SubstanceEditor
{
    /// <summary>
    /// Cached info for a substance input. Allow drawing UI without having to query description values from the SerializedProperty object.
    /// </summary>
    internal class SubstanceInputCachedInfo
    {
        /// <summary>
        /// Input serialized property.
        /// </summary>
        public SerializedProperty InputProperty { get; }

        /// <summary>
        /// GUIContent for the drawing the input.
        /// </summary>
        public SubstanceInputGUIContent GUIContent { get; }

        public int Index { get; }

        public SubstanceInputCachedInfo(SerializedProperty inputProperty, SubstanceInputGUIContent GUIContent, int index)
        {
            this.InputProperty = inputProperty;
            this.GUIContent = GUIContent;
            Index = index;
        }
    }

    internal class SubstanceInputGroupCachedInfo
    {
        public string Name { get; }

        public List<SubstanceInputCachedInfo> Inputs { get; }

        public bool ShowGroup { get; set; }

        public SubstanceInputGroupCachedInfo(string groupName)
        {
            Name = groupName;
            Inputs = new List<SubstanceInputCachedInfo>();
            ShowGroup = false;
        }
    }

    /// <summary>
    /// Helper class for caching grouping information for inputs so we don't have to query them every UI draw.
    /// </summary>
    internal class GraphInputsGroupingHelper
    {
        /// <summary>
        /// Readonly list with all input groups and their elements info.
        /// </summary>
        public IReadOnlyList<SubstanceInputGroupCachedInfo> InputGroups { get; }

        /// <summary>
        /// Groups with inputs that don't have grouping information.
        /// </summary>
        public SubstanceInputGroupCachedInfo GrouplessInputs { get; }

        public GraphInputsGroupingHelper(SubstanceGraphSO graph, SerializedObject targetObject)
        {
            var GUIgroups = new List<SubstanceInputGroupCachedInfo>();
            var graphInputs = targetObject.FindProperty("Input");
            var groups = graph.Input.Select(a => a.Description.GuiGroup).Distinct();

            foreach (var group in groups)
            {
                if (group.Equals("Channels", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var groupInfo = new SubstanceInputGroupCachedInfo(group);

                for (int i = 0; i < graph.Input.Count; i++)
                {
                    var target = graph.Input[i];

                    if (target.Description.GuiGroup == group)
                    {
                        SubstanceInputGUIContent guiContent;

                        var graphInput = graphInputs.GetArrayElementAtIndex(i);
                        var dataProp = graphInput.FindPropertyRelative("Data");

                        target.TryGetNumericalDescription(out ISubstanceInputDescNumerical descNumerical);

                        switch (target.Description.Type)
                        {
                            case SubstanceValueType.Float:
                                guiContent = new SubstanceFloatGUIContent(target.Description, dataProp, descNumerical as SubstanceInputDescNumericalFloat);
                                break;

                            case SubstanceValueType.Float2:
                                guiContent = new SubstanceFloat2GUIContent(target.Description, dataProp, descNumerical as SubstanceInputDescNumericalFloat2);
                                break;

                            case SubstanceValueType.Float3:
                                guiContent = new SubstanceFloat3GUIContent(target.Description, dataProp, descNumerical as SubstanceInputDescNumericalFloat3);
                                break;

                            case SubstanceValueType.Float4:
                                guiContent = new SubstanceFloat4GUIContent(target.Description, dataProp, descNumerical as SubstanceInputDescNumericalFloat4);
                                break;

                            case SubstanceValueType.Int:
                                guiContent = GetGUIContentForInt(target, descNumerical, dataProp);
                                break;

                            case SubstanceValueType.Int2:
                                guiContent = new SubstanceInt2GUIContent(target.Description, dataProp, descNumerical as SubstanceInputDescNumericalInt2);
                                break;

                            case SubstanceValueType.Int3:
                                guiContent = new SubstanceInt3GUIContent(target.Description, dataProp, descNumerical as SubstanceInputDescNumericalInt3);
                                break;

                            case SubstanceValueType.Int4:
                                guiContent = new SubstanceInt4GUIContent(target.Description, dataProp, descNumerical as SubstanceInputDescNumericalInt4);
                                break;

                            default:
                                guiContent = new SubstanceInputGUIContent(target.Description, dataProp);
                                break;
                        }

                        groupInfo.Inputs.Add(new SubstanceInputCachedInfo(graphInput, guiContent, i));
                    }
                }

                if (string.IsNullOrEmpty(groupInfo.Name))
                {
                    if (GrouplessInputs == null)
                        GrouplessInputs = groupInfo;
                    else
                        GrouplessInputs.Inputs.AddRange(groupInfo.Inputs);
                }
                else
                {
                    GUIgroups.Add(groupInfo);
                }
            }

            InputGroups = GUIgroups;
        }

        private static SubstanceInputGUIContent GetGUIContentForInt(ISubstanceInput target, ISubstanceInputDescNumerical descNumerical, SerializedProperty dataProp)
        {
            if (target.Description.WidgetType == SubstanceWidgetType.ComboBox)
            {
                return new SubstanceIntComboBoxGUIContent(target.Description, descNumerical as SubstanceInputDescNumericalInt, dataProp);
            }
            else if (target.Description.WidgetType == SubstanceWidgetType.EnumButton)
            {
                return new SubstanceIntEnumButtonGUIContent(target.Description, descNumerical as SubstanceInputDescNumericalInt, dataProp);
            }
   
            return new SubstanceIntGUIContent(target.Description, dataProp, descNumerical as SubstanceInputDescNumericalInt);
        }
    }
    internal class GraphOutputAlphaChannelsHelper
    {
        private readonly List<string> _channels;

        public GraphOutputAlphaChannelsHelper(SubstanceGraphSO graph)
        {
            _channels = new List<string> { "source" };

            foreach (var item in graph.Output.Where(a => a.IsAlphaAssignable).Select(b => b.Description.Label))
                _channels.Add(item);
        }

        public string[] GetAlphaChannels(string label)
        {
            return _channels.Where(a => a != label).ToArray();
        }
    }
}