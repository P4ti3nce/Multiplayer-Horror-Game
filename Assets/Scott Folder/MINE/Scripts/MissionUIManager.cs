using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.GameCenter;

public class MissionUIManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform missionTitleTransform;
    public TMP_Text missionTitleText;
    public Vector3 titleOffset;
    public Vector3 objOffset;
    public RectTransform topLeftAnchor;
    public RectTransform objectiveContainer;
    public TMP_Text objectiveTextPrefab;

    [Header("Animation Settings")]
    public float typingSpeed = 0.05f;
    public float moveDuration = 1f;
    public float fadeDuration = 1f;

    private Vector3 originalPosition;
    private Vector3 originalScale;

    void Awake()
    {
        originalPosition = missionTitleTransform.position;
        originalScale = missionTitleTransform.localScale;

        missionTitleText.text = "";
        ClearObjectives();
    }

    void Start()
    {
        
    }

    public void ShowMission(string title, string[] objectives)
    {
        StopAllCoroutines();
        missionTitleTransform.position = originalPosition;
        missionTitleTransform.localScale = originalScale;
        originalPosition = missionTitleTransform.position;
        originalScale = missionTitleTransform.localScale;
        missionTitleText.text = "";
        missionTitleText.alignment = TextAlignmentOptions.Left;
        ClearObjectives();

        StartCoroutine(AnimateMissionSequence(title, objectives));
    }

    private IEnumerator AnimateMissionSequence(string title, string[] objectives)
    {
        // Type the mission title in the center
        yield return StartCoroutine(TypeText(missionTitleText, title, typingSpeed));

        yield return new WaitForSeconds(0.5f);

        // Animate move and shrink to top-left
        yield return StartCoroutine(MoveAndScale(
            missionTitleTransform, 
            topLeftAnchor.position+titleOffset,
            originalScale, 
            moveDuration,
            missionTitleText
        ));

        // Fade in each objective
        int count = 0;
        foreach (string obj in objectives)
        {
            TMP_Text objText = Instantiate(objectiveTextPrefab, objectiveContainer);
            objText.transform.position=missionTitleTransform.position+objOffset;
            objText.transform.position+=new Vector3(-150,-50-(30*count),0);
            objText.text = obj;
            CanvasGroup cg = objText.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            yield return StartCoroutine(FadeIn(cg, fadeDuration));
            yield return new WaitForSeconds(0.2f);
            count++;
        }
    }

    private IEnumerator TypeText(TMP_Text textMesh, string fullText, float delay)
    {
        textMesh.alignment = TextAlignmentOptions.Left;
        textMesh.text = "";
    
        RectTransform rect = textMesh.GetComponent<RectTransform>();
        rect.localScale = Vector3.one * 2f;
    
        // Set full text to get width
        textMesh.text = fullText;
        textMesh.ForceMeshUpdate();
        float width = textMesh.preferredWidth;
    
        // Center using anchoredPosition
        rect.anchoredPosition = new Vector2(-width / 2f, 0f); // Assuming vertical center is already fine
    
        // Type out
        textMesh.text = "";
        foreach (char c in fullText)
        {
            textMesh.text += c;
            yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator MoveAndScale(RectTransform target, Vector3 endPos, Vector3 endScale, float duration, TMP_Text tmpText)
    {
        Vector3 startPos = target.position;
        Vector3 startScale = target.localScale;
        float elapsed = 0f;
        bool alignmentChanged = false;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            target.position = Vector3.Lerp(startPos, endPos, t);
            target.localScale = Vector3.Lerp(startScale, endScale, t);

            // Midway alignment switch
            if (!alignmentChanged && elapsed >= duration / 2f)
            {
                tmpText.alignment = TextAlignmentOptions.Left;
                alignmentChanged = true;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.position = endPos;
        target.localScale = endScale;
    }

    private IEnumerator FadeIn(CanvasGroup cg, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            cg.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 1f;
    }

    private void ClearObjectives()
    {
        foreach (Transform child in objectiveContainer)
            Destroy(child.gameObject);
    }
}
