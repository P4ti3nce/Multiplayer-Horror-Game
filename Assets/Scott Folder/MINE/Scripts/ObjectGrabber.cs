using KinematicCharacterController.Examples;
using UnityEngine;
using UnityEngine.UI;

public class ObjectGrabber : MonoBehaviour
{
    public float grabDistance = 3f;
    public float moveSmoothness = 10f;
    public Transform holdPoint;
    public Camera playerCamera;
    public GameObject flashlight;
    public float throwForce = 10f;

    private Rigidbody grabbedObject;

    private GameObject outlinedObject;
    private Renderer outlinedRenderer;
    private Material outlinedOriginalMaterial;
    public Material outlineMaterial;

    public float rotationSpeed = 5f;
    public GameObject player;

    // Flashlight behavior
    private Light flashlightLight;
    private float startIntensity = 1f;
    private float currentIntensity = 1f;
    public bool flashlightOn = true;
    public float drainRate = 0.2f; // Intensity per second
    public Slider flashSlider;

    void Start()
    {
        if (flashlight != null)
        {
            flashlightLight = flashlight.GetComponent<Light>();
            if (flashlightLight != null)
            {
                if (flashlightOn)
                {
                    startIntensity = flashlightLight.intensity;
                    currentIntensity = startIntensity;
                    flashlightLight.enabled = true;
                    flashSlider.gameObject.SetActive(true);
                }
                else
                {
                    startIntensity = flashlightLight.intensity;
                    currentIntensity = startIntensity;
                    flashlightLight.enabled = false;
                    flashSlider.gameObject.SetActive(false);
                }
            }
        }

        if (flashSlider != null)
        {
            flashSlider.maxValue = startIntensity;
            flashSlider.value = startIntensity;
        }
    }

    void Update()
    {
        HandleOutlineHighlight();

        HandleInput();

        HandleFlashlightDrain();
    }

    void FixedUpdate()
    {
        if (grabbedObject != null)
        {
            Vector3 direction = (holdPoint.position - grabbedObject.position);
            grabbedObject.AddForce(direction * moveSmoothness, ForceMode.Acceleration);
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlash();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (grabbedObject == null)
                TryGrabObject();
            else
                DropObject();
        }

        if (grabbedObject != null)
        {
            ExampleCharacterCamera cam = playerCamera.GetComponent<ExampleCharacterCamera>();

            if (Input.GetMouseButton(1)) // right-click
            {
                if (cam.RotationSpeed != 0) { cam.RotationSpeed = 0; cam.MinDistance = -0.5f; }
                RotateHeldObject();
            }
            else
            {
                if (cam.RotationSpeed != 10) { cam.RotationSpeed = 10; cam.MinDistance = 0; }
            }

            if (Input.GetMouseButtonDown(0)) // left-click
            {
                ThrowObject();
            }
        }
    }

    void ToggleFlash()
    {
        if (flashlightLight == null) return;

        if (flashlightOn)
        {
            if (currentIntensity <= 0f)
            {
                // Recharge the flashlight
                currentIntensity = startIntensity;
                flashlightLight.intensity = currentIntensity;
                flashlightLight.enabled = true;
                flashSlider.gameObject.SetActive(true);
            }
            else
            {
                currentIntensity = 0;
                flashlightLight.intensity = currentIntensity;
                flashlightOn = false;
                flashSlider.value = flashSlider.maxValue;
                flashSlider.gameObject.SetActive(false);
            }
        }
        else
        {
            // Turning it back on from "off" state
            flashlightOn = true;
            currentIntensity = startIntensity;
            flashlightLight.intensity = currentIntensity;
            flashlightLight.enabled = true;
            flashSlider.gameObject.SetActive(true);
        }
    }

    void HandleFlashlightDrain()
    {
        if (flashlightOn && flashlightLight != null && currentIntensity > 0f)
        {
            currentIntensity -= drainRate * Time.deltaTime;
            currentIntensity = Mathf.Max(currentIntensity, 0f);
            flashlightLight.intensity = currentIntensity;

            if (flashSlider != null)
            {
                flashSlider.value = currentIntensity > 0f ? currentIntensity : flashSlider.maxValue;
            }

            if (currentIntensity <= 0f)
            {
                flashlightLight.enabled = false;
                flashlightOn = true; // Stay in on-state but with 0 light
                flashSlider.gameObject.SetActive(false);
            }
        }
    }

    void RotateHeldObject()
    {
        if (grabbedObject.linearVelocity.magnitude > 0.1f)
        {
            grabbedObject.linearVelocity *= 0.9f;
        }
        else
        {
            grabbedObject.linearVelocity = Vector3.zero;
        }

        grabbedObject.freezeRotation = true;

        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        float yaw = mouseDelta.x * rotationSpeed;
        float pitch = -mouseDelta.y * rotationSpeed;

        grabbedObject.transform.Rotate(playerCamera.transform.up, yaw, Space.World);
        grabbedObject.transform.Rotate(playerCamera.transform.right, pitch, Space.World);
    }

    void HandleOutlineHighlight()
    {
        if (grabbedObject != null)
        {
            ClearOutline();
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.CompareTag("grab"))
            {
                if (hitObject == outlinedObject)
                {
                    return;
                }
                else
                {
                    ClearOutline();
                }

                Renderer rend = hitObject.GetComponent<Renderer>();
                if (rend != null)
                {
                    outlinedObject = hitObject;
                    outlinedRenderer = rend;

                    outlinedOriginalMaterial = rend.sharedMaterials[0];

                    Material[] newMats = new Material[2];
                    newMats[0] = outlinedOriginalMaterial;
                    newMats[1] = outlineMaterial;
                    rend.materials = newMats;
                }

                return;
            }

            ClearOutline();
        }

        ClearOutline();
    }

    void ClearOutline()
    {
        if (outlinedObject != null && outlinedRenderer != null && outlinedOriginalMaterial != null)
        {
            outlinedRenderer.materials = new Material[] { outlinedRenderer.materials[0] };
        }

        outlinedObject = null;
        outlinedRenderer = null;
        outlinedOriginalMaterial = null;
    }

    void TryGrabObject()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance))
        {
            if (hit.collider.CompareTag("grab"))
            {
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    grabbedObject = rb;
                    grabbedObject.useGravity = false;
                    grabbedObject.linearDamping = 10f;
                }
            }
            else if (hit.collider.CompareTag("interactable"))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }

    void DropObject()
    {
        if (grabbedObject != null)
        {
            grabbedObject.useGravity = true;
            grabbedObject.linearDamping = 0f;
            grabbedObject = null;
        }
    }

    void ThrowObject()
    {
        if (grabbedObject != null)
        {
            grabbedObject.useGravity = true;
            grabbedObject.linearDamping = 0f;
            grabbedObject.linearVelocity = playerCamera.transform.forward * throwForce;
            grabbedObject = null;
        }
    }
}
