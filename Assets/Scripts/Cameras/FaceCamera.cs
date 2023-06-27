using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform cameraTransform;

    private void Start() 
    {
        cameraTransform = Camera.main.transform;
    }

    private void LateUpdate() 
    {
        transform.LookAt(transform.position + cameraTransform.rotation * Vector3.forward,
            cameraTransform.rotation * Vector3.up);
    }
}
