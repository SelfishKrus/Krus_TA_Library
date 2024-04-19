using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public float speed = 5.0f;
    public float mouseSensitivity = 2.0f;

    bool isActive = false;
    Vector3 rotation = Vector3.zero;

    void Start()
    {
        rotation = transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        //lock 
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isActive = !isActive;
        }

        #region Move
        if (isActive)
        {
            // Get WASD input
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            // Move 
            Vector3 moveDir = (transform.right * horizontalInput + transform.forward * verticalInput).normalized;
            transform.position += moveDir * speed * Time.deltaTime;

            // Vertical move
            if (Input.GetKey(KeyCode.Q))
            {
                transform.position -= transform.up * speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.E))
            {
                transform.position += transform.up * speed * Time.deltaTime;
            }
            #endregion

            #region Rotation
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            rotation.y += mouseX;
            rotation.x -= mouseY;
            rotation.x = Mathf.Clamp(rotation.x, -90f, 90f);

            transform.eulerAngles = new Vector2(rotation.x, rotation.y);
            #endregion
        }



    }
}
