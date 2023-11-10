using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BoundingBox : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UpdateBounds();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying)
        {
            UpdateBounds();
        }
    }

    void OnValidate()
    {
        UpdateBounds();
    }

    void OnEnale()
    {
        UpdateBounds();
    }

    void UpdateBounds()
    {
        // Get the bounds of the SkinnedMeshRenderer
        Bounds bounds = GetComponent<SkinnedMeshRenderer>().bounds;
        Vector3 boundSize = bounds.size;
        Vector3 boundMin = bounds.min;

        // Get the material list of game object
        Material[] materials;
        if (Application.isPlaying)
        {
            materials = GetComponent<SkinnedMeshRenderer>().materials;
        }
        else
        {
            materials = GetComponent<SkinnedMeshRenderer>().sharedMaterials;
        }
        
        for (int i = 0; i < materials.Length; i++)
        {
            // get shader
            materials[i].SetVector("_BoundSize", boundSize);
            materials[i].SetVector("_BoundMin", boundMin);
        }
    }
}
