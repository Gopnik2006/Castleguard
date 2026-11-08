using UnityEngine;

/// Powieś na obiekcie Shield (dziecko XR Controller Left).
/// Tarcza musi mieć Collider (NIE trigger). Rigidbody (kinematic) dodaje się automatycznie.
[RequireComponent(typeof(Rigidbody))]
public class ShieldBlocker : MonoBehaviour
{
    void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }
}
