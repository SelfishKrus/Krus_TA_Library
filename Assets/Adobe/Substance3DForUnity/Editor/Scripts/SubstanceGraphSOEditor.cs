using Adobe.Substance;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    [CustomEditor(typeof(SubstanceGraphSO))]
    public class SubstanceGraphSOEditor : UnityEditor.Editor
    {
        private GraphInputsGroupingHelper _inputGroupingHelper;

        private GraphOutputAlphaChannelsHelper _outputChannelsHelper;

        private bool _propertiesChanged = false;

        private bool _showOutput = true;

        private bool _showExportPresentationHandler = false;

        private bool _showPhysicalSize = false;

        private SubstanceGraphSO _target = null;

        private SubstanceNativeGraph _nativeGraph = null;

        private Rect lastRect;

        private Texture2D _backgroundImage;

        private MaterialEditor _materialPreviewEditor;

        private Vector2 _textureOutputScrollView;

        private SerializedProperty _generateAllOutputsProperty;
        private SerializedProperty _generateAllMipmapsProperty;
        private SerializedProperty _runtimeOnlyProperty;
        private SerializedProperty _outputRemapedProperty;
        private SerializedProperty _graphOutputs;
        private SerializedProperty _presetProperty;
        private SerializedProperty _physicalSizelProperty;
        private SerializedProperty _hasPhysicalSizeProperty;
        private SerializedProperty _enablePhysicalSizeProperty;

        private IReadOnlyList<SerializedProperty> _outputProperties;
        private SerializedProperty _presetsListProperty;

        public void OnEnable()
        {
            if (!IsSerializedObjectReady())
                return;

            _target = serializedObject.targetObject as SubstanceGraphSO;
            _textureOutputScrollView = Vector2.zero;
            _propertiesChanged = false;

            if (_inputGroupingHelper == null)
                _inputGroupingHelper = new GraphInputsGroupingHelper(_target, serializedObject);

            if (_outputChannelsHelper == null)
                _outputChannelsHelper = new GraphOutputAlphaChannelsHelper(_target);

            float c = (EditorGUIUtility.isProSkin) ? 0.35f : 0.65f;

            if (_backgroundImage == null)
                _backgroundImage = Globals.CreateColoredTexture(16, 16, new Color(c, c, c, 1));

            EditorApplication.projectWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
            _generateAllOutputsProperty = serializedObject.FindProperty("GenerateAllOutputs");
            _generateAllMipmapsProperty = serializedObject.FindProperty("GenerateAllMipmaps");
            _runtimeOnlyProperty = serializedObject.FindProperty("IsRuntimeOnly");
            _outputRemapedProperty = serializedObject.FindProperty("OutputRemaped");
            _graphOutputs = serializedObject.FindProperty("Output");
            _presetProperty = serializedObject.FindProperty("CurrentStatePreset");
            _physicalSizelProperty = serializedObject.FindProperty("PhysicalSize");
            _hasPhysicalSizeProperty = serializedObject.FindProperty("HasPhysicalSize");
            _enablePhysicalSizeProperty = serializedObject.FindProperty("EnablePhysicalSize");
            _presetsListProperty = serializedObject.FindProperty("Presets");

            if (!SubstanceEditorEngine.instance.TryGetHandlerFromInstance(_target, out _nativeGraph))
            {
                if (!SubstanceEditorEngine.instance.IsInitialized)
                    return;

                SubstanceEditorEngine.instance.InitializeInstance(_target, null, out SubstanceGraphSO _);

                if (SubstanceEditorEngine.instance.TryGetHandlerFromInstance(_target, out _nativeGraph))
                    _target.RuntimeInitialize(_nativeGraph, _target.IsRuntimeOnly);
            }

            PopulateOutputProperties();
        }

        private void PopulateOutputProperties()
        {
            var list = new List<SerializedProperty>();

            for (int i = 0; i < _graphOutputs.arraySize; i++)
            {
                var output = _graphOutputs.GetArrayElementAtIndex(i);
                var textureTarget = output.FindPropertyRelative("MaterialTextureTarget");
                list.Add(textureTarget);
            }

            _outputProperties = list;
        }

        private void GetShaderInputTextures(Shader shader)
        {
            _shaderInputTextures.Add("none");
            EditorTools.GetShaderProperties(shader, _shaderInputTextures);
        }

        public void OnDisable()
        {
            if (_materialPreviewEditor != null)
            {
                _materialPreviewEditor.OnDisable();
                _materialPreviewEditor = null;
            }

            SaveEditorChanges();
            EditorApplication.projectWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
        }

        public void SaveEditorChanges()
        {
            if (_propertiesChanged)
            {
                SaveTGAFiles();
                UpdateGraphMaterialLabel();

                AssetDatabase.Refresh();
            }

            _propertiesChanged = false;
        }

        public override void OnInspectorGUI()
        {
            if (serializedObject.targetObject == null)
            {
                return;
            }

            if (_nativeGraph == null)
            {
                if (!SubstanceEditorEngine.instance.TryGetHandlerFromInstance(_target, out _nativeGraph))
                {
                    return;
                }

                if (_nativeGraph.IsDisposed())
                {
                    _nativeGraph = null;
                    return;
                }
            }

            if (_materialPreviewEditor == null)
            {
                var material = _target.OutputMaterial;

                if (material != null)
                    _materialPreviewEditor = MaterialEditor.CreateEditor(material) as MaterialEditor;
            }

            serializedObject.Update();

            try
            {
                if (FirstConfigurePresets() || DrawGraph())
                {
                    serializedObject.ApplyModifiedProperties();
                    _propertiesChanged = true;
                }
            }
            catch (ObjectDisposedException)
            {
            }

            
        }

        /// <summary>
        /// Callback for GUI events to block substance files from been duplicated
        /// </summary>
        /// <param name="guid">Asset guid.</param>
        /// <param name="rt">GUI rect.</param>
        protected static void OnHierarchyWindowItemOnGUI(string guid, Rect rt)
        {
            var currentEvent = Event.current;

            if ("Duplicate" == currentEvent.commandName && currentEvent.type == EventType.ExecuteCommand)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var instanceObject = AssetDatabase.LoadAssetAtPath<SubstanceGraphSO>(assetPath);

                if (instanceObject != null)
                {
                    Debug.LogWarning("Substance graph can not be manually duplicated.");
                    currentEvent.Use();
                }
            }
        }

        #region Material Preview

        public override bool HasPreviewGUI()
        {
            return _materialPreviewEditor != null;
        }

        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("Material", null, "");
        }

        public override void OnPreviewSettings()
        {
            if (_materialPreviewEditor)
                _materialPreviewEditor.OnPreviewSettings();
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (_materialPreviewEditor)
                _materialPreviewEditor.OnPreviewGUI(r, background);
        }

        #endregion Material Preview

        #region Draw

        private bool DrawGraph()
        {
            bool valuesChanged = false;

            if (DrawTextureGenerationSettings(_generateAllOutputsProperty, _generateAllMipmapsProperty, _runtimeOnlyProperty))
            {
                _outputRemapedProperty.boolValue = true;
                valuesChanged = true;
            }

            GUILayout.Space(16);

            if (DrawPresentExport(_target))
                valuesChanged = true;

            DrawInputs(out bool serializedObject, out bool renderGraph);

            if (renderGraph)
            {
                var newPreset = _nativeGraph.CreatePresetFromCurrentState();
                _presetProperty.stringValue = newPreset;
                SubstanceEditorEngine.instance.SubmitAsyncRenderWork(_nativeGraph, _target);
                valuesChanged = true;
            }

            if (serializedObject)
                valuesChanged = true;

            EditorGUILayout.Space();

            _showOutput = EditorGUILayout.Foldout(_showOutput, "Generated textures");

            if (_showOutput)
            {
                if (DrawAdvanceSettings())
                {
                    MaterialUtils.EnableEmissionIfAssigned(_target.OutputMaterial);
                    _outputRemapedProperty.boolValue = true;
                    valuesChanged = true;
                }

                if (DrawGeneratedTextures(_graphOutputs, _generateAllOutputsProperty.boolValue))
                {
                    _outputRemapedProperty.boolValue = true;
                    valuesChanged = true;
                }
            }

            return valuesChanged;
        }

        #region Texture Generation Settings

        private bool DrawTextureGenerationSettings(SerializedProperty generateAllOutputsProperty, SerializedProperty generateAllMipmapsProperty, SerializedProperty runtimeOnlyProperty)
        {
            bool changed = false;

            GUILayout.Space(4);

            var boxWidth = EditorGUIUtility.currentViewWidth;
            var boxHeight = (3 * EditorGUIUtility.singleLineHeight) + 16;
            var padding = 16;

            DrawHighlightBox(boxWidth, boxHeight, padding);

            if (DrawGenerateAllOutputs(generateAllOutputsProperty) ||
                DrawGenerateAllMipmaps(generateAllMipmapsProperty) ||
                DrawRuntimeOnlyToggle(runtimeOnlyProperty))
            {
                changed = true;
            }

            return changed;
        }

        private static readonly GUIContent _GenerateAllOutputsGUI = new GUIContent("Generate All Outputs", "Force the generation of all Substance outputs");

        private bool DrawGenerateAllOutputs(SerializedProperty generateAllOutputsProperty)
        {
            var oldValue = generateAllOutputsProperty.boolValue;
            generateAllOutputsProperty.boolValue = EditorGUILayout.Toggle(_GenerateAllOutputsGUI, generateAllOutputsProperty.boolValue);
            return oldValue != generateAllOutputsProperty.boolValue;
        }

        private static readonly GUIContent _GenerateAllMipMapsGUI = new GUIContent("Generate Mip Maps", "Enable MipMaps when generating textures");

        private bool DrawGenerateAllMipmaps(SerializedProperty generateAllMipmapsProperty)
        {
            var oldValue = generateAllMipmapsProperty.boolValue;
            generateAllMipmapsProperty.boolValue = EditorGUILayout.Toggle(_GenerateAllMipMapsGUI, generateAllMipmapsProperty.boolValue);
            return oldValue != generateAllMipmapsProperty.boolValue;
        }

        private static readonly GUIContent _RuntimeOnlyGUI = new GUIContent("Runtime only", "If checked this instance will not generate TGA texture files");

        private bool DrawRuntimeOnlyToggle(SerializedProperty runtimeOnlyProperty)
        {
            var oldValue = runtimeOnlyProperty.boolValue;
            runtimeOnlyProperty.boolValue = EditorGUILayout.Toggle(_RuntimeOnlyGUI, runtimeOnlyProperty.boolValue);
            return oldValue != runtimeOnlyProperty.boolValue;
        }

        #endregion Texture Generation Settings

        #region Physical size

        private bool DrawPhysicalSize()
        {
            if (!_hasPhysicalSizeProperty.boolValue)
                return false;

            _showPhysicalSize = EditorGUILayout.Foldout(_showPhysicalSize, "Physical Size");
            bool valueChanged = false;

            if (_showPhysicalSize)
            {
                var currentValue = _physicalSizelProperty.vector3Value;
                var enablePhysicaSize = _enablePhysicalSizeProperty.boolValue;

                if (EditorGUILayout.Toggle("Use Physical Size", enablePhysicaSize) != enablePhysicaSize)
                {
                    _enablePhysicalSizeProperty.boolValue = !enablePhysicaSize;
                    valueChanged = true;
                }

                var newValue = new Vector3();

                newValue.x = EditorGUILayout.FloatField("X:", currentValue.x);
                newValue.y = EditorGUILayout.FloatField("Y:", currentValue.y);
                newValue.z = EditorGUILayout.FloatField("Z:", currentValue.z);

                if ((newValue - currentValue).sqrMagnitude >= 0.01f)
                {
                    _physicalSizelProperty.vector3Value = newValue;
                    valueChanged = true;
                }

                if (_target.OutputMaterial != null)
                {
                    DrawPhysicalSizeOffsets(_target.OutputMaterial);
                }
            }
            return valueChanged;
        }

        private static bool _showPhysicalSizePositionOffset = false;

        private void DrawPhysicalSizeOffsets(Material material)
        {
            EditorGUI.indentLevel++;

            _showPhysicalSizePositionOffset = EditorGUILayout.Foldout(_showPhysicalSizePositionOffset, "Position Offset (In %)");

            if (_showPhysicalSizePositionOffset)
            {
                Vector2 offset = MaterialUtils.GetPhysicalSizePositionOffset(material);
                Vector2 newOffset = offset;
                newOffset.x = EditorGUILayout.FloatField("X:", offset.x * 100.0f) / 100.0f;
                newOffset.y = EditorGUILayout.FloatField("Y:", offset.y * 100.0f) / 100.0f;

                if ((newOffset - offset).sqrMagnitude >= 0.0000001f)
                {
                    MaterialUtils.SetPhysicalSizePositionOffset(material, newOffset);
                }
            }

            EditorGUI.indentLevel--;
        }

        #endregion Physical size

        #region Input draw

        /// <summary>
        /// Draws substance file inputs.
        /// </summary>
        /// <param name="serializeObject">True if object properties have changed.</param>
        /// <param name="renderGraph">True if substance graph must be re rendered.</param>
        private void DrawInputs(out bool serializeObject, out bool renderGraph)
        {
            renderGraph = false;
            serializeObject = false;

            EditorGUILayout.Space();

            if (DrawGrouplessInputs(_inputGroupingHelper.GrouplessInputs))
            {
                renderGraph = true;
                serializeObject = true;
            }

            EditorGUILayout.Space();

            if (PhysicalSizeExtension.IsSupported())
            {
                if (DrawPhysicalSize())
                {
                    renderGraph = true;
                    serializeObject = true;
                    MaterialUtils.ApplyPhysicalSize(_target.OutputMaterial, _physicalSizelProperty.vector3Value, _enablePhysicalSizeProperty.boolValue);
                    UpdateGraphMaterialLabel();
                }

                EditorGUILayout.Space();
            }

            foreach (var groupInfo in _inputGroupingHelper.InputGroups)
            {
                if (DrawInputGroup(groupInfo))
                {
                    renderGraph = true;
                    serializeObject = true;
                }              
            }
        }

        /// <summary>
        /// Draws the inputs that are not part of any input group.
        /// </summary>
        /// <param name="inputsInfo">Inputs info</param>
        /// <returns>True if any input has changed.</returns>
        private bool DrawGrouplessInputs(SubstanceInputGroupCachedInfo inputsInfo)
        {
            var indexArray = inputsInfo.Inputs;

            bool changed = false;

            for (int i = 0; i < indexArray.Count; i++)
            {
                var property = indexArray[i].InputProperty;
                var guiContent = indexArray[i].GUIContent;
                var index = indexArray[i].Index;

                if (_nativeGraph.IsInputVisible(index))
                {
                    if (SubstanceInputDrawer.DrawInput(property, guiContent, _nativeGraph, index))
                        changed = true;
                }
            }

            return changed;
        }

        /// <summary>
        /// Draws inputs from a input group.
        /// </summary>
        /// <param name="groupInfo"></param>
        /// <returns></returns>
        private bool DrawInputGroup(SubstanceInputGroupCachedInfo groupInfo)
        {
            var groupName = groupInfo.Name;
            var indexArray = groupInfo.Inputs;

            var visibilityArray = indexArray.Select(a => _nativeGraph.IsInputVisible(a.Index)).ToArray();

            if (visibilityArray.Where(a => a).Count() == 0)
            {
                return false;
            }

            groupInfo.ShowGroup = EditorGUILayout.Foldout(groupInfo.ShowGroup, groupName);

            if (!groupInfo.ShowGroup)
            {
                EditorGUILayout.Space();
                return false;
            }

            bool changed = false;

            for (int i = 0; i < indexArray.Count; i++)
            {
                EditorGUI.indentLevel++;

                var index = indexArray[i].Index;

                if (visibilityArray[i])
                {
                    var property = indexArray[i].InputProperty;
                    var guiContent = indexArray[i].GUIContent;

                    if (SubstanceInputDrawer.DrawInput(property, guiContent, _nativeGraph, index))
                        changed = true;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            return changed;
        }

        #endregion Input draw

        #region Output draw

        private static readonly GUIContent _GeneratedTextureGUI = new GUIContent();

        private bool DrawGeneratedTextures(SerializedProperty outputList, bool generateAllTextures)
        {
            bool valueChanged = false;
            EditorGUILayout.Space(4);

            using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(_textureOutputScrollView, false, false))
            {
                scrollViewScope.handleScrollWheel = false;
                _textureOutputScrollView = scrollViewScope.scrollPosition;

                EditorGUILayout.BeginHorizontal();
                {
                    var outputsCount = outputList.arraySize;

                    var shaderProperties = EditorTools.GetShaderProperties(_target.GetShader());

                    for (int i = 0; i < outputsCount; i++)
                    {
                        var outputProperty = outputList.GetArrayElementAtIndex(i);
                        var outputTexture = _target.Output[i];

                        if (generateAllTextures || outputTexture.IsStandardOutput(shaderProperties))
                            valueChanged |= DrawOutputTexture(outputProperty, _GeneratedTextureGUI, outputTexture);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            return valueChanged;
        }

        private bool DrawOutputTexture(SerializedProperty output, GUIContent content, SubstanceOutputTexture substanceOutput)
        {
            var valueChanged = false;

            EditorGUILayout.BeginVertical(GUILayout.Width(120));
            {
                var texture = output.FindPropertyRelative("OutputTexture").objectReferenceValue as Texture2D;
                var label = output.FindPropertyRelative("Description.Channel").stringValue;
                var sRGB = output.FindPropertyRelative("sRGB");
                var alpha = output.FindPropertyRelative("AlphaChannel");
                var inverAlpha = output.FindPropertyRelative("InvertAssignedAlpha");
                var isAlphaAssignable = output.FindPropertyRelative("IsAlphaAssignable").boolValue;

                //Draw texture preview.
                if (texture != null)
                {
                    if (texture != null)
                    {
                        content.text = null;

                        var thumbnail = EditorUtility.IsDirty(texture) ? AssetPreview.GetMiniThumbnail(texture) : AssetPreview.GetAssetPreview(texture);

                        if (thumbnail == null)
                            thumbnail = AssetPreview.GetAssetPreview(texture);

                        content.image = thumbnail;
                        content.tooltip = texture.name;

                        if (GUILayout.Button(content, //style,
                                         GUILayout.Width(70),
                                         GUILayout.Height(70)))
                        {
                            // Highlight object in project browser:
                            EditorGUIUtility.PingObject(texture);
                        }
                    }
                }

                GUILayout.Label(label);

                if (substanceOutput.IsBaseColor() || substanceOutput.IsDiffuse() || substanceOutput.IsEmissive())
                {
                    var oldsRGB = sRGB.boolValue;
                    var newsRGB = GUILayout.Toggle(oldsRGB, "sRGB");

                    if (newsRGB != oldsRGB)
                    {
                        sRGB.boolValue = newsRGB;
                        valueChanged = true;
                    }
                }

                //Draw alpha remapping.
                EditorGUILayout.BeginHorizontal(GUILayout.Width(80), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                {
                    if (isAlphaAssignable)
                    {
                        var option = _outputChannelsHelper.GetAlphaChannels(label);
                        var index = 0;

                        if (!string.IsNullOrEmpty(alpha.stringValue))
                            index = Array.IndexOf(option, alpha.stringValue);

                        EditorGUILayout.LabelField("A", GUILayout.Width(10));

                        var newIndex = EditorGUILayout.Popup(index, option, GUILayout.Width(70));

                        if (newIndex != index)
                        {
                            alpha.stringValue = newIndex != 0 ? option[newIndex] : string.Empty;
                            valueChanged = true;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                //Draw inver alpha.
                EditorGUILayout.BeginHorizontal(GUILayout.Width(80), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                {
                    if (!string.IsNullOrEmpty(alpha.stringValue))
                    {
                        var oldValue = inverAlpha.boolValue;
                        var newValue = GUILayout.Toggle(oldValue, "Invert alpha");

                        if (newValue != oldValue)
                        {
                            inverAlpha.boolValue = newValue;
                            valueChanged = true;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            return valueChanged;
        }

        private static bool _showAdvanceSettings = false;

        private readonly List<string> _shaderInputTextures = new List<string>();
        private string _shaderName = string.Empty;

        private bool DrawAdvanceSettings()
        {
            EditorGUILayout.Space();

            EditorGUI.indentLevel++;

            _showAdvanceSettings = EditorGUILayout.Foldout(_showAdvanceSettings, "Output Textures Mapping");

            EditorGUILayout.Space();

            bool result = false;

            if (_showAdvanceSettings)
            {
                EditorGUI.indentLevel++;

                if (_target.OutputMaterial != null)
                {
                    if (!_shaderName.Equals(_target.OutputMaterial.shader.name, StringComparison.OrdinalIgnoreCase))
                    {
                        _shaderInputTextures.Clear();
                        GetShaderInputTextures(_target.OutputMaterial.shader);
                    }

                    if (_outputProperties == null)
                    {
                        PopulateOutputProperties();
                    }

                    if (_outputProperties != null)
                    {
                        for (int i = 0; i < _outputProperties.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                GUILayout.TextField(_target.Output[i].Description.Channel, GUILayout.Width(200));

                                var oldstring = _outputProperties[i].stringValue;
                                var oldSelected = _shaderInputTextures.FindIndex(0, _shaderInputTextures.Count, (value) => { return oldstring.Equals(value, StringComparison.OrdinalIgnoreCase); });

                                if (oldSelected == -1)
                                    oldSelected = 0;

                                var newSelectedIndex = EditorGUILayout.Popup("", oldSelected, _shaderInputTextures.ToArray());

                                if (newSelectedIndex != oldSelected)
                                {
                                    result = true;
                                    var updateString = newSelectedIndex == 0 ? string.Empty : _shaderInputTextures[newSelectedIndex];
                                    _outputProperties[i].stringValue = updateString;
                                    MaterialUtils.UpdateTextureTarget(_target.OutputMaterial, _target.Output[i].OutputTexture, oldstring, updateString);

                                    //Clean other outputs that have the same assignment.
                                    for (int j = 0; j < _outputProperties.Count; j++)
                                    {
                                        if (string.Equals(_outputProperties[j].stringValue, updateString) && i != j)
                                        {
                                            _outputProperties[j].stringValue = string.Empty;
                                        }
                                    }
                                }
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;

            return result;
        }

        #endregion Output draw

        #region Presets draw

        private static readonly GUIContent _ApplyPresetGUIContent = new GUIContent("Apply", "Apply preset.");
        private static readonly GUIContent _UpdatePresetGUIContent = new GUIContent("Update", "Update preset.");
        private static readonly GUIContent _DeletePresetGUIContent = new GUIContent("Delete", "Delete preset.");
        private static readonly GUIContent _PresetExportGUIContent = new GUIContent("Export", "Export preset");
        private static readonly GUIContent _CreateNewPresetGUIContent = new GUIContent("Create", "Create preset.");
        private static readonly GUIContent _PresetImportGUIContent = new GUIContent("Import", "Import preset");

        private static readonly GUIContent _CreateNewPresetApplyGUIContent = new GUIContent("Create", "Create");
        private static readonly GUIContent _UpdateApplyGUIContent = new GUIContent("Update", "Update");
        private static readonly GUIContent _DeleteConfirmText = new GUIContent("Confirm", "Confirm");

        private bool _showPresetsUpdate = false;
        private bool _showPresetsCreateField = false;
        private bool _showDeleteLabelText = false;

        private string _createPresetName = string.Empty;
        private string _renamePresetName = string.Empty;

        private static int _presetSelectedIndex = 0;

        private bool DrawPresentExport(SubstanceGraphSO graph)
        {
            var result = false;

            int labelWidth = (int)EditorGUIUtility.labelWidth - 15;

            _showExportPresentationHandler = EditorGUILayout.Foldout(_showExportPresentationHandler, "Preset Handling", true);

            if (_showExportPresentationHandler)
            {
                if (_presetsListProperty != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(6);

                        var presetsToShow = GetPresetsGUIList();

                        var oldIndex = _presetSelectedIndex;

                        _presetSelectedIndex = EditorGUILayout.Popup(oldIndex, presetsToShow, GUILayout.Width(labelWidth));

                        if (oldIndex != _presetSelectedIndex)
                        {
                            ResetPresetsUIState();
                        }

                        EditorGUILayout.BeginVertical();
                        {
                            var isNative = IsPresetNative();

                            //Apply Preset
                            if (GUILayout.Button(_ApplyPresetGUIContent))
                            {
                                HandleApplyPreset(graph);
                                result = true;
                            }

                            //Disable Update and Delete for native presets.
                            if (isNative)
                            {
                                GUI.enabled = false;
                            }
                            {
                                //Update Preset
                                EditorGUILayout.BeginHorizontal();
                                {
                                    if (_showPresetsUpdate)
                                    {
                                        if (GUILayout.Button("Cancel"))
                                        {
                                            _showPresetsUpdate = false;
                                        }
                                    }

                                    if (GUILayout.Button(_showPresetsUpdate ? _UpdateApplyGUIContent : _UpdatePresetGUIContent) && !isNative)
                                    {
                                        if (HandleUpdatePreset(graph))
                                        {
                                            result = true;
                                        }
                                    }

                                    if (_showPresetsUpdate)
                                    {
                                        ShowPresetsRenameField(graph);
                                    }
                                }
                                EditorGUILayout.EndHorizontal();

                                //Delete Preset.
                                EditorGUILayout.BeginHorizontal();
                                {
                                    if (isNative)
                                    {
                                        GUI.enabled = false;
                                    }

                                    if (_showDeleteLabelText)
                                    {
                                        if (GUILayout.Button("Cancel"))
                                        {
                                            _showDeleteLabelText = false;
                                        }
                                    }

                                    if (GUILayout.Button(_showDeleteLabelText ? _DeleteConfirmText : _DeletePresetGUIContent) && !isNative)
                                    {
                                        if (HandleDeletePreset(graph))
                                        {
                                            result = true;
                                        }
                                    }

                                    if (_showDeleteLabelText)
                                    {
                                        ShowDeleteConfirmationField();
                                    }

                                    if (isNative)
                                    {
                                        GUI.enabled = true;
                                    }

                                }
                                EditorGUILayout.EndHorizontal();
                            }                           
                            if (isNative)
                            {
                                GUI.enabled = true;
                            }

                            //Export Preset.
                            if (GUILayout.Button(_PresetExportGUIContent))
                            {
                                HandleExportPresets(graph);
                            }

                            //Creare new Preset.
                            EditorGUILayout.BeginHorizontal();
                            {
                                if (_showPresetsCreateField)
                                {
                                    if (GUILayout.Button("Cancel"))
                                    {
                                        _showPresetsCreateField = false;
                                    }
                                }

                                if (GUILayout.Button(_showPresetsCreateField ? _CreateNewPresetApplyGUIContent : _CreateNewPresetGUIContent))
                                {
                                    if (HandleCreatePreset(graph))
                                    {
                                        result = true;
                                    }
                                }

                                if (_showPresetsCreateField)
                                {
                                    ShowPresetCreateField(graph);
                                }
                            }
                            EditorGUILayout.EndHorizontal();

                            //Import preset.
                            if (GUILayout.Button(_PresetImportGUIContent))
                            {
                                HandleImportPresets(graph);
                                result = true;
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            return result;
        }

        private void HandleExportPresets(SubstanceGraphSO graph)
        {
            var presetLabel = "Default";

            if (_presetSelectedIndex != 0)
            {
                var presetElement = _presetsListProperty.GetArrayElementAtIndex(_presetSelectedIndex - 1);
                var presetInfoProp = presetElement.FindPropertyRelative("Info");
                presetLabel = presetInfoProp.FindPropertyRelative("Label").stringValue;
            }

            string savePath = EditorUtility.SaveFilePanel("Save Preset as...", graph.AssetPath, presetLabel, "sbsprs");

            if (savePath != "")
            {
                string savePreset = "<sbspresets count=\"1\" formatversion=\"1.1\">\n "; //formatting line needed by other integrations
                savePreset += SubstanceEditorEngine.instance.ExportGraphPresetXML(graph);
                savePreset += "</sbspresets>";

                savePreset = ReplaceLabelField(savePreset, presetLabel);

                File.WriteAllText(savePath, savePreset);
            }

            GUIUtility.ExitGUI();
        }

        private void HandleImportPresets(SubstanceGraphSO graph)
        {
            string loadPath = EditorUtility.OpenFilePanel("Select Preset", graph.AssetPath, "sbsprs");

            if (loadPath != "")
            {
                string presetFile = System.IO.File.ReadAllText(loadPath);

                int startIndex = presetFile.IndexOf("<sbspreset ");
                int endIndex = presetFile.IndexOf("sbspreset>") + 10;
                var presetXML = presetFile.Substring(startIndex, endIndex - startIndex);

                var label = Path.GetFileName(loadPath);

                CreateNewPreset(label, string.Empty, string.Empty, presetXML);

                SubstanceEditorEngine.instance.LoadPresetsToGraph(graph, presetXML);
            }

            GUIUtility.ExitGUI();
        }

        private void HandleApplyPreset(SubstanceGraphSO graph)
        {
            if (_presetsListProperty == null)
            {
                return;
            }

            if (_presetsListProperty.arraySize <= _presetSelectedIndex - 1)
            {
                return;
            }

            if (_presetSelectedIndex == 0)
            {
                SubstanceEditorEngine.instance.LoadPresetsToGraph(graph, graph.DefaultPreset);
                return;
            }

            var presetElement = _presetsListProperty.GetArrayElementAtIndex(_presetSelectedIndex - 1);
            var isNative = presetElement.FindPropertyRelative("NativePreset").boolValue;

            if (isNative)
            {
                SubstanceEditorEngine.instance.LoadBakedPresetsToGraph(graph, _presetSelectedIndex - 1);
            }
            else
            {
                var presetValue = presetElement.FindPropertyRelative("PresetValue").stringValue;
                SubstanceEditorEngine.instance.LoadPresetsToGraph(graph, presetValue);
            }
        }

        private bool HandleCreatePreset(SubstanceGraphSO graph)
        {
            if (_presetsListProperty == null)
                return false;

            if (!_showPresetsCreateField)
            {
                _showPresetsCreateField = true;
                _createPresetName = string.Empty;
                return false;
            }

            _showPresetsCreateField = false;

            if (string.IsNullOrEmpty(_createPresetName))
            {
                Debug.LogWarning("Presets label can not be null;");
                return false;
            }

            CreateNewPreset(_createPresetName, string.Empty, string.Empty, _nativeGraph.CreatePresetFromCurrentState());
            return true;
        }

        private bool HandleDeletePreset(SubstanceGraphSO graph)
        {
            if (_presetsListProperty == null)
            {
                return false;
            }

            if (_presetsListProperty.arraySize <= _presetSelectedIndex - 1)
            {
                return false;
            }

            if (!_showDeleteLabelText)
            {
                _showDeleteLabelText = true;
                return false;
            }

            _presetsListProperty.DeleteArrayElementAtIndex(_presetSelectedIndex - 1);
            _presetSelectedIndex = 0;
            _showDeleteLabelText = false;

            return true;
        }

        private bool HandleUpdatePreset(SubstanceGraphSO graph)
        {
            if (_presetSelectedIndex == 0)
            {
                Debug.LogWarning("This preset is baked into the sbsar file and can not be updated.");
                return false;
            }

            if (_presetsListProperty == null)
            {
                _showPresetsUpdate = false;
                Debug.LogWarning("Invalid preset to rename");
                return false;
            }

            if (_presetsListProperty.arraySize <= _presetSelectedIndex - 1)
            {
                _showPresetsUpdate = false;
                Debug.LogWarning("Invalid preset to rename");
                return false;
            }

            var presetElement = _presetsListProperty.GetArrayElementAtIndex(_presetSelectedIndex - 1);
            var presetInfoProp = presetElement.FindPropertyRelative("Info");
            var labelProperty = presetInfoProp.FindPropertyRelative("Label");
            var presetValueProp = presetElement.FindPropertyRelative("PresetValue");

            bool result = false;

            if (_showPresetsUpdate)
            {
                if (string.IsNullOrEmpty(_renamePresetName))
                {
                    Debug.LogWarning("Presets label can not be null;");
                }
                else
                {
                    labelProperty.stringValue = _renamePresetName;
                    presetValueProp.stringValue = _nativeGraph.CreatePresetFromCurrentState();
                    result = true;
                }

                _renamePresetName = string.Empty;
            }
            else
            {
                _renamePresetName = labelProperty.stringValue;
            }

            _showPresetsUpdate = !_showPresetsUpdate;

            return result;
        }

        private void ShowPresetsRenameField(SubstanceGraphSO graph)
        {          
            int labelWidth = ((int)EditorGUIUtility.labelWidth) / 2;

            _renamePresetName = GUILayout.TextField(_renamePresetName, GUILayout.Width(labelWidth));
        }

        private void ShowPresetCreateField(SubstanceGraphSO graph)
        {
            int labelWidth = ((int)EditorGUIUtility.labelWidth) / 2;
            _createPresetName = GUILayout.TextField(_createPresetName, GUILayout.Width(labelWidth));
        }

        private void ShowDeleteConfirmationField()
        {
            int labelWidth = ((int)EditorGUIUtility.labelWidth) / 2;

            GUILayout.Label("Delete?", GUILayout.Width(labelWidth));
        }

        private void CreateNewPreset(string label, string description, string URL, string value)
        {
            var index = _presetsListProperty.arraySize;

            _presetsListProperty.InsertArrayElementAtIndex(index);

            var presetElement = _presetsListProperty.GetArrayElementAtIndex(index);

            presetElement.FindPropertyRelative("NativePreset").boolValue = false;
            presetElement.FindPropertyRelative("PresetValue").stringValue = value;
            presetElement.FindPropertyRelative("PresetIndex").intValue = 0;
            var presetInfoProp = presetElement.FindPropertyRelative("Info");
            presetInfoProp.FindPropertyRelative("Label").stringValue = label;
            presetInfoProp.FindPropertyRelative("Description").stringValue = description;
            presetInfoProp.FindPropertyRelative("URL").stringValue = URL;
        }

        private bool IsPresetNative()
        {
            if (_presetSelectedIndex == 0)
                return true;

            var presetElement = _presetsListProperty.GetArrayElementAtIndex(_presetSelectedIndex - 1);
            var isNative = presetElement.FindPropertyRelative("NativePreset").boolValue;

            return isNative;
        }

        private string[] GetPresetsGUIList()
        {
            var arraySize = _presetsListProperty.arraySize;

            string[] result = new string[arraySize + 1];
            result[0] = "Default";

            for (int i = 0; i < arraySize; i++)
            {
                var presetElement = _presetsListProperty.GetArrayElementAtIndex(i);
                var presetInfoProp = presetElement.FindPropertyRelative("Info");
                result[i + 1] = presetInfoProp.FindPropertyRelative("Label").stringValue;

                if (string.IsNullOrEmpty(result[i + 1]))
                {
                    result[i + 1] = "empty";
                }
            }

            return result;
        }

        private void ResetPresetsUIState()
        {
            _showPresetsUpdate = false;
            _showPresetsCreateField = false;
            _showDeleteLabelText = false;
            _createPresetName = string.Empty;
            _renamePresetName = string.Empty;
        }

        private string ReplaceLabelField(string presetXML, string label)
        {
            return presetXML.Replace("label=\"\"", $"label=\"{label}\"");
        }

        #endregion Presets draw

        #region Thumbnail preview

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            if (_target.HasThumbnail)
                return _target.GetThumbnailTexture();

            var icon = UnityPackageInfo.GetSubstanceIcon(width, height);

            if (icon != null)
            {
                Texture2D tex = new Texture2D(width, height);
                EditorUtility.CopySerialized(icon, tex);
                return tex;
            }

            return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        #endregion Thumbnail preview

        #endregion Draw

        #region Scene Drag

        static Renderer s_previousDraggedUponRenderer;
        static Material[] s_previousMaterialValue;
        static bool s_previousAlreadyHadPrefabModification;

        private const string undoAssignMaterial = "Assign Material";

        public void OnSceneDrag(SceneView sceneView, int index)
        {
            Event evt = Event.current;

            if (evt.type == EventType.Repaint)
                return;

            var go = HandleUtility.PickGameObject(evt.mousePosition, out int materialIndex);

            if (EditorMaterialUtility.IsBackgroundMaterial((_target.OutputMaterial as Material)))
            {
                ClearDragMaterialRendering();
            }
            else if (go && go.GetComponent<Renderer>())
            {
                if (_target.OutputMaterial != null)
                {
                    if (go && go.GetComponent<Renderer>())
                    {
                        HandleRenderer(go.GetComponent<Renderer>(), materialIndex, _target.OutputMaterial, evt.type, evt.alt);
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                        if (_target.IsRuntimeOnly)
                        {
                            var runtimeComponent = go.GetComponent<Adobe.Substance.Runtime.SubstanceRuntimeGraph>();

                            if (runtimeComponent == null)
                                runtimeComponent = go.AddComponent<Adobe.Substance.Runtime.SubstanceRuntimeGraph>();

                            runtimeComponent.AttachGraph(_target);
                        }
                    }
                }
            }
            else
            {
                ClearDragMaterialRendering();
            }
        }

        internal static void HandleRenderer(Renderer r, int materialIndex, Material dragMaterial, EventType eventType, bool alt)
        {
            var applyMaterial = false;
            switch (eventType)
            {
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    applyMaterial = true;
                    break;

                case EventType.DragPerform:
                    DragAndDrop.AcceptDrag();
                    applyMaterial = true;

                    ClearDragMaterialRendering();
                    break;
            }
            if (applyMaterial)
            {
                if (eventType != EventType.DragPerform)
                {
                    ClearDragMaterialRendering();
                    s_previousDraggedUponRenderer = r;
                    s_previousMaterialValue = r.sharedMaterials;

                    // Update prefab modification status cache
                    s_previousAlreadyHadPrefabModification = false;
                    if (PrefabUtility.GetPrefabInstanceStatus(s_previousDraggedUponRenderer) == PrefabInstanceStatus.Connected)
                    {
                        var materialRendererSerializedObject = new SerializedObject(s_previousDraggedUponRenderer).FindProperty("m_Materials");
                        s_previousAlreadyHadPrefabModification = materialRendererSerializedObject.prefabOverride;
                    }
                }

                Undo.RecordObject(r, undoAssignMaterial);
                var materials = r.sharedMaterials;

                bool isValidMaterialIndex = (materialIndex >= 0 && materialIndex < r.sharedMaterials.Length);
                if (!alt && isValidMaterialIndex)
                {
                    materials[materialIndex] = dragMaterial;
                }
                else
                {
                    for (int q = 0; q < materials.Length; ++q)
                        materials[q] = dragMaterial;
                }

                r.sharedMaterials = materials;
            }
        }

        private static void ClearDragMaterialRendering()
        {
            TryRevertDragChanges();
            s_previousDraggedUponRenderer = null;
            s_previousMaterialValue = null;
        }

        private static void TryRevertDragChanges()
        {
            if (s_previousDraggedUponRenderer != null)
            {
                bool hasRevert = false;
                if (!s_previousAlreadyHadPrefabModification &&
                    PrefabUtility.GetPrefabInstanceStatus(s_previousDraggedUponRenderer) == PrefabInstanceStatus.Connected)
                {
                    var materialRendererSerializedObject = new SerializedObject(s_previousDraggedUponRenderer).FindProperty("m_Materials");
                    PrefabUtility.RevertPropertyOverride(materialRendererSerializedObject, InteractionMode.AutomatedAction);
                    hasRevert = true;

                    if (!hasRevert)
                        s_previousDraggedUponRenderer.sharedMaterials = s_previousMaterialValue;
                }
            }
        }

        #endregion Scene Drag

        #region Utilities

        private void SaveTGAFiles()
        {
            if (_target == null)
                return;

            if (_target.IsRuntimeOnly)
                return;

            _target.OutputRemaped = true;
            _target.RenderTextures = true;
            EditorUtility.SetDirty(_target);
        }

        private Rect DrawHighlightBox(float width, float height, float xPadding)
        {
            float bx, by, bw, bh;

            bx = xPadding;
            by = GetPosition();
            bw = width - xPadding;
            bh = height;

            var boxRect = new Rect(bx, by, bw, bh);

            var backgroundStyle = new GUIStyle();
            backgroundStyle.normal.background = _backgroundImage;
            GUI.Box(boxRect, GUIContent.none, backgroundStyle);
            return boxRect;
        }

        private int GetPosition()
        {
            Rect rect = GUILayoutUtility.GetLastRect();

            if ((rect.x != 0) || (rect.y != 0))
                lastRect = rect;

            return (int)lastRect.y;
        }

        /// This is a workaround a bug in the Unity asset database for generating materials previews.
        /// It basically generated a previews image whenever a property changes in the material, but it is now considering changes in the
        /// textures assign to the material itself. By adding a random label we ensure that the asset preview image will be updated.
        private void UpdateGraphMaterialLabel()
        {
            if (_target == null)
                return;

            const string tagPrefix = "sb_";

            var material = _target.OutputMaterial;

            if (material != null)
            {
                var labels = AssetDatabase.GetLabels(material);
                var newLabels = labels.Where(a => !a.Contains(tagPrefix)).ToList();
                newLabels.Add($"{tagPrefix}{Guid.NewGuid().ToString("N")}");
                AssetDatabase.SetLabels(material, newLabels.ToArray());
            }
        }

        #endregion Utilities

        #region Presets Config

        private bool _presetsFirstConfig = false;

        private bool FirstConfigurePresets()
        {
            if (_presetsFirstConfig)
                return false;

            _presetsFirstConfig = true;

            if (_presetsListProperty == null)
                return false;

            if (_presetsListProperty.arraySize != 0)
                return false;

            var bakedPresets = _nativeGraph.GetPresetsList();

            if (bakedPresets.Count == 0)
                return false;

            for (int i = 0; i < bakedPresets.Count; i++)
            {
                _presetsListProperty.InsertArrayElementAtIndex(i);
                var presetElement = _presetsListProperty.GetArrayElementAtIndex(i);

                presetElement.FindPropertyRelative("NativePreset").boolValue = true;
                presetElement.FindPropertyRelative("PresetValue").stringValue = string.Empty;
                presetElement.FindPropertyRelative("PresetIndex").intValue = i;

                var presetInfoProp = presetElement.FindPropertyRelative("Info");
                presetInfoProp.FindPropertyRelative("Label").stringValue = bakedPresets[i].Label;
                presetInfoProp.FindPropertyRelative("Description").stringValue = bakedPresets[i].Description;
                presetInfoProp.FindPropertyRelative("URL").stringValue = bakedPresets[i].URL;
            }

            return true;
        }

        #endregion Presets Config

        /// Work around Unity SerializedObjectNotCreatableException during script compilation.
        private bool IsSerializedObjectReady()
        {
            try
            {
                if (serializedObject.targetObject == null)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}