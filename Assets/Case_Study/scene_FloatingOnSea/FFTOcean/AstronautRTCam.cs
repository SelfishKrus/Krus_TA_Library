using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AstronautRTCam : MonoBehaviour
{   
    Camera cam;
    RenderTexture rt_col;
    RenderTexture rt_depth;

    float farClipPlane_initial;
    Vector3 pos_initial;

    GameObject astronaut;
    FloatingTransformationFFT floatingTransformationFFT;
    Vector3 displacement;
    Vector3 astronautDisplacement;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        rt_depth = new RenderTexture(512, 512, 24, RenderTextureFormat.Depth);
        rt_depth.Create();

        cam.targetTexture = rt_depth;
        cam.depthTextureMode = DepthTextureMode.Depth;

        pos_initial = transform.position;
        farClipPlane_initial = cam.farClipPlane;
        astronaut = GameObject.Find("Mod_TreadingAstronaut");
        floatingTransformationFFT = astronaut.GetComponent<FloatingTransformationFFT>();
    }

    // Update is called once per frame
    void Update()
    {   

        displacement = floatingTransformationFFT.displacement;
        astronautDisplacement = floatingTransformationFFT.astronautDisplacement;

        cam.farClipPlane = farClipPlane_initial + astronautDisplacement.y;
        Vector3 newPosition = cam.transform.position;
        newPosition.y = pos_initial.y + astronaut.transform.position.y;
        cam.transform.position = newPosition;

        Shader.SetGlobalTexture("_AstronautDepthTex", rt_depth);

    }
}
