using System.Collections;
using UnityEngine;

public class DoorTriggerZone : MonoBehaviour
{
    [Header("Target Door")]
    public Door targetDoor;
    public bool lockDoorOnEnter = false;

    [Header("Optional Mission")]
    public bool triggerMission = false;
    public string missionTitle;
    public Color titleColor;

    public string[] missionObjectives = new string[3];

    private bool triggered = false;

    [Header("Optional Music/Audio")]
    public AudioSource audioSource;
    public bool playOnEnter = false;

    [Header("Optional Hint")]
    public PhoneBooth phoneBooth;
    public float delay = 0;
    public bool playHint = false;

    [Header("Optional DespawnRooms")]
    public bool despawn = false;
    public GameObject roomsToDespawn;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        if (audioSource != null)
        {
            audioSource.Play();
        }

        triggered = true;

        if (targetDoor != null && lockDoorOnEnter)
        {
            ForceCloseAndLockDoor(targetDoor);
        }

        if (triggerMission && missionObjectives.Length >= 3)
        {
            MissionUIManager missionUI = FindFirstObjectByType<MissionUIManager>();
            if (missionUI != null)
            {
                missionUI.missionTitleText.color = titleColor;
                missionUI.ShowMission(missionTitle, new[] {
                    missionObjectives[0],
                    missionObjectives[1],
                    missionObjectives[2]
                });
            }
        }

        if (playHint) { StartCoroutine(StartHint()); }
        
        if(despawn) {roomsToDespawn.SetActive(false); }
    }

    private void ForceCloseAndLockDoor(Door door)
    {
        // Stop any door motion
        door.StopAllCoroutines();

        // Begin smooth closing to ensure the door finishes correctly
        door.ForceClose();
        door.isLocked = true;
    }

    private void Reset()
    {
        if (TryGetComponent<BoxCollider>(out var col))
        {
            col.isTrigger = true;
        }
        else
        {
            gameObject.AddComponent<BoxCollider>().isTrigger = true;
        }
    }

    private IEnumerator StartHint()
    {
        yield return new WaitForSeconds(delay);
        phoneBooth.TriggerPhoneCall(true);
    }
}
