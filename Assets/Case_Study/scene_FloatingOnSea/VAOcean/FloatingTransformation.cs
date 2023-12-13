using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTransformation : MonoBehaviour
{
    GameObject ocean;
    Mesh oceanMesh;
    Vector3 oceanVertex_initialPos;
    Vector3 this_initialPos;
    int nearestVertex;

    [SerializeField]
    Vector3 offset = new Vector3(0, 0, 0);

    // Start is called before the first frame update
    void Start()
    {
        this_initialPos = transform.position;

        ocean = GameObject.Find("SM_OceanSimulation");
        oceanMesh = ocean.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        Vector3[] verticesOS = oceanMesh.vertices;
        float minDistance = Mathf.Infinity;
        for (int i = 0; i < verticesOS.Length; i++)
        {
            Vector3 vertexWS = ocean.transform.TransformPoint(verticesOS[i]);
            float distance = Vector3.Distance(transform.position, vertexWS);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestVertex = i;
            }
        }
        oceanVertex_initialPos = oceanMesh.vertices[nearestVertex];
    }

    // Update is called once per frame
    void Update()
    {
        // Get the world position of the nearest vertex
        Mesh bakedMesh = new Mesh();
        ocean.GetComponent<SkinnedMeshRenderer>().BakeMesh(bakedMesh);
        Vector3 oceanVertex_currentPos = bakedMesh.vertices[nearestVertex];
        oceanVertex_currentPos = ocean.transform.TransformPoint(oceanVertex_currentPos);
        Vector3 dst = (oceanVertex_currentPos - oceanVertex_initialPos) * ocean.transform.localScale.x;

        transform.position = oceanVertex_currentPos + offset ;
    }
}
