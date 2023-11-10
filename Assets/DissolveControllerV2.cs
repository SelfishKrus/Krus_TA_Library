using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.VFX;

public class DissolveControllerV2 : MonoBehaviour
{
    public VisualEffect VFX;

    private Material[] skinnedMeshMaterial;
    private SkinnedMeshRenderer skinnedMeshRenderer;

    public float dissolveRate = 0.01f;
    public float refreshRate = 0.001f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown (KeyCode.Space))
        {
            StopAllCoroutines();

            StartCoroutine(VFXDissolveCoroutine());
            StartCoroutine(ShaderDissolveCoroutine());
        }

    }

    IEnumerator VFXDissolveCoroutine()
    {

        if (VFX != null)
        {
            VFX.Play();
            yield return new WaitForSeconds(refreshRate);
        }

    }

    IEnumerator ShaderDissolveCoroutine()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        if (skinnedMeshRenderer != null)
        {
            skinnedMeshMaterial = new Material[skinnedMeshRenderer.materials.Length];

            for (int i = 0; i < skinnedMeshRenderer.materials.Length; i++)
            {
                StartCoroutine(SubShaderDissolveCoroutine(i));
                yield return new WaitForSeconds(refreshRate);
            }
        }
        else{
            Debug.Log("No SkinnedMeshRenderer found");
        }

        IEnumerator SubShaderDissolveCoroutine(int index)
        {
            // initialize
            skinnedMeshMaterial[index] = skinnedMeshRenderer.materials[index];
            skinnedMeshMaterial[index].SetFloat("_ClipThreshold", 0);

            // dissolve 
            while (skinnedMeshMaterial[index].GetFloat("_ClipThreshold") < 1)
            {
                float counter = skinnedMeshMaterial[index].GetFloat("_ClipThreshold");
                counter += dissolveRate;
                skinnedMeshMaterial[index].SetFloat("_ClipThreshold", counter);
                yield return new WaitForSeconds(refreshRate);
            }
        }





    }
}
