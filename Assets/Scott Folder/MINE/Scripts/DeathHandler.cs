using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KinematicCharacterController.Examples;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class DeathHandler : MonoBehaviour
{
    [Header("Assign the root CanvasGroup here")]
    public CanvasGroup deathCanvasGroup;
    public ExamplePlayer exampleplayer;

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    private bool hasDied = false;

    // Store all UI Graphic elements to fade
    private List<Graphic> uiGraphics = new List<Graphic>();
    private float originalAudioVolume;

    void Start()
    {
        if (deathCanvasGroup == null)
        {
            Debug.LogError("DeathHandler: CanvasGroup not assigned!");
            return;
        }

        // Get all Graphic components (TextMeshProUGUI, Image, Button text, etc.)
        uiGraphics.AddRange(deathCanvasGroup.GetComponentsInChildren<Graphic>(true));

        // Set initial alpha to 0 for all UI
        foreach (var graphic in uiGraphics)
        {
            SetAlpha(graphic, 0f);
        }

        // Initialize CanvasGroup
        deathCanvasGroup.alpha = 0f;
        deathCanvasGroup.interactable = false;
        deathCanvasGroup.blocksRaycasts = false;

        // Save original volume
        originalAudioVolume = AudioListener.volume;
    }

    public void TriggerDeath()
    {
        if (hasDied) return;

        hasDied = true;
        StartCoroutine(HandleDeath());
    }

    private IEnumerator HandleDeath()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            float t = timer / fadeDuration;

            // Fade in CanvasGroup and all UI elements
            deathCanvasGroup.alpha = t;
            foreach (var graphic in uiGraphics)
            {
                if (graphic != null)
                    SetAlpha(graphic, t);
            }

            // Fade out global audio
            AudioListener.volume = Mathf.Lerp(originalAudioVolume, 0f, t);

            timer += Time.unscaledDeltaTime; // ignore time scale
            yield return null;
        }

        // Final state
        deathCanvasGroup.alpha = 1f;
        foreach (var graphic in uiGraphics)
        {
            if (graphic != null)
                SetAlpha(graphic, 1f);
        }

        AudioListener.volume = 0f;

        // Freeze time
        Time.timeScale = 0f;

        // Enable UI interactions
        deathCanvasGroup.interactable = true;
        deathCanvasGroup.blocksRaycasts = true;

        // Show and unlock cursor
        exampleplayer.dead = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void SetAlpha(Graphic graphic, float alpha)
    {
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }

    public void Restart()
    {
        SceneLoadFader slf = FindFirstObjectByType<SceneLoadFader>();
        if (slf != null)
        {
            Time.timeScale = 1f;
            slf.StartCoroutine(slf.FadeInCanvas());
            StartCoroutine(HandleRestart(slf.fadeDuration));
        }
    }

    private IEnumerator HandleRestart(float duration)
    {
        yield return new WaitForSeconds(duration);
        Scene currentScene = SceneManager.GetActiveScene();
        AudioListener.volume = originalAudioVolume;
        SceneManager.LoadScene(currentScene.name, LoadSceneMode.Single);
    }
}
