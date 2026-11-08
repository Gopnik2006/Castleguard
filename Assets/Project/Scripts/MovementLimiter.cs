using UnityEngine;

/// Ogranicza ruch gracza do prostokąta (np. tylko przed zamkiem).
/// Działa zarówno na ruch XR Origin (locomotion), jak i na fizyczne chodzenie po pokoju:
/// jeśli głowa wyjdzie poza obszar, XR Origin jest cofany.
/// Powieś na XR Origin (VR). Rozmiar podawany jest w jednostkach ŚWIATA (przy gigancie 8x: 1 m w pokoju = 8 jednostek).
public class MovementLimiter : MonoBehaviour
{
    public Transform xrOrigin;      // puste = ten obiekt
    public Camera head;             // puste = Camera.main
    public Transform areaCenter;    // puste = pozycja startowa gracza
    public Vector2 size = new Vector2(30f, 20f);   // szerokość (X) i głębokość (Z) obszaru

    Vector3 startPos;
    float startYaw;

    void Start()
    {
        if (xrOrigin == null) xrOrigin = transform;
        if (head == null) head = Camera.main;
        startPos = xrOrigin.position;
        startYaw = xrOrigin.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (head == null) return;

        Vector3 center = areaCenter != null ? areaCenter.position : startPos;
        float yaw = areaCenter != null ? areaCenter.eulerAngles.y : startYaw;
        Quaternion rot = Quaternion.Euler(0f, yaw, 0f);

        Vector3 local = Quaternion.Inverse(rot) * (head.transform.position - center);
        float cx = Mathf.Clamp(local.x, -size.x * 0.5f, size.x * 0.5f);
        float cz = Mathf.Clamp(local.z, -size.y * 0.5f, size.y * 0.5f);

        Vector3 correction = rot * new Vector3(cx - local.x, 0f, cz - local.z);
        if (correction.sqrMagnitude > 0f) xrOrigin.position += correction;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = areaCenter != null ? areaCenter.position : transform.position;
        float yaw = areaCenter != null ? areaCenter.eulerAngles.y : transform.eulerAngles.y;
        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0f, yaw, 0f), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, 2f, size.y));
    }
}
