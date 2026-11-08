using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light sun;                 // opcjonalne
    public Material sky;
    public float dayLength = 120f;
    [Range(0, 1)] public float t = 0.3f;

    [Header("Moon")]
    public int moonCycleDays = 8;     // pełnia -> 3/4 -> 1/2 -> 1/4 -> nów -> 1/4 -> ... -> pełnia
    public bool smoothPhase = false;  // true = faza zmienia się płynnie, false = skokowo co dzień
    public int dayCount = 0;

    void Update()
    {
        t += Time.deltaTime / dayLength;
        if (t >= 1f) { t -= 1f; dayCount++; }

        Quaternion rot = Quaternion.Euler(t * 360f - 90f, 170f, 0f);
        Vector3 toSun = rot * Vector3.back;

        if (sun != null)
        {
            sun.transform.rotation = rot;
        }

        float day = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-0.15f, 0.25f, toSun.y));
        if (sun != null) sun.intensity = day;

        float phase = smoothPhase ? (dayCount + t) / moonCycleDays
                                  : dayCount / (float)moonCycleDays;
        phase = Mathf.Repeat(phase, 1f);

        sky.SetVector("_SunDir", toSun);
        sky.SetFloat("_DayFactor", day);
        sky.SetFloat("_MoonPhase", phase);
    }
}