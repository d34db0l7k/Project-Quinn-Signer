using UnityEngine;

public class CollectableRotate : MonoBehaviour
{
    [SerializeField] float xRotationSpeed = 0f;
    [SerializeField] float yRotationSpeed = 0f;
    [SerializeField] float zRotationSpeed = 0f;

    void Update()
    {

        transform.Rotate(xRotationSpeed, yRotationSpeed, zRotationSpeed, Space.World);
        
    }
}
