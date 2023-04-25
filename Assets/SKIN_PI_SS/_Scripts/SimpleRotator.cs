using UnityEngine;

namespace FI
{

    public class SimpleRotator : MonoBehaviour
    {
        [SerializeField]
        private float speed = 25f;

        // Start is called before the first frame update
        private void Start() { }

        // Update is called once per frame
        void Update()
        {
            Vector3 angles = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(angles.x, angles.y + speed * Time.deltaTime, angles.z);
        }
    }

}