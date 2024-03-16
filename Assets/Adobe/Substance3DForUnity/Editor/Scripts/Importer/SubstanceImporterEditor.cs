using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using Adobe.Substance;

#if UNITY_2020_2_OR_NEWER

using UnityEditor.AssetImporters;

#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace Adobe.SubstanceEditor.Importer
{
    [CustomEditor(typeof(SubstanceImporter)), CanEditMultipleObjects]
    internal class SubstanceImporterEditor : ScriptedImporterEditor
    {
        protected override bool needsApplyRevert => false;

        public override bool showImportedObject => false;

        private int _selectedInstance = 0;

        //Size of the preview thumbnail for instances in the list.
        private const int _instanceListPreviewSize = 64;

        private const int _isntanceListElementSpacing = 0;

        private Vector2 _scrollPosition = Vector2.zero;

        public SubstanceImporter _importer;

        public List<SubstanceGraphSO> _graphs;

        public string _tempLabelName;

        private Dictionary<SubstanceGraphSO, SubstanceGraphSOEditor> _elementsEditors;

        private Dictionary<SubstanceGraphSO, MaterialEditor> _previewEditors;

        private SubstanceGraphSOEditor _currentEditor;

        private Texture2D _backgroundImage = default;

        private Texture2D _textHightlightBackground = default;

        public override void OnEnable()
        {
            base.OnEnable();

            _elementsEditors = new Dictionary<SubstanceGraphSO, SubstanceGraphSOEditor>();
            _previewEditors = new Dictionary<SubstanceGraphSO, MaterialEditor>();
            _importer = target as SubstanceImporter;
            _graphs = _importer._fileAsset.GetGraphs();

            EditorApplication.projectWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
            Undo.undoRedoPerformed += UndoCallback;
            EnsureRefreshedMaterials();
            EnsureRequiredTextures();
        }

        public override void OnDisable()
        {
            EditorApplication.projectWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
            Undo.undoRedoPerformed -= UndoCallback;
            SaveCurrentInsatance();
            base.OnDisable();
        }

        private void SaveCurrentInsatance()
        {
            if (_currentEditor != null)
            {
                _currentEditor.SaveEditorChanges();
                return;
            }

            if (_graphs == null || _graphs.Count <= _selectedInstance)
            {
                return;
            }

            var currentInstance = _graphs[_selectedInstance];

            if (_elementsEditors.TryGetValue(currentInstance, out SubstanceGraphSOEditor editor))
                editor.SaveEditorChanges();
        }

        private void UndoCallback()
        {
            if (Selection.activeObject is SubstanceFileSO)
            {
                var target = Selection.activeObject as SubstanceFileSO;

                if (_importer._fileAsset == target)
                {
                    var targetGraph = _graphs[_selectedInstance];
                    targetGraph.RenderTextures = true;
                    SubstanceEditorEngine.instance.PushAllInputsToUpdate();
                    EditorUtility.SetDirty(targetGraph);
                }
            }
        }

        protected static void OnHierarchyWindowItemOnGUI(string guid, Rect rt)
        {
            var currentEvent = Event.current;

            if ("Duplicate" == currentEvent.commandName && currentEvent.type == EventType.ExecuteCommand)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                if (Path.GetExtension(assetPath) == ".sbsar")
                {
                    Debug.LogWarning("Substance graph can not be manually duplicated.");
                    currentEvent.Use();
                }
            }
        }

        #region Draw Body

        public override void OnInspectorGUI()
        {
            DrawMainUI();
        }

        private void DrawMainUI()
        {
            if (_graphs.Count == 0)
                return;

            EditorGUILayout.BeginVertical();
            {
                //Draw instances list.
                EditorGUILayout.BeginVertical();
                DrawInstancesListSection();
                EditorGUILayout.EndVertical();
                DrawUILine();

                //Draw shader UI.
                EditorGUILayout.BeginHorizontal();
                DrawShaderSelectionSection();
                EditorGUILayout.EndHorizontal();

                DrawUILine();

                //Draw selected instance properties.
                EditorGUILayout.BeginVertical();
                DrawSelectedInstanceProperties(_graphs[_selectedInstance]);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawInstancesListSection()
        {
            int numGraphs = _graphs.Count;

            if (numGraphs == 0)
                return;

            //Instance UI width = preview texture size + padding.
            float entryWidth = _instanceListPreviewSize + 16;

            //Instance UI height = preview texture size + name text.
            float entryHeight = _instanceListPreviewSize + EditorGUIUtility.singleLineHeight;

            EditorGUILayout.LabelField("Substance Graphs");
            EditorGUILayout.Space();

            using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(_scrollPosition, false, false))
            {
                scrollViewScope.handleScrollWheel = false;
                _scrollPosition = scrollViewScope.scrollPosition;

                var listStyle = new GUIStyle();
                listStyle.padding = new RectOffset(15, 15, 15, 15);

                var rect = EditorGUILayout.BeginHorizontal(listStyle);
                {
                    //Gray area
                    var targetRect = new Rect(rect.x + 5, rect.y, rect.width - 10, rect.height);
                    DrawGrayRectangle(targetRect);

                    //Text styles
                    var normalTextStyle = new GUIStyle();
                    normalTextStyle.wordWrap = true;
                    normalTextStyle.alignment = TextAnchor.MiddleCenter;

                    if (EditorGUIUtility.isProSkin)
                        normalTextStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1);

                    var highlightTextStyle = new GUIStyle();
                    highlightTextStyle.alignment = TextAnchor.MiddleCenter;
                    highlightTextStyle.normal.textColor = Color.white;
                    highlightTextStyle.normal.background = _textHightlightBackground;
                    highlightTextStyle.wordWrap = true;

                    for (int instanceIndex = 0; instanceIndex < numGraphs; instanceIndex++)
                    {
                        if (TryGetInstanceByIndex(instanceIndex, out SubstanceGraphSO instance))
                        {
                            EditorGUILayout.BeginVertical(GUILayout.Width(entryWidth), GUILayout.Height(entryHeight));
                            {
                                DrawListElement(instance, instanceIndex, entryWidth - 12, normalTextStyle, highlightTextStyle);
                            }
                            EditorGUILayout.EndVertical();
                            GUILayout.Space(_isntanceListElementSpacing);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            DrawAddAndRemove();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGrayRectangle(Rect rect)
        {
            var style = new GUIStyle();
            style.normal.background = _backgroundImage;
            GUI.Box(rect, GUIContent.none, style);
        }

        private void DrawListElement(SubstanceGraphSO instance, int instanceIndex, float entryWidth, GUIStyle normalStyle, GUIStyle highlightStyle)
        {
            Material graphMaterial = instance.OutputMaterial;

            if (graphMaterial == null)
                return;

            Texture2D miniThumbnail = AssetPreview.GetAssetPreview(graphMaterial);

            if (GUILayout.Button(miniThumbnail, GUILayout.Width(_instanceListPreviewSize),
                                                GUILayout.Height(_instanceListPreviewSize)))
            {
                OnInstanceSelected(instanceIndex);
                return;
            }

            if (instanceIndex != _selectedInstance)
            {
                GUILayout.Label(instance.Name, normalStyle, GUILayout.Width(entryWidth));
                return;
            }

            var label = _tempLabelName ?? instance.Name;
            _tempLabelName = GUILayout.TextField(label, highlightStyle, GUILayout.Width(entryWidth));

            if (!_tempLabelName.Equals(instance.Name, StringComparison.Ordinal))
            {
                Event e = Event.current;
                if (e.keyCode == KeyCode.Return)
                {
                    if (e.type == EventType.KeyUp)
                    {
                        if (!TryRenameInstance(instance, _tempLabelName))
                        {
                            EditorUtility.DisplayDialog("Error", "The provided name can't be assigned to a substance instance.", "Ok");
                            _tempLabelName = instance.Name;
                        }
                    }
                }
            }
        }

        private void DrawAddAndRemove()
        {
            if (GUILayout.Button("Copy graph"))
            {
                CreateNewInstance();
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("Delete graph"))
            {
                if (TryGetSelectedInstance(out SubstanceGraphSO instanceSO))
                {
                    DeleteInstance(instanceSO);
                    GUIUtility.ExitGUI();
                }
            }
        }

        private void DrawShaderSelectionSection()
        {
            EditorGUILayout.LabelField("Shader", GUILayout.Width(55));

            if (!TryGetCurrentGraph(out SubstanceGraphSO graph))
                return;

            if (graph.OutputMaterial == null || graph.OutputMaterial.shader == null)
                return;

            var currentShader = graph.OutputMaterial.shader;

            var shaderNames = ShaderUtil.GetAllShaderInfo().Select((a) => a.name).Where(b => !b.StartsWith("Hidden") && !b.StartsWith("GUI")).ToArray();
            var selectedElement = shaderNames.FirstOrDefault(a => a == currentShader.name);
            var selectedIndex = Array.IndexOf(shaderNames, selectedElement);

            var newSelected = EditorGUILayout.Popup(selectedIndex, shaderNames, GUILayout.MaxWidth(320));

            if (newSelected != selectedIndex)
            {
                var newSelectedElement = shaderNames[newSelected];
                _graphs[_selectedInstance].OutputMaterial.shader = Shader.Find(newSelectedElement);
                EditorUtility.SetDirty(_graphs[_selectedInstance].OutputMaterial);
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Edit", GUILayout.MaxWidth(60)))
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = currentShader;
            }
        }

        private void DrawSelectedInstanceProperties(SubstanceGraphSO currentInstance)
        {
            if (!_elementsEditors.TryGetValue(currentInstance, out SubstanceGraphSOEditor editor))
            {
                editor = SubstanceGraphSOEditor.CreateEditor(currentInstance) as SubstanceGraphSOEditor;
                _elementsEditors.Add(currentInstance, editor);
            }

            if (_currentEditor != null && _currentEditor != editor)
            {
                _currentEditor.SaveEditorChanges();
            }

            _currentEditor = editor;
            editor.OnInspectorGUI();
        }

        #endregion Draw Body

        #region Static Preview

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            if (_importer == null)
                return null;

            if (_graphs[0] == null)
                return null;

            if (_graphs[0].HasThumbnail)
            {
                return _graphs[0].GetThumbnailTexture();
            }
            else
            {
                var icon = UnityPackageInfo.GetSubstanceIcon(width, height);

                if (icon != null)
                {
                    Texture2D tex = new Texture2D(width, height);
                    EditorUtility.CopySerialized(icon, tex);
                    return tex;
                }
            }

            return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        #endregion Static Preview

        #region Preview

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override GUIContent GetPreviewTitle()
        {
            if (!TryGetCurrentGraph(out SubstanceGraphSO graph))
                return new GUIContent("Material", null, "");

            if (graph.OutputMaterial == null)
                return new GUIContent("Material", null, "");

            if (string.IsNullOrEmpty(graph.OutputMaterial.name))
                return new GUIContent("Material", null, "");

            return new GUIContent(graph.OutputMaterial.name, null, "");
        }

        public override void OnPreviewSettings()
        {
            if (_graphs == null || _graphs.Count == 0)
            {
                Debug.LogWarning("No graphs found. Please make sure to not rename folders with that are managed by the Substance Plugin.");
                return;
            }

            var selectedInstance = _graphs[_selectedInstance];

            if (selectedInstance == null)
                return;

            if (!_previewEditors.TryGetValue(selectedInstance, out MaterialEditor editor))
            {
                var material = selectedInstance?.OutputMaterial;

                if (material != null)
                {
                    editor = MaterialEditor.CreateEditor(material) as MaterialEditor;
                    _previewEditors.Add(selectedInstance, editor);
                }
            }

            if (editor != null)
                editor.OnPreviewSettings();
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (_graphs == null || _graphs.Count == 0)
            {
                Debug.LogWarning("No graphs found. Please make sure to not rename folders with that are managed by the Substance Plugin.");
                return;
            }

            var selectedInstance = _graphs[_selectedInstance];

            if (selectedInstance == null)
                return;

            if (!_previewEditors.TryGetValue(selectedInstance, out MaterialEditor editor))
            {
                var material = selectedInstance?.OutputMaterial;

                if (material != null)
                {
                    editor = MaterialEditor.CreateEditor(material) as MaterialEditor;
                    _previewEditors.Add(selectedInstance, editor);
                }
            }

            if (editor != null)
                editor.OnPreviewGUI(r, background);
        }

        public override void DrawPreview(Rect previewArea)
        {
            OnPreviewGUI(previewArea, new GUIStyle());
        }

        #endregion Preview

        /// <summary>
        /// Creates a new instance of a SubstanceGraphSO with a copy of the values from the current selected instance.
        /// </summary>
        /// <param name="name">Name for the new instance.</param>
        /// <param name="currentInstance">Current selected instance.</param>
        private void CreateNewInstance()
        {
            if (!TryGetSelectedInstance(out SubstanceGraphSO rootInstance))
            {
                if (!TryGetInstanceByIndex(0, out rootInstance))
                {
                    return;
                }
            }

            var newInstanceName = GenerateNewInstanceName(rootInstance);

            var instanceAsset = EditorTools.CreateSubstanceGraphCopy(newInstanceName, rootInstance);
            SubstanceEditorEngine.instance.RenderInstanceAsync(instanceAsset);

            _graphs.Add(instanceAsset);
            EditorUtility.SetDirty(_importer);
            AssetDatabase.Refresh();
            _selectedInstance = _graphs.Count - 1;
            ResetTempName();
        }

        private void DeleteInstance(SubstanceGraphSO instance)
        {
            if (instance.IsRoot)
            {
                EditorUtility.DisplayDialog("Invalid operation", "Can't delete root instance.", "OK");
                return;
            }

            _graphs.Remove(instance);
            EditorUtility.SetDirty(_importer);

            SubstanceEditorEngine.instance.ReleaseInstance(instance);
            instance.FlagedForDelete = true;
            EditorUtility.SetDirty(instance);

            var assetPath = AssetDatabase.GetAssetPath(instance);
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.Refresh();
            _selectedInstance = 0;
            ResetTempName();
        }

        private void OnInstanceSelected(int instanceIndex)
        {
            _selectedInstance = instanceIndex;

            if (TryGetSelectedInstance(out SubstanceGraphSO target))
                EditorGUIUtility.PingObject(target);

            ResetTempName();

            GUIUtility.ExitGUI();
        }

        private bool TryRenameInstance(SubstanceGraphSO instanceSO, string name)
        {
            if (!IsValidAndNoConflict(name))
                return false;

            instanceSO.Rename(name);
            return true;
        }

        #region Utilities

        private string GenerateNewInstanceName(SubstanceGraphSO currentInstance)
        {
            var index = _graphs.Count;
            var newName = currentInstance.Name + $"_copy";

            while (!IsValidAndNoConflict(newName))
                newName = currentInstance.Name + $"__copy{index++}";

            return newName;
        }

        private bool IsValidAndNoConflict(string name)
        {
            if (!IsValidName(name))
                return false;

            if (_graphs.Where(a => a != null).FirstOrDefault(a => a.Name == name) != null)
                return false;

            return true;
        }

        private bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                return false;

            Regex containsABadCharacter = new Regex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]");

            if (containsABadCharacter.IsMatch(name))
                return false;

            return true;
        }

        private bool TryGetCurrentGraph(out SubstanceGraphSO graph)
        {
            graph = null;

            if (!TryGetSelectedInstance(out graph))
                return false;

            return true;
        }

        private bool TryGetInstanceByIndex(int index, out SubstanceGraphSO instance)
        {
            instance = null;

            if (_importer == null || _graphs == null || _graphs.Count == 0)
                return false;

            if (_graphs.Count <= index)
                return false;

            instance = _graphs[index];

            return instance != null;
        }

        private bool TryGetSelectedInstance(out SubstanceGraphSO instance)
        {
            return TryGetInstanceByIndex(_selectedInstance, out instance);
        }

        private static void DrawUILine()
        {
            var rect = EditorGUILayout.BeginVertical();
            {
                Handles.color = Color.black;
                EditorGUILayout.Space(15);
                Handles.DrawLine(new Vector2(rect.x - 40, rect.y + (rect.height / 2)), new Vector2(rect.width + 20, rect.y + (rect.height / 2)));
                EditorGUILayout.Space(15);
            }
            EditorGUILayout.EndVertical();
        }

        private void ResetTempName()
        {
            if (TryGetSelectedInstance(out SubstanceGraphSO material))
                _tempLabelName = material.Name;
        }

        private void EnsureRequiredTextures()
        {
            float c = (EditorGUIUtility.isProSkin) ? 0.35f : 0.65f;
            _backgroundImage = Globals.CreateColoredTexture(64, 64, new Color(c, c, c, 1));
            _textHightlightBackground = Globals.CreateColoredTexture(_instanceListPreviewSize, _instanceListPreviewSize, Color.gray);
        }

        private void EnsureRefreshedMaterials()
        {
            foreach (var instance in _graphs)
            {
                if (instance == null)
                    return;

                var material = instance.OutputMaterial;

                if (material == null)
                    continue;

                EditorUtility.SetDirty(material);
            }

            AssetDatabase.Refresh();
        }

        #endregion Utilities
    }
}