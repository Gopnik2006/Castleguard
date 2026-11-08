using System.Collections.Generic;
using UnityEngine;

/// Ustawia na terenie tyle katapult/armat, ile wybrano w menu. Powieś na pustym obiekcie "Spawners".
public class EnemySpawner : MonoBehaviour
{
    public CastleHealth castle;
    public GameObject catapultPrefab;   // opcjonalnie: prefab z komponentem EnemyLauncher
    public GameObject cannonPrefab;     // opcjonalnie

    [Header("Rozmieszczenie wokół zamku")]
    public float minRadius = 80f;
    public float maxRadius = 130f;
    public float arcCenterAngle = 0f;   // 0 = kierunek +Z (świat)
    public float arcWidth = 360f;       // zawęź np. do 120, żeby wrogowie byli tylko z jednej strony

    [Header("Wielkość zastępczych modeli (gdy brak prefabów)")]
    public float placeholderScale = 4f;

    readonly List<EnemyLauncher> active = new List<EnemyLauncher>();
    int spawnedTotal;

    void Start()
    {
        if (castle == null) castle = FindFirstObjectByType<CastleHealth>();
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || castle == null) return;

        int want = gm.State == GameState.Ready ? 0 : GameManager.EnemyCount;

        while (active.Count < want) Spawn();
        while (active.Count > want)
        {
            var last = active[active.Count - 1];
            active.RemoveAt(active.Count - 1);
            if (last != null) Destroy(last.gameObject);
        }
    }

    void Spawn()
    {
        bool cannon = spawnedTotal % 2 == 1;
        spawnedTotal++;

        float angle = (arcCenterAngle + Random.Range(-arcWidth * 0.5f, arcWidth * 0.5f)) * Mathf.Deg2Rad;
        float radius = Random.Range(minRadius, maxRadius);
        Vector3 pos = castle.Center + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;

        Vector3 top = new Vector3(pos.x, castle.Center.y + 1000f, pos.z);
        if (Physics.Raycast(top, Vector3.down, out RaycastHit hit, 3000f, ~0, QueryTriggerInteraction.Ignore))
            pos.y = hit.point.y;

        Vector3 flatDir = castle.Center - pos;
        flatDir.y = 0f;
        Quaternion rot = Quaternion.LookRotation(flatDir.normalized);

        GameObject prefab = cannon ? cannonPrefab : catapultPrefab;
        GameObject go;
        EnemyLauncher launcher;

        if (prefab != null)
        {
            go = Instantiate(prefab, pos, rot);
            launcher = go.GetComponentInChildren<EnemyLauncher>();
            if (launcher == null) launcher = go.AddComponent<EnemyLauncher>();
        }
        else
        {
            go = BuildPlaceholder(cannon, pos, rot, out launcher);
        }

        launcher.target = castle;
        active.Add(launcher);
    }

    GameObject BuildPlaceholder(bool cannon, Vector3 pos, Quaternion rot, out EnemyLauncher launcher)
    {
        float s = placeholderScale;
        var root = new GameObject(cannon ? "Cannon (placeholder)" : "Catapult (placeholder)");
        root.transform.SetPositionAndRotation(pos, rot);

        var baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(baseObj.GetComponent<Collider>());
        baseObj.transform.SetParent(root.transform, false);
        baseObj.transform.localScale = new Vector3(1.6f, 0.8f, 2.4f) * s;
        baseObj.transform.localPosition = Vector3.up * 0.4f * s;
        baseObj.GetComponent<Renderer>().material.color = new Color(0.35f, 0.22f, 0.1f);

        var arm = GameObject.CreatePrimitive(cannon ? PrimitiveType.Cylinder : PrimitiveType.Cube);
        Destroy(arm.GetComponent<Collider>());
        arm.transform.SetParent(root.transform, false);
        arm.transform.localPosition = new Vector3(0f, 1.2f * s, 0.2f * s);
        arm.transform.localRotation = Quaternion.Euler(cannon ? 60f : -50f, 0f, 0f);
        arm.transform.localScale = cannon ? new Vector3(0.7f, 1.2f, 0.7f) * s : new Vector3(0.3f, 0.3f, 2.2f) * s;
        arm.GetComponent<Renderer>().material.color = cannon ? new Color(0.15f, 0.15f, 0.17f) : new Color(0.5f, 0.32f, 0.15f);

        var muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(root.transform, false);
        muzzle.localPosition = new Vector3(0f, 2.2f * s, 0.9f * s);

        launcher = root.AddComponent<EnemyLauncher>();
        launcher.muzzle = muzzle;
        launcher.flightTime = cannon ? 2.2f : 3.8f;
        launcher.damage = cannon ? 4f : 7f;
        launcher.projectileScale = cannon ? 2.5f : 4f;
        launcher.projectileColor = cannon ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.45f, 0.4f, 0.35f);
        return root;
    }
}
