using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public Transform rotationPoint;       // The pivot point to rotate around
    public float openAngle = 90f;         // How far the door opens
    public float rotationSpeed = 2f;      // Speed of rotation
    public bool clockwise = true;         // Direction of opening
    public bool isLocked = false;         // Whether the door is locked
    private bool isLockedCheck = false;
    private bool hasBeenUnlocked = false;

    public AudioSource audioSource;       // Audio source for playing sounds
    public AudioClip openDoorSound;       // Sound when door opens
    public AudioClip closeDoorSound;      // Sound when door closes
    public AudioClip lockedDoorSound;      // Sound when door locked
    public AudioClip unlockingDoorSound;
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f); // Random pitch range

    private bool isOpen = false;          // Current door state
    private bool isMoving = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    public bool triggerUI;
    public string misiontitle;
    public string[] missionObj;
    private bool firstOpen = true;

    void Start()
    {
        closedRotation = transform.rotation;

        Vector3 axis = rotationPoint.up;
        float direction = clockwise ? 1f : -1f;

        transform.RotateAround(rotationPoint.position, axis, openAngle * direction);
        openRotation = transform.rotation;

        transform.rotation = closedRotation;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        isLockedCheck = isLocked;
    }

    void Update()
    {
        if (isLockedCheck && !isLocked && !hasBeenUnlocked)
        {
            hasBeenUnlocked = true;
            if (audioSource != null && unlockingDoorSound != null)
            {
                audioSource.pitch = UnityEngine.Random.Range(pitchRange.x, pitchRange.y);
                audioSource.PlayOneShot(unlockingDoorSound);
            }
        }
    }

    public void Interact()
    {
        ToggleDoor();
    }

    public void ToggleDoor()
    {
        if (isLocked)
        {
            if (audioSource == null) return;
            audioSource.pitch = UnityEngine.Random.Range(pitchRange.x, pitchRange.y);
            audioSource.clip = lockedDoorSound;
            audioSource.Play();

            return;
        }

        if (isMoving) return;

        isOpen = !isOpen;
        StopAllCoroutines();
        PlayDoorSound(isOpen);
        StartCoroutine(RotateToTarget(isOpen ? openRotation : closedRotation));

        if (firstOpen && triggerUI)
        {
            firstOpen = false;
            FindFirstObjectByType<MissionUIManager>().ShowMission(
                misiontitle,
                new[] { missionObj[0], missionObj[1], missionObj[2] }
            );
        }
    }

    public void ForceClose()
    {
        if (isMoving) StopAllCoroutines();
        isOpen = false;
        PlayDoorSound(false);
        StartCoroutine(RotateToTarget(closedRotation));
    }

    private void PlayDoorSound(bool opening)
    {
        if (audioSource == null) return;
        audioSource.pitch = UnityEngine.Random.Range(pitchRange.x, pitchRange.y);
        audioSource.clip = opening ? openDoorSound : closeDoorSound;
        audioSource.Play();
    }

    private IEnumerator RotateToTarget(Quaternion targetRot)
    {
        isMoving = true;
        while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            yield return null;
        }
        transform.rotation = targetRot;
        isMoving = false;
    }
}
