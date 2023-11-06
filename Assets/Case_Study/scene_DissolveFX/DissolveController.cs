using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.VFX;

public class DissolveController : MonoBehaviour
{
    public VisualEffect VFX;

    private Material skinnedMeshMaterial;
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
            
            
            StartCoroutine(VFXDissolveCoroutine());
            StartCoroutine(ShaderDissolveCoroutine());
        }

    }

    IEnumerator VFXDissolveCoroutine()
    {

        if (VFX != null)
        {
            VFX.Play();
            VFX.SetFloat ("SpawnPos", 0);
            while (VFX.GetFloat("SpawnPos") < 1) 
            {
                float counter = VFX.GetFloat("SpawnPos");
                counter += dissolveRate;
                VFX.SetFloat("SpawnPos", counter);
                yield return new WaitForSeconds(refreshRate);
            }
        }

    }

    IEnumerator ShaderDissolveCoroutine()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        skinnedMeshMaterial = skinnedMeshRenderer.material;

        if (skinnedMeshMaterial != null) 
        {
            skinnedMeshMaterial.SetFloat("_DissolveThreshold", 0);
            while (skinnedMeshMaterial.GetFloat("_DissolveThreshold") < 1)
            {
                float counter = skinnedMeshMaterial.GetFloat("_DissolveThreshold");
                counter += dissolveRate;
                skinnedMeshMaterial.SetFloat("_DissolveThreshold", counter);
                yield return new WaitForSeconds(refreshRate);
            }
        }

    }
}
