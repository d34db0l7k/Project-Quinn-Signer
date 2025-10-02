using UnityEngine;

public class CollectableRotate : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 1f;

    void Update()
    {

        transform.Rotate(0, rotationSpeed, 0, Space.World);
        
    }
}
