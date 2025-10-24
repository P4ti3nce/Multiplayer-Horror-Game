using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneLoadFader : MonoBehaviour
{
    [Header("Assign the root CanvasGroup here")]
    public CanvasGroup loadCanvasGroup;

    [Header("Fade Out Settings")]
    public float fadeDuration = 1f;

    private List<Graphic> uiGraphics = new List<Graphic>();

    void Start()
    {
        if (loadCanvasGroup == null)
        {
            Debug.LogError("SceneLoadFader: CanvasGroup not assigned!");
            return;
        }

        // Gather all UI graphics under the canvas
        uiGraphics.AddRange(loadCanvasGroup.GetComponentsInChildren<Graphic>(true));

        // Start fully visible
        foreach (var graphic in uiGraphics)
        {
            SetAlpha(graphic, 1f);
        }

        loadCanvasGroup.alpha = 1f;
        loadCanvasGroup.interactable = false;
        loadCanvasGroup.blocksRaycasts = false;

        // Begin fade out
        StartCoroutine(FadeOutCanvas());
    }

    public IEnumerator FadeOutCanvas()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            float t = timer / fadeDuration;

            // Fade canvas group and all graphics
            loadCanvasGroup.alpha = 1f - t;

            foreach (var graphic in uiGraphics)
            {
                if (graphic != null)
                    SetAlpha(graphic, 1f - t);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Finalize
        loadCanvasGroup.alpha = 0f;
        foreach (var graphic in uiGraphics)
        {
            if (graphic != null)
                SetAlpha(graphic, 0f);
        }

        FindFirstObjectByType<MissionUIManager>().ShowMission(
            "Where am I?", 
            new[] { "- Look around the room", "- Find a way out" }
        );

        // Optionally disable the canvas object
        //gameObject.SetActive(false);
    }

    public IEnumerator FadeInCanvas()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            float t = timer / fadeDuration;

            loadCanvasGroup.alpha = t;

            foreach (var graphic in uiGraphics)
            {
                if (graphic != null)
                    SetAlpha(graphic, t);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Finalize
        loadCanvasGroup.alpha = 1f;
        foreach (var graphic in uiGraphics)
        {
            if (graphic != null)
                SetAlpha(graphic, 1f);
        }

        FindFirstObjectByType<MissionUIManager>().ShowMission(
            "Where am I?", 
            new[] { "- Look around the room", "- Find a way out" }
        );

        // Optionally disable the canvas object
        //gameObject.SetActive(false);
    }

    private void SetAlpha(Graphic graphic, float alpha)
    {
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }
}
