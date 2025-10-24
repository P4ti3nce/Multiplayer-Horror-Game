using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DisolveController : MonoBehaviour
{
    [Tooltip("Material must use the 'DisolveTexture' shader")]
    public bool animateIn = true;
    public float disolveSpeed = 1.5f;

    private Material disolveMat;
    private Coroutine disolveRoutine;

    private static readonly int UseTimeID = Shader.PropertyToID("_UseTime");
    private static readonly int DisolveAmountID = Shader.PropertyToID("_DisolveAmount");

    void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();

        if (renderer == null || renderer.sharedMaterial == null)
        {
            Debug.LogWarning($"[{name}] No valid material found.");
            return;
        }

        // Instantiate material to avoid editing shared material
        disolveMat = renderer.material;

        if (disolveMat.shader.name != "Shader Graphs/DisolveColor")
        {
            Debug.LogWarning($"[{name}] Material does not use the 'DisolveTexture' shader.");
            disolveMat = null;
        }
        
        disolveMat.SetFloat(UseTimeID, 0f);

        if (!animateIn) { disolveMat.SetFloat(DisolveAmountID, 1f); } else { disolveMat.SetFloat(DisolveAmountID, 0f); }
    }

    /// <summary>
    /// Call to animate dissolve in or out
    /// </summary>
    public void TriggerDisolve()
    {
        if (disolveMat == null) return;

        // Stop any previous coroutine
        if (disolveRoutine != null)
            StopCoroutine(disolveRoutine);

        disolveRoutine = StartCoroutine(AnimateDisolve(animateIn));
    }

    private System.Collections.IEnumerator AnimateDisolve(bool fadeIn)
    {
        // Disable time-based animation
        disolveMat.SetFloat(UseTimeID, 0f);

        float start = fadeIn ? -1f : 1f;
        float end = fadeIn ? 1f : -1f;
        float duration = Mathf.Abs(end - start) / disolveSpeed;

        float t = 0f;

        while (t < duration)
        {
            float lerp = Mathf.Lerp(start, end, t / duration);
            disolveMat.SetFloat(DisolveAmountID, lerp);
            t += Time.deltaTime;
            yield return null;
        }

        disolveMat.SetFloat(DisolveAmountID, end);
    }
}
