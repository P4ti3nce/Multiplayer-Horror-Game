using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhoneBooth : MonoBehaviour, IInteractable
{
    [Header("Audio")]
    public AudioSource ringingAudio;

    [Header("UI")]
    public CanvasGroup canvasGroup;
    [TextArea]
    public string messageText;
    public float fadeDuration = 1.5f;
    public float messageDuration = 5f;
    public float typeSpeed = 0.05f;

    private bool isRinging = false;
    private bool isInSequence = false;
    private TextMeshProUGUI messageTextUI;

    private void Awake()
    {
        if (canvasGroup != null)
        {
            messageTextUI = canvasGroup.GetComponentInChildren<TextMeshProUGUI>(true);
            if (messageTextUI == null)
            {
                Debug.LogWarning("No TextMeshProUGUI found under the CanvasGroup!");
            }
        }
        else
        {
            Debug.LogWarning("CanvasGroup not assigned on PhoneBooth.");
        }
    }

    public void TriggerPhoneCall(bool ring)
    {
        if (isRinging || isInSequence) return;

        isRinging = true;

        if (ringingAudio != null && ring)
        {
            ringingAudio.loop = true;
            ringingAudio.Play();
        }
    }

    public void Interact()
    {
        if (!isRinging || isInSequence) return;

        isRinging = false;
        isInSequence = true;

        if (ringingAudio != null)
        {
            ringingAudio.Stop();
        }

        StartCoroutine(ShowMessageSequence());
    }

    private IEnumerator ShowMessageSequence()
    {
        if (messageTextUI != null) messageTextUI.text = "";

        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));

        if (messageTextUI != null)
            yield return StartCoroutine(TypeText(messageText));

        yield return new WaitForSeconds(messageDuration);

        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

        isInSequence = false; // Allow it to be triggered again
        TriggerPhoneCall(false);
    }

    private IEnumerator FadeCanvas(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = end;
    }

    private IEnumerator TypeText(string text)
    {
        messageTextUI.text = "";
        foreach (char c in text)
        {
            messageTextUI.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
    }
}
