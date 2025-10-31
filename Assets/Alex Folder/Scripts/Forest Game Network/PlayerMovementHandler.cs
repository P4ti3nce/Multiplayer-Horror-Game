using UnityEngine;
using Mirror;

namespace SteamLobbyNamespace
{
    [RequireComponent(typeof(NetworkIdentity))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovementHandler : NetworkBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 2.5f;
        [SerializeField] private float runSpeed = 5.5f;
        [SerializeField] private float acceleration = 20f; // speed smoothing

        [Header("Jump / Gravity")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpHeight = 1.4f;
        [SerializeField] private float groundedGravity = -0.5f; // small downward force to keep grounded
        [SerializeField] private float coyoteTime = 0.15f; // allow jump shortly after leaving ground
        [SerializeField] private float jumpBufferTime = 0.12f; // allow jump input shortly before landing

        [Header("Camera")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 75f;

        [Header("Stamina / Sprint")]
        [SerializeField] private float maxStamina = 5f;
        [SerializeField] private float staminaDrainRate = 1.5f;
        [SerializeField] private float staminaRegenRate = 0.9f;
        [SerializeField] private float regenDelay = 1f;

        [Header("Animation (optional)")]
        [SerializeField] private Animator animator;
        [SerializeField] private string animParamSpeed = "Speed";
        [SerializeField] private string animParamRunning = "IsRunning";
        [SerializeField] private string animParamGrounded = "IsGrounded";
        [SerializeField] private string animParamVertical = "VerticalVelocity"; // optional float
        private bool isMoving = false;
        private float animSpeedSmooth = 0f; // optional internal smooth variable

        // internals
        private CharacterController charController;
        private float yaw = 0f;
        private float pitch = 0f;

        private float currentSpeed = 0f;
        private float targetSpeed = 0f;
        private Vector3 velocity = Vector3.zero; // used for vertical velocity

        private float currentStamina;
        private float regenTimer = 0f;
        private bool isSprinting = false;

        // jump helpers
        private float lastGroundedTime = -999f;
        private float lastJumpPressedTime = -999f;

        private void Awake()
        {
            charController = GetComponent<CharacterController>();
            currentStamina = maxStamina;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            // Enable local camera/audio
            if (playerCamera != null)
            {
                playerCamera.enabled = true;
                var al = playerCamera.GetComponent<AudioListener>();
                if (al) al.enabled = true;
            }

            // lock cursor for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // initialize look angles
            yaw = transform.eulerAngles.y;
            if (cameraPivot != null)
            {
                float rawPitch = cameraPivot.localEulerAngles.x;
                if (rawPitch > 180f) rawPitch -= 360f;
                pitch = rawPitch;
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // disable remote players' cameras on this client
            if (!isLocalPlayer && playerCamera != null)
            {
                playerCamera.enabled = false;
                var al = playerCamera.GetComponent<AudioListener>();
                if (al) al.enabled = false;
            }
        }

        private void OnDisable()
        {
            if (isLocalPlayer)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void Update()
        {
            // Local-only input & camera
            if (!isLocalPlayer) return;

            HandleMouseLook();
            ReadInputsAndMove();
            HandleStamina();
            UpdateAnimator();
        }

        // ---------- Input / Movement ----------
        private void HandleMouseLook()
        {
            if (!isLocalPlayer) return;
            float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
            float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

            yaw += mx;
            pitch -= my;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (cameraPivot != null)
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void ReadInputsAndMove()
        {
            if (!isLocalPlayer) return;
            // Read movement axes
            float hx = Input.GetAxisRaw("Horizontal");
            float vz = Input.GetAxisRaw("Vertical");
            isMoving = Mathf.Abs(hx) > 0.1f || Mathf.Abs(vz) > 0.1f;
            Vector3 input = new Vector3(hx, 0f, vz);
            float inputMag = Mathf.Clamp01(input.magnitude);

            // Sprint logic
            bool sprintKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool wantsSprint = sprintKey && vz > 0.1f && currentStamina > 0f; // forward only
            isSprinting = wantsSprint && currentStamina > 0f;
            targetSpeed = isSprinting ? runSpeed : walkSpeed;

            // Smooth speed
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed * inputMag, acceleration * Time.deltaTime);

            // Horizontal movement (relative to player rotation)
            Vector3 move = transform.TransformDirection(input.normalized) * currentSpeed;

            // Jump input buffering
            if (Input.GetButtonDown("Jump"))
                lastJumpPressedTime = Time.time;

            // Grounded check via CharacterController
            bool grounded = charController.isGrounded;
            if (grounded)
                lastGroundedTime = Time.time;

            // Coyote & buffer conditions
            bool canUseCoyote = (Time.time - lastGroundedTime) <= coyoteTime;
            bool bufferedJump = (Time.time - lastJumpPressedTime) <= jumpBufferTime;

            // If grounded, small downward velocity to stick to ground
            if (grounded && velocity.y <= 0f)
            {
                velocity.y = groundedGravity;
            }
            else
            {
                // apply gravity
                velocity.y += gravity * Time.deltaTime;
            }

            // Jump: if we have a buffered jump and we are allowed (coyote or grounded)
            if (bufferedJump && (grounded || canUseCoyote))
            {
                velocity.y = Mathf.Sqrt(2f * -gravity * jumpHeight);
                lastJumpPressedTime = -999f;
                lastGroundedTime = -999f;
                animator?.SetTrigger("Jump");
            }

            // Combine horizontal + vertical
            Vector3 finalMove = move + new Vector3(0f, velocity.y, 0f);

            // Apply movement (CharacterController handles collisions)
            charController.Move(finalMove * Time.deltaTime);
        }

        // ---------- Stamina / Sprint ----------
        private void HandleStamina()
        {
            if (!isLocalPlayer) return;
            if (isSprinting)
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
                regenTimer = 0f;
                if (currentStamina <= 0f)
                {
                    currentStamina = 0f;
                    isSprinting = false;
                }
            }
            else
            {
                regenTimer += Time.deltaTime;
                if (regenTimer >= regenDelay)
                {
                    currentStamina += staminaRegenRate * Time.deltaTime;
                    if (currentStamina > maxStamina) currentStamina = maxStamina;
                }
            }
        }

        // ---------- Animator ----------
        private void UpdateAnimator()
        {
            if (!isLocalPlayer) return;
            if (animator == null) return;
            animator.applyRootMotion = false;
            float targetNormalizedSpeed = Mathf.Clamp01(currentSpeed / runSpeed);

            // Smooth damp it (more stable than direct set)
            animSpeedSmooth = Mathf.Lerp(animSpeedSmooth, targetNormalizedSpeed, Time.deltaTime * 8f);
            Debug.Log(animSpeedSmooth);
            // Update animator parameters
            animator.SetFloat("Speed", animSpeedSmooth);
            animator.SetBool("IsRunning", isSprinting);
            animator.SetBool("IsGrounded", charController.isGrounded);
            animator.SetBool("IsMoving",isMoving);
            // Optional: set vertical velocity for animation blend
            if (!string.IsNullOrEmpty(animParamVertical))
                animator.SetFloat(animParamVertical, velocity.y);
        }

        // ---------- Helpers / Accessors ----------
        public float GetStamina() => currentStamina;
        public float GetMaxStamina() => maxStamina;
        public bool IsSprinting() => isSprinting;
    }
}