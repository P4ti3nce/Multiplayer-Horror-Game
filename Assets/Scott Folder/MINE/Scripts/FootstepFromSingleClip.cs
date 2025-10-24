using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepFromSingleClip : MonoBehaviour
{
    public AudioClip longFootstepClip;
    public int footstepCount = 30;
    public float clipLength = 52f;

    private float[] stepTimestamps;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;

        float interval = clipLength / footstepCount;

        stepTimestamps = new float[footstepCount];
        for (int i = 0; i < footstepCount; i++)
        {
            stepTimestamps[i] = i * interval;
        }
    }

    public void PlayRandomFootstep()
    {
        if (!longFootstepClip) return;

        float start = stepTimestamps[Random.Range(0, stepTimestamps.Length)];

        audioSource.clip = longFootstepClip;
        audioSource.time = start;
        audioSource.Play();
        StartCoroutine(StopAfterInterval(start));
    }

    private System.Collections.IEnumerator StopAfterInterval(float startTime)
    {
        // Stop after ~interval
        float stepLength = clipLength / footstepCount;
        yield return new WaitForSeconds(stepLength);
        audioSource.Stop();
    }
}
