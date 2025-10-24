using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMaterialReplacer : MonoBehaviour
{
    public Material targetMaterial;
    public float minDelay = 0.1f;
    public float maxDelay = 1f;

    public AudioClip infectionSound; // 🎵 Assign this in the Inspector

    private List<Renderer> renderers = new List<Renderer>();
    private int curObj = 0;

    void Start()
    {
        renderers.AddRange(GetComponentsInChildren<Renderer>());
        Shuffle(renderers);
        StartCoroutine(ReplaceMaterialsOverTime());
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randIndex = Random.Range(0, i + 1);
            (list[i], list[randIndex]) = (list[randIndex], list[i]);
        }
    }

    IEnumerator ReplaceMaterialsOverTime()
    {
        foreach (Renderer rend in renderers)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            if (rend != null && targetMaterial != null)
            {
                // Change materials
                rend.materials = new Material[] { targetMaterial, targetMaterial, targetMaterial };
                rend.gameObject.tag = "Untagged";

                // Rigidbody logic
                Rigidbody rb = rend.gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = false;
                    rb.mass = 0;
                    rb.linearVelocity = new Vector3(0, 0.25f, 0);
                }

                // Audio logic
                AudioSource audio = rend.gameObject.GetComponent<AudioSource>();
                if (audio == null && infectionSound != null)
                {
                    audio = rend.gameObject.AddComponent<AudioSource>();
                    audio.playOnAwake = true;
                    audio.spatialBlend = 1f; // Optional: make it 3D
                    audio.loop = false;
                    audio.pitch = Random.Range(0.95f, 1.05f);
                    audio.volume = 0.33f;
                    audio.minDistance = 1.25f;
                    audio.clip = infectionSound;
                    audio.Play();
                }
            }

            curObj++;
        }
    }
}
