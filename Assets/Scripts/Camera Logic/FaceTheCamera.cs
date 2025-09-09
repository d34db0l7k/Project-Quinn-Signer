using UnityEngine;

public class FaceTheCamera : MonoBehaviour
{
    UnityEngine.Camera cam;

    void Start()
    {
        cam = UnityEngine.Camera.main;
    }


    void LateUpdate()
    {
        if (cam == null) return;
        // face the camera
        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.transform.position,
            Vector3.up
        );
    }
}