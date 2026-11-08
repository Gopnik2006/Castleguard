using UnityEngine;

/// Katapulta / armata. Strzela po paraboli tak, żeby pocisk doleciał w zamek po 'flightTime' sekundach.
public class EnemyLauncher : MonoBehaviour
{
    public CastleHealth target;
    public Transform muzzle;
    public GameObject projectilePrefab;   // puste = zwykła kula

    public float flightTime = 3f;         // armata ~2, katapulta ~4 (wyższy łuk)
    public Vector2 fireInterval = new Vector2(4f, 8f);
    [Range(0f, 1f)] public float aimSpread = 0.5f;   // rozrzut celowania (ułamek promienia zamku)
    public float damage = 5f;
    public float projectileScale = 3f;
    public Color projectileColor = Color.black;

    float timer;

    void Start()
    {
        timer = Random.Range(1f, fireInterval.y);
    }

    void Update()
    {
        if (target == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Fire();
            timer = Random.Range(fireInterval.x, fireInterval.y);
        }
    }

    void Fire()
    {
        Vector3 start = muzzle != null ? muzzle.position : transform.position + Vector3.up * 2f;
        Vector3 aim = target.Center + Random.insideUnitSphere * target.Radius * aimSpread;

        // v = d/t - 0.5 * g * t
        Vector3 velocity = (aim - start) / flightTime - 0.5f * Physics.gravity * flightTime;

        var p = Projectile.Spawn(projectilePrefab, start, projectileScale, projectileColor, damage, true);
        p.Launch(velocity);
    }
}
