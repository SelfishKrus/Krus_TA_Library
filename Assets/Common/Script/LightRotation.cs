using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightRotation : MonoBehaviour
{   
    [SerializeField]
    private float speed = 50f;

    public float totalRotation = 0f;

    // rotate light in world space with time
    // quit play mode  if it has rotated a whole circle
    void Update()
    {
        float currentRotation = speed * Time.deltaTime;
        totalRotation += currentRotation;
        transform.Rotate(Vector3.up, currentRotation, Space.World);

        if (totalRotation >= 360f)
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}