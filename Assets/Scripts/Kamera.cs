using UnityEngine;

public class SmoothCamera2D : MonoBehaviour
{
    [Header("Ziel (z. B. Spieler)")]
    public Transform target;

    [Header("Bewegungsparameter")]
    [Range(0f, 1f)] public float smoothSpeed = 0.125f; // je kleiner, desto weicher
    public Vector3 offset = new Vector3(0f, 1f, -10f); // z immer negativ für 2D-Kameras

    [Header("Zoom-Einstellungen")]
    public float orthographicSize = 5f; // Größerer Wert = mehr rausgezoomt
    public bool smoothZoom = true;
    [Range(0f, 1f)] public float zoomSmoothSpeed = 0.1f;

    [Header("Kamerabegrenzungen (optional)")]
    public bool useBounds = false;
    public Vector2 minPosition;
    public Vector2 maxPosition;

    [Header("Realistisches Verhalten")]
    public float lookAheadDistance = 2f;   // Wie weit die Kamera vorausblickt
    public float lookAheadSmoothness = 0.1f;

    private Vector3 _velocity = Vector3.zero;
    private Vector3 _lookAheadPos = Vector3.zero;
    private Vector3 _lastTargetPosition;
    private Camera _camera;
    private float _zoomVelocity = 0f;

    void Start()
    {
        _camera = GetComponent<Camera>();
        if (target != null)
            _lastTargetPosition = target.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Zoom anwenden
        if (_camera != null)
        {
            if (smoothZoom)
            {
                _camera.orthographicSize = Mathf.SmoothDamp(
                    _camera.orthographicSize,
                    orthographicSize,
                    ref _zoomVelocity,
                    zoomSmoothSpeed
                );
            }
            else
            {
                _camera.orthographicSize = orthographicSize;
            }
        }

        // Bewegung des Ziels bestimmen
        Vector3 deltaMove = target.position - _lastTargetPosition;
        _lastTargetPosition = target.position;

        // Blickrichtung abhängig von Bewegung
        _lookAheadPos = Vector3.Lerp(
            _lookAheadPos,
            deltaMove.normalized * lookAheadDistance,
            lookAheadSmoothness
        );

        // Zielposition der Kamera
        Vector3 desiredPosition = target.position + offset + _lookAheadPos;

        // Optional: Begrenzungen anwenden
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minPosition.x, maxPosition.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minPosition.y, maxPosition.y);
        }

        // Sanftes Nachziehen
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothSpeed);
        transform.position = smoothedPosition;
    }
}