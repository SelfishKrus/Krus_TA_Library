using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class GrassMaskPainter : MonoBehaviour
{
    Vector3 mousePos;

    public Vector3 hitPosGizmo;

    public LayerMask hitMask = 1;


    
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
            // only allow painting while this object is selected
            if ((Selection.Contains(gameObject)))
            {
                Event e = Event.current;
                RaycastHit terrainHit;

                // convert mouse position in pixel space to scene view
                mousePos = e.mousePosition;
                float ppp = EditorGUIUtility.pixelsPerPoint;
                mousePos.y = scene.camera.pixelHeight - mousePos.y * ppp;
                mousePos.x *= ppp;

                // ray for gizmo(disc)
                Ray rayGizmo = scene.camera.ScreenPointToRay(mousePos);
                RaycastHit hitGizmo;

                if (Physics.Raycast(rayGizmo, out hitGizmo, 200f, hitMask.value))
                {
                    hitPosGizmo = hitGizmo.point;
                }

                if (e.type == EventType.MouseDrag && e.button == 1)
                {
                    
                }
            }
        }
    #endif 
}
