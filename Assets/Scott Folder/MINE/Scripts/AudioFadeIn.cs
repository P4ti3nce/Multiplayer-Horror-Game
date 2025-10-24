using System.Collections;
using UnityEngine;

public class AudioFadeIn : MonoBehaviour
{
    [Tooltip("Time it takes to fade audio in (seconds)")]
    public float fadeDuration = 2f;

    private AudioSource[] allAudioSources;
    private float[] originalVolumes;

    void Start()
    {
        allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        originalVolumes = new float[allAudioSources.Length];

        // Store original volumes and set all sources to volume 0
        for (int i = 0; i < allAudioSources.Length; i++)
        {
            if (allAudioSources[i] != null)
            {
                originalVolumes[i] = allAudioSources[i].volume;
                allAudioSources[i].volume = 0f;
            }
        }

        StartCoroutine(FadeInAudio());
    }

    IEnumerator FadeInAudio()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            float t = timer / fadeDuration;

            for (int i = 0; i < allAudioSources.Length; i++)
            {
                if (allAudioSources[i] != null)
                    allAudioSources[i].volume = Mathf.Lerp(0f, originalVolumes[i], t);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure all final volumes are correct
        for (int i = 0; i < allAudioSources.Length; i++)
        {
            if (allAudioSources[i] != null)
                allAudioSources[i].volume = originalVolumes[i];
        }
    }
}
