using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    [Tooltip("Minimum light intensity")]
    public float minIntensity = 0.5f;

    [Tooltip("Maximum light intensity")]
    public float maxIntensity = 1.5f;

    [Tooltip("How fast the flicker changes")]
    public float flickerSpeed = 0.1f;

    [Tooltip("How random the flicker is (0 = smooth, 1 = very jittery)")]
    [Range(0, 1)] public float randomness = 0.5f;

    private Light lightSource;
    private float baseIntensity;
    private float targetIntensity;
    private float timer;

    void Start()
    {
        lightSource = GetComponent<Light>();
        baseIntensity = lightSource.intensity;
        targetIntensity = Random.Range(minIntensity, maxIntensity);
        timer = flickerSpeed;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            // Choose a new target intensity randomly between min and max
            targetIntensity = Random.Range(minIntensity, maxIntensity);

            // Reset the timer with slight random variation
            timer = flickerSpeed * Random.Range(1f - randomness, 1f + randomness);
        }

        // Smoothly interpolate toward the target intensity
        lightSource.intensity = Mathf.Lerp(lightSource.intensity, targetIntensity, Time.deltaTime * 10f);
    }
}
