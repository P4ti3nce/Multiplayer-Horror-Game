using UnityEngine;
using TMPro;

public class CodeInputter : MonoBehaviour, IInteractable
{
    [Header("Setup")]
    public TextMeshPro[] digitTexts;       // Assign each 0-9 display
    public Transform[] upButtons;              // Top cubes (increase)
    public Transform[] downButtons;            // Bottom cubes (decrease)

    [Header("Code Settings")]
    public bool randomCode=false;
    public string correctCode = "000";         // Set your correct code here
    public bool codeSolved = false;
    public Door doorToUnlock;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonPressSound;
    public AudioClip successSound;
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);


    private int[] currentDigits;

    private void Awake()
    {
        if(randomCode){ correctCode = Random.Range(000, 999).ToString("000"); }
    }

    private void Start()
    {
        currentDigits = new int[digitTexts.Length];

        currentDigits = new int[] { 0, 0, 0 };

        // Initialize display
        UpdateDisplay();
    }

    public void Interact()
    {
        if (codeSolved) return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        float closestDistance = Mathf.Infinity;
        int closestIndex = -1;
        int direction = 0; // 1 for up, -1 for down

        for (int i = 0; i < digitTexts.Length; i++)
        {
            float upDist = DistanceFromRay(ray, upButtons[i].position);
            if (upDist < closestDistance)
            {
                closestDistance = upDist;
                closestIndex = i;
                direction = 1;
            }

            float downDist = DistanceFromRay(ray, downButtons[i].position);
            if (downDist < closestDistance)
            {
                closestDistance = downDist;
                closestIndex = i;
                direction = -1;
            }
        }

        if (closestIndex != -1)
        {
            ChangeDigit(closestIndex, direction);
        }
    }

    private float DistanceFromRay(Ray ray, Vector3 point)
    {
        // Get closest point on ray to the target point
        Vector3 projected = Vector3.Project(point - ray.origin, ray.direction.normalized);
        Vector3 closestPoint = ray.origin + projected;
        return Vector3.Distance(point, closestPoint);
    }

    private void ChangeDigit(int index, int delta)
    {
        currentDigits[index] = (currentDigits[index] + delta + 10) % 10;
        UpdateDisplay();
        PlayButtonSound();
        CheckCode();
    }

    private void PlayButtonSound()
    {
        if (audioSource != null && buttonPressSound != null)
        {
            audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
            audioSource.PlayOneShot(buttonPressSound);
        }
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < digitTexts.Length; i++)
        {
            digitTexts[i].text = currentDigits[i].ToString();
        }
    }

    private void UpdateDisplayColor(Color color)
    {
        for (int i = 0; i < digitTexts.Length; i++)
        {
            digitTexts[i].color = color;
        }
    }

    private void CheckCode()
    {
        string entered = string.Join("", currentDigits);
        if (entered == correctCode)
        {
            codeSolved = true;
            Debug.Log("Correct code entered!");
            UpdateDisplayColor(Color.green);

            if (audioSource != null && successSound != null)
            {
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(successSound);
            }

            // You can replace this with whatever you want to trigger
            FindFirstObjectByType<MissionUIManager>().ShowMission("Success", new[] { "- Code accepted", "- Continue exploring" });

            if (doorToUnlock != null)
            {
                doorToUnlock.isLocked = false;
                Debug.Log("Door unlocked!");
            }
        }
    }
}
