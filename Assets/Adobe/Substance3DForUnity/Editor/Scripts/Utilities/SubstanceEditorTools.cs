using Adobe.Substance;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    /// <summary>
    /// Tools and utilities for users to consume on Editor scripts.
    /// </summary>
    public static class SubstanceEditorTools
    {
        /// <summary>
        /// Set graph float input.
        /// </summary>
        /// <param name="graph">Target graph.</param>
        /// <param name="inputId">Input id.</param>
        /// <param name="value">Value</param>
        public static void SetGraphFloatInput(SubstanceGraphSO graph, int inputId, float value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.floatValue = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        /// <summary>
        /// Set graph float2 input.
        /// </summary>
        /// <param name="graph">Target graph.</param>
        /// <param name="inputId">Input id.</param>
        /// <param name="value">Value</param>
        public static void SetGraphFloat2Input(SubstanceGraphSO graph, int inputId, Vector2 value)
        {
            var so = new SerializedObject(graph);

            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.vector2Value = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        /// <summary>
        /// Set graph float3 input.
        /// </summary>
        /// <param name="graph">Target graph.</param>
        /// <param name="inputId">Input id.</param>
        /// <param name="value">Value</param>
        public static void SetGraphFloat3Input(SubstanceGraphSO graph, int inputId, Vector3 value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.vector3Value = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        /// <summary>
        /// Set graph float4 input.
        /// </summary>
        /// <param name="graph">Target graph.</param>
        /// <param name="inputId">Input id.</param>
        /// <param name="value">Value</param>
        public static void SetGraphFloat4Input(SubstanceGraphSO graph, int inputId, Vector4 value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.vector4Value = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        /// <summary>
        /// Set graph int input.
        /// </summary>
        /// <param name="graph">Target graph.</param>
        /// <param name="inputId">Input id.</param>
        /// <param name="value">Value</param>
        public static void SetGraphIntInput(SubstanceGraphSO graph, int inputId, int value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.intValue = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        /// <summary>
        /// Set graph int2 input.
        /// </summary>
        /// <param name="graph">Target graph.</param>
        /// <param name="inputId">Input id.</param>
        /// <param name="value">Value</param>
        public static void SetGraphInt2Input(SubstanceGraphSO graph, int inputId, Vector2Int value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.vector2IntValue = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        /// <summary>
        /// Set graph int3 input.
        /// </summary>
        /// <param name="graph">Target graph.</param>
        /// <param name="inputId">Input id.</param>
        /// <param name="value">Value</param>
        public static void SetGraphInt3Input(SubstanceGraphSO graph, int inputId, Vector3Int value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.vector3IntValue = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        /// <summary>
        /// Set graph int4 input.
        /// </summary>
        /// <param name="graph">Target graph.</param>
        /// <param name="inputId">Input id.</param>
        /// <param name="value0">Value for x</param>
        /// <param name="value1">Value for y</param>
        /// <param name="value2">Value for z</param>
        /// <param name="value3">Value for w</param>
        public static void SetGraphInt4Input(SubstanceGraphSO graph, int inputId, int value0, int value1, int value2, int value3)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp0 = graphInput.FindPropertyRelative("Data0");
            var dataProp1 = graphInput.FindPropertyRelative("Data1");
            var dataProp2 = graphInput.FindPropertyRelative("Data2");
            var dataProp3 = graphInput.FindPropertyRelative("Data3");
            dataProp0.intValue = value0;
            dataProp1.intValue = value1;
            dataProp2.intValue = value2;
            dataProp3.intValue = value3;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        /// <summary>
        /// Set graph string input.
        /// </summary>
        /// <param name="graph">Target graph.</param>
        /// <param name="inputId">Input id.</param>
        /// <param name="value">Value</param>
        public static void SetGraphInputString(SubstanceGraphSO graph, int inputId, string value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.stringValue = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        /// <summary>
        /// Set graph texture input.
        /// </summary>
        /// <param name="graph">Target graph.</param>
        /// <param name="inputId">Input id.</param>
        /// <param name="value">Value</param>
        public static void SetGraphInputTexture(SubstanceGraphSO graph, int inputId, Texture2D value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.objectReferenceValue = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        /// <summary>
        /// Renders target graph and updates its assets.
        /// </summary>
        /// <param name="graph">Target graph</param>
        public static void RenderGraph(SubstanceGraphSO graph)
        {
            if (!SubstanceEditorEngine.instance.TryGetHandlerFromInstance(graph, out SubstanceNativeGraph _nativeGraph))
            {
                if (!SubstanceEditorEngine.instance.IsInitialized)
                    return;

                SubstanceEditorEngine.instance.InitializeInstance(graph, null, out SubstanceGraphSO _);
            }

            if (SubstanceEditorEngine.instance.TryGetHandlerFromInstance(graph, out _nativeGraph))
            {
                SubstanceEditorEngine.instance.SubmitAsyncRenderWork(_nativeGraph, graph);
            }
        }

        /// <summary>
        /// Creates a preset XML from the current state of the graph object.
        /// </summary>
        /// <param name="graph">Target graph</param>
        /// <returns>XML with current state as a substance preset.</returns>
        public static string CreatePresetFromCurrentState(SubstanceGraphSO graph)
        {
            if (!SubstanceEditorEngine.instance.TryGetHandlerFromInstance(graph, out SubstanceNativeGraph _nativeGraph))
            {
                if (!SubstanceEditorEngine.instance.IsInitialized)
                    return string.Empty;

                SubstanceEditorEngine.instance.InitializeInstance(graph, null, out SubstanceGraphSO _);
            }

            if (SubstanceEditorEngine.instance.TryGetHandlerFromInstance(graph, out _nativeGraph))
                graph.RuntimeInitialize(_nativeGraph, graph.IsRuntimeOnly);

            return _nativeGraph.CreatePresetFromCurrentState();
        }

        /// <summary>
        /// Load preset JSON into the current state of the target SubstanceGraphSO.
        /// </summary>
        /// <param name="graph">Target SubstanceGraphSO. </param>
        /// <param name="presetJSON">Preset text.</param>
        public static void LoadPresetToCurrentState(SubstanceGraphSO graph, string presetJSON)
        {
            if (!SubstanceEditorEngine.instance.TryGetHandlerFromInstance(graph, out SubstanceNativeGraph _nativeGraph))
            {
                if (!SubstanceEditorEngine.instance.IsInitialized)
                    return;

                SubstanceEditorEngine.instance.InitializeInstance(graph, null, out SubstanceGraphSO _);
            }

            if (SubstanceEditorEngine.instance.TryGetHandlerFromInstance(graph, out _nativeGraph))
                graph.RuntimeInitialize(_nativeGraph, graph.IsRuntimeOnly);

            SubstanceEditorEngine.instance.LoadPresetsToGraph(graph, presetJSON);
            SubstanceEditorEngine.instance.SubmitAsyncRenderWork(_nativeGraph, graph);
        }

        /// <summary>
        /// Returns the list of SubstanceGraphSOs associated with a SubstanceFileSO.
        /// </summary>
        /// <param name="fileSO">Target SubstanceFileSO.</param>
        /// <returns>List of related SubstanceGraphSO.</returns>
        public static List<SubstanceGraphSO> GetGraphs(this SubstanceFileSO fileSO)
        {
            var result = new List<SubstanceGraphSO>();

            var path = AssetDatabase.GetAssetPath(fileSO);

            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(SubstanceGraphSO)));

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                SubstanceGraphSO graph = AssetDatabase.LoadAssetAtPath<SubstanceGraphSO>(assetPath);

                if (graph != null)
                {
                    var filePath = AssetDatabase.GetAssetPath(graph.RawData);

                    if (filePath.Equals(path, System.StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(graph);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Updates the native representation of the SubstanceGraphSO object with the value of a given input.
        /// </summary>
        /// <param name="graph">Target SubstanceGraphSO object.</param>
        /// <param name="inputId">Input id.</param>
        private static void UpdateNativeInput(SubstanceGraphSO graph, int inputId)
        {
            if (!SubstanceEditorEngine.instance.TryGetHandlerFromInstance(graph, out SubstanceNativeGraph _nativeGraph))
            {
                if (!SubstanceEditorEngine.instance.IsInitialized)
                    return;

                SubstanceEditorEngine.instance.InitializeInstance(graph, null, out SubstanceGraphSO _);
            }

            if (SubstanceEditorEngine.instance.TryGetHandlerFromInstance(graph, out _nativeGraph))
                graph.RuntimeInitialize(_nativeGraph, graph.IsRuntimeOnly);

            graph.Input[inputId].UpdateNativeHandle(_nativeGraph);
        }
    }
}