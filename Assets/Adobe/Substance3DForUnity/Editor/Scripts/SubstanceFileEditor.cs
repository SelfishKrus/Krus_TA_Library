using UnityEngine;
using UnityEditor;
using System.IO;
using Adobe.Substance;
using Adobe.SubstanceEditor.Importer;
using UnityEditor.SceneManagement;
using UnityEditor.Graphs;

namespace Adobe.SubstanceEditor
{
    [CustomEditor(typeof(SubstanceFileSO))]
    [CanEditMultipleObjects]
    public class SubstanceFileEditor : UnityEditor.Editor
    {
        private SubstanceFileSO _target;

        public void OnEnable()
        {
            _target = serializedObject.targetObject as SubstanceFileSO;
        }

        /// <summary>
        /// Callback for GUI events to block substance files from been duplicated.
        /// </summary>
        /// <param name="guid">Asset guid.</param>
        /// <param name="rt">GUI rect.</param>
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

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            if (_target == null)
                return null;

            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_target)) as SubstanceImporter;

            if (importer == null)
                return null;

            var defaultGraph = importer.GetDefaultGraph();

            if (defaultGraph == null)
                return null;

            if (defaultGraph.HasThumbnail)
            {
                var thumbnailTexture = defaultGraph.GetThumbnailTexture();
                return thumbnailTexture;
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

            if (go && go.GetComponent<Renderer>())
            {
                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_target)) as SubstanceImporter;

                if (importer != null)
                {
                    var defaultGraph = importer.GetDefaultGraph();

                    if (defaultGraph != null && defaultGraph.OutputMaterial != null)
                    {
                        if (go && go.GetComponent<Renderer>())
                        {
                            HandleRenderer(go.GetComponent<Renderer>(), materialIndex, defaultGraph.OutputMaterial, evt.type, evt.alt);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }
                    }
                }
            }
            else
            {
                ClearDragMaterialRendering();
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

        #endregion Scene Drag
    }
}