using System;
using UnityEngine;

/// Powieś na obiekcie zamku (zamok). Zamek musi mieć Collidery (na nim lub dzieciach).
public class CastleHealth : MonoBehaviour
{
    public float maxHealth = 100f;

    public float Current { get; private set; }
    public Vector3 Center { get; private set; }
    public float Radius { get; private set; }

    public event Action<float, float> OnHealthChanged;
    public event Action OnDestroyed;

    void Awake()
    {
        Current = maxHealth;

        var renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            Center = b.center;
            Radius = b.extents.magnitude;
        }
        else
        {
            Center = transform.position;
            Radius = 10f;
        }
    }

    void Start()
    {
        OnHealthChanged?.Invoke(Current, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (Current <= 0f) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

        Current = Mathf.Max(0f, Current - amount);
        OnHealthChanged?.Invoke(Current, maxHealth);
        if (Current <= 0f) OnDestroyed?.Invoke();
    }
}
