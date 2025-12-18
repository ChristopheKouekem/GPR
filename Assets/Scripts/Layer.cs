using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public Transform cam;
    public float parallaxSpeed = 0.7f;

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
        transform.position += new Vector3(deltaMovement.x, 0, 0) * parallaxSpeed;
        lastCamPos = cam.position;
    }
}