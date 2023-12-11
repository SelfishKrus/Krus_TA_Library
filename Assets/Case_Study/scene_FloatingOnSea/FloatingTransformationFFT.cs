using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTransformationFFT : MonoBehaviour
{
    GameObject ocean;
    Mesh oceanMesh;
    Vector3 oceanVertex_initialPos;
    Vector3 this_initialPos;
    int nearestVertex;
    
    Texture2D tex;

    [SerializeField]
    Vector3 offset = new Vector3(0, 0, 0);
    [SerializeField]
    float xzScale = 0.1f;
    [SerializeField]
    float yScale = 0.1f;

    FFTOcean fftOcean;

    // Start is called before the first frame update
    void Start()
    {
        this_initialPos = transform.position;

        ocean = GameObject.Find("OceanMesh");
        oceanMesh = ocean.GetComponent<MeshFilter>().mesh;

        // find the nearest vertex
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

        // set position to nearest vertex
        transform.position = ocean.transform.TransformPoint(oceanVertex_initialPos);
    }

    // Update is called once per frame
    void Update()
    {
        // // Find the nearest vertex
        // Vector3[] verticesOS = oceanMesh.vertices;
        // float minDistance = Mathf.Infinity;
        // for (int i = 0; i < verticesOS.Length; i++)
        // {
        //     Vector3 vertexWS = ocean.transform.TransformPoint(verticesOS[i]);
        //     float distance = Vector3.Distance(transform.position, vertexWS);
        //     if (distance < minDistance)
        //     {
        //         minDistance = distance;
        //         nearestVertex = i;
        //     }
        // }
        // oceanVertex_initialPos = oceanMesh.vertices[nearestVertex];

        // // set position to nearest vertex
        // transform.position = ocean.transform.TransformPoint(oceanVertex_initialPos);

        // Get the world position of the nearest vertex
        Mesh bakedMesh = new Mesh();
        bakedMesh = ocean.GetComponent<MeshFilter>().mesh;
        Vector3 oceanVertex_currentPos = bakedMesh.vertices[nearestVertex];
        
        // Get tex coordinate based on posOS
        fftOcean = ocean.GetComponent<FFTOcean>();
        float scale = fftOcean.tileGlobalScale;
        Vector2 uv = new Vector2(oceanVertex_currentPos.x / scale, oceanVertex_currentPos.z / scale);
        uv = new Vector2(Mathf.Abs(uv.x), Mathf.Abs(uv.y));
        uv = new Vector2(uv.x - Mathf.Floor(uv.x), uv.y - Mathf.Floor(uv.y));
        Vector2 pixelPos = uv * fftOcean.RTResolution;
        // Debug.Log(pixelPos);

        // Read displacement data from displacement texture in compute shader
        if (tex == null)
        {
            tex = new Texture2D(fftOcean.RTResolution, fftOcean.RTResolution, TextureFormat.RGBAHalf, true);
        };
        RenderTexture.active = fftOcean.displacementTextures;
        tex.ReadPixels(new Rect(0, 0, fftOcean.displacementTextures.width, fftOcean.displacementTextures.height), 0, 0);
        Color color = tex.GetPixel((int)pixelPos.x, (int)pixelPos.y);
        Vector3 displacement = new Vector3(color.r, color.g, color.b);

        Debug.Log(displacement);
        displacement.x *= xzScale;
        displacement.y *= yScale;
        displacement.z *= xzScale;

        transform.position = oceanVertex_currentPos + offset + displacement;
    }
}
