using System.Collections;
using UnityEngine;

public class KeyForDoor : MonoBehaviour, IInteractable
{
    public Door doorToUnlock;
    private bool interacted = false;

    public void Interact()
    {
        if (interacted) return;
        interacted = true;

        if (doorToUnlock != null)
        {
            doorToUnlock.isLocked = false;
            Debug.Log("Door unlocked!");
        }
        else
        {
            Debug.LogWarning("No door assigned to this key!");
        }

        FindFirstObjectByType<MissionUIManager>().ShowMission(
            "Get out",
            new[] { "- Get to the door" }
        );

        DisolveController dc = GetComponent<DisolveController>();
        if (dc != null)
        {
            dc.TriggerDisolve();
            Destroy(gameObject, dc.disolveSpeed);
        }
    }
}