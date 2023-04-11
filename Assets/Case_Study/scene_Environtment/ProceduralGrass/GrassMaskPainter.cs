using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class GrassMaskPainter : MonoBehaviour
{
    #if UNITY_EDITOR
        void OnFocus()
        {
            // Remove delegate listener if it has previously
            // been assigned.
            SceneView.duringSceneGui -= this.OnScene;
            // Add (or re-add) the delegate.
            SceneView.duringSceneGui += this.OnScene;
        }

        void OnDestroy()
        {
            // When the window is destroyed, remove the delegate
            // so that it will no longer do any drawing.
            SceneView.duringSceneGui -= this.OnScene;
        }
        
        void OnScene(SceneView scene)
        {

        }
    #endif 
}
