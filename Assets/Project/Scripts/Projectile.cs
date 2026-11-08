using UnityEngine;

/// Pocisk (kula z katapulty/armaty) albo meteoryt. Tworzony przez EnemyLauncher i MeteorSpawner.
/// Oprócz zwykłych kolizji sprawdza całą trasę lotu między klatkami fizyki (SphereCast),
/// dzięki czemu szybki pocisk nie przelatuje przez cienką tarczę.
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public float damage = 5f;
    public float lifetime = 25f;
    public GameObject hitEffect;
    [Range(0.1f, 1f)] public float sweepRadiusFactor = 0.5f;   // ułamek promienia pocisku użyty do sprawdzania trasy

    Rigidbody rb;
    Vector3 lastPos;
    float sweepRadius = 0.5f;
    bool done;

    public static Projectile Spawn(GameObject prefab, Vector3 position, float scale, Color color,
                                   float damage, bool useGravity)
    {
        GameObject go;
        if (prefab != null)
        {
            go = Instantiate(prefab, position, Quaternion.identity);
        }
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * scale;
            go.GetComponent<Renderer>().material.color = color;
        }

        var body = go.GetComponent<Rigidbody>();
        if (body == null) body = go.AddComponent<Rigidbody>();
        body.useGravity = useGravity;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        var p = go.GetComponent<Projectile>();
        if (p == null) p = go.AddComponent<Projectile>();
        p.damage = damage;
        return p;
    }

    public void Launch(Vector3 velocity)
    {
        GetComponent<Rigidbody>().linearVelocity = velocity;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lastPos = transform.position;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);

        var col = GetComponentInChildren<Collider>();
        if (col != null)
        {
            Vector3 e = col.bounds.extents;
            sweepRadius = Mathf.Max(0.05f, Mathf.Max(e.x, Mathf.Max(e.y, e.z)) * sweepRadiusFactor);
        }
    }

    void FixedUpdate()
    {
        if (done) return;

        Vector3 pos = rb.position;
        Vector3 delta = pos - lastPos;
        float dist = delta.magnitude;

        if (dist > 0.001f)
        {
            var hits = Physics.SphereCastAll(lastPos, sweepRadius, delta / dist, dist, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var h in hits)
            {
                if (IsIgnored(h.collider)) continue;
                Resolve(h.collider, h.point == Vector3.zero ? pos : h.point);
                return;
            }
        }

        lastPos = pos;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (done) return;
        if (IsIgnored(collision.collider)) return;
        Resolve(collision.collider, transform.position);
    }

    bool IsIgnored(Collider c)
    {
        if (c.transform.IsChildOf(transform)) return true;
        if (c.GetComponentInParent<Projectile>() != null) return true;
        if (c.GetComponentInParent<EnemyLauncher>() != null) return true;
        return false;
    }

    void Resolve(Collider other, Vector3 point)
    {
        done = true;

        if (other.GetComponentInParent<ShieldBlocker>() != null)
        {
            if (GameManager.Instance != null) GameManager.Instance.AddBlock();
        }
        else
        {
            var castle = other.GetComponentInParent<CastleHealth>();
            if (castle != null) castle.TakeDamage(damage);
        }

        if (hitEffect != null) Instantiate(hitEffect, point, Quaternion.identity);
        Destroy(gameObject);
    }
}