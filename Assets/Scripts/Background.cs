using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform cam;
    public float parallaxSpeed = 0.3f;

    private Vector3 lastCamPos;

    void Start()
    {
        if (cam == null)
            cam = Camera.main.transform;
        lastCamPos = cam.position;
    }

    void Update()
    {
        Vector3 deltaMovement = cam.position - lastCamPos;
        transform.position += deltaMovement * parallaxSpeed;
        lastCamPos = cam.position;
    }
}