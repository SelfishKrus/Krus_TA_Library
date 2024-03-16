using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance.Runtime
{
    /// <summary>
    /// Singleton class that handles Substance engine initialization and it is used to get native handlers to substance instances.
    /// </summary>
    public class SubstanceRuntime : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        /// <value>Global singleton instance.</value>
        public static SubstanceRuntime Instance
        {
            get
            {
                if (_instance != null)
                {
                    _instance.Initialize();
                    return _instance;
                }

                _instance = FindObjectOfType<SubstanceRuntime>();

                if (_instance != null)
                {
                    _instance.Initialize();
                    return _instance;
                }

                var go = new GameObject("SubstanceRuntime");
                _instance = go.AddComponent<SubstanceRuntime>();

                _instance.Initialize();
                return _instance;
            }
        }

        private static SubstanceRuntime _instance = null;

        private static bool _isInitialized = false;

        /// <summary>
        /// Handles initialization of the substance engine.
        /// </summary>
        private void Initialize()
        {
            if (_isInitialized)
                return;

            var enginePath = PlatformUtils.GetEnginePath();
            var pluginPath = PlatformUtils.GetPluginPath();
            Engine.Initialize(pluginPath, enginePath);
            _isInitialized = true;
        }

        /// <summary>
        /// Creates a Substance SDK handle for a given SubstanceGraphSO.
        /// </summary>
        /// <param name="substanceInstance">Target SubstanceGraphSO</param>
        /// <returns>Handle that comunicates with the Substance SDK.</returns>
        public SubstanceNativeGraph InitializeInstance(SubstanceGraphSO substanceInstance)
        {
            if (substanceInstance == null)
                return null;

            return Engine.OpenFile(substanceInstance.RawData.FileContent, substanceInstance.GetNativeID());
        }
    }
}