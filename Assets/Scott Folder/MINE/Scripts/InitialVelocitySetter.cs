using UnityEngine;

public class InitialVelocitySetter : MonoBehaviour
{
    public Vector3 initialVelocity = Vector3.zero;  // editable in Inspector

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = initialVelocity;
        }
    }
}
