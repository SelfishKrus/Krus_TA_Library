using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Focus : MonoBehaviour
{
    [SerializeField]
    GameObject target;

    [SerializeField]
    Vector3 offset = new Vector3(-0.45f, 1.5f, 0f);

    [SerializeField]
    [Range(0f, 1f)]
    float scale = 0.5f;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            // create a matrix and make camera look at the target
            transform.LookAt(scale * (target.transform.position + offset));
        }
    }
}
