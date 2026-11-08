using UnityEngine;

/// Spuszcza meteoryty pod kątem na zamek. Ilość na minutę ustawia się w menu. Powieś na "Spawners".
public class MeteorSpawner : MonoBehaviour
{
    public CastleHealth castle;
    public GameObject meteorPrefab;       // opcjonalnie (ze Rigidbody lub bez)

    [Header("Tor lotu")]
    public float spawnHeight = 180f;      // wysokość startu nad celem
    public float horizontalDistance = 140f; // odległość w poziomie => kąt spadania = atan(wysokość / odległość)
    public float speed = 45f;
    [Range(0f, 1f)] public float impactSpread = 0.6f;

    [Header("Wygląd / obrażenia")]
    public float scale = 6f;
    public float damage = 15f;
    public Color color = new Color(1f, 0.45f, 0.1f);

    float timer = 3f;

    void Start()
    {
        if (castle == null) castle = FindFirstObjectByType<CastleHealth>();
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || castle == null || !gm.IsPlaying) return;

        int perMinute = GameManager.MeteorsPerMinute;
        if (perMinute <= 0) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Spawn();
            timer = 60f / perMinute * Random.Range(0.6f, 1.4f);
        }
    }

    void Spawn()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));

        Vector3 aim = castle.Center + Random.insideUnitSphere * castle.Radius * impactSpread;
        Vector3 start = aim + dir * horizontalDistance + Vector3.up * spawnHeight;

        var meteor = Projectile.Spawn(meteorPrefab, start, scale, color, damage, false);
        meteor.Launch((aim - start).normalized * speed);
    }
}
