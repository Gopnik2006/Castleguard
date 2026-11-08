using UnityEngine;

/// Pocisk (kula z katapulty/armaty) albo meteoryt. Tworzony przez EnemyLauncher i MeteorSpawner.
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public float damage = 5f;
    public float lifetime = 25f;
    public GameObject hitEffect;

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

        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.useGravity = useGravity;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        var p = go.GetComponent<Projectile>();
        if (p == null) p = go.AddComponent<Projectile>();
        p.damage = damage;
        return p;
    }

    public void Launch(Vector3 velocity)
    {
        GetComponent<Rigidbody>().linearVelocity = velocity;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (done) return;

        Collider other = collision.collider;
        // ignoruj inne pociski i wyrzutnie
        if (other.GetComponentInParent<Projectile>() != null) return;
        if (other.GetComponentInParent<EnemyLauncher>() != null) return;

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

        if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
