using UnityEngine;

public class GrassShaderInteractor : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalVector("_InteractorPos0", transform.position);
    }
}
