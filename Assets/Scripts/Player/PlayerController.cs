using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using TripleDrop.PowerUps;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerPowerUpController))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 0.1f;
    public Transform cameraTransform;

    [Header("Throw Settings")]
    public float maxChargePower = 100f;
    public float chargeSpeed = 30f;

    [Header("Audio Settings")]
    public AudioClip footstepClip;
    public float walkStepInterval = 0.5f;
    public float sprintStepInterval = 0.35f;
    public float stepMinPitch = 0.85f;
    public float stepMaxPitch = 1.15f;

    private CharacterController characterController;
    private PlayerInputHandler inputHandler;
    private PlayerPowerUpController powerUpController;
    private Vector3 velocity;
    private float cameraPitch = 0f;
    private float targetCameraPitch = 0f;

    private AudioSource audioSource;
    private Vector3 lastPosition;
    private float stepTimer;

    public bool hasBall = false;
    public float chargePower = 0f;
    public bool isCharging = false;

    public static PlayerController Local;
    public bool InputLocked = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        powerUpController = GetComponent<PlayerPowerUpController>();
        audioSource = GetComponent<AudioSource>();

        inputHandler.OnJumpEvent += OnJump;
        inputHandler.OnThrowStartedEvent += OnThrowStarted;
        inputHandler.OnThrowCanceledEvent += OnThrowCanceled;
        inputHandler.OnEscapeEvent += OnEscape;
    }

    private void Start()
    {
        lastPosition = transform.position;
        if (!photonView.IsMine)
        {
            if (cameraTransform != null)
            {
                Camera cam = cameraTransform.GetComponent<Camera>();
                if (cam != null) cam.enabled = false;

                AudioListener audioListener = cameraTransform.GetComponent<AudioListener>();
                if (audioListener != null) audioListener.enabled = false;
            }
        }
        else
        {
            Local = this;
            inputHandler.EnableInputs();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        if (InputLocked) return;

        HandleMovement();
        UpdateFootsteps();
        HandleLook();

        if (isCharging && hasBall)
        {
            chargePower += Time.deltaTime * chargeSpeed;
            chargePower = Mathf.Clamp(chargePower, 0f, maxChargePower);
        }
    }

    private void UpdateFootsteps()
    {
        Vector3 currentPos = transform.position;
        Vector3 horizontalMovement = currentPos - lastPosition;
        horizontalMovement.y = 0f;
        float speed = horizontalMovement.magnitude / Time.deltaTime;
        lastPosition = currentPos;

        bool isPlayerGrounded = false;
        if (photonView.IsMine)
        {
            isPlayerGrounded = characterController.isGrounded;
        }
        else
        {
            isPlayerGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f);
        }

        if (isPlayerGrounded && speed > 0.2f)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                bool isSprinting = false;
                if (photonView.IsMine)
                {
                    isSprinting = inputHandler.IsSprinting;
                }
                else
                {
                    isSprinting = speed > moveSpeed * 1.1f;
                }

                float interval = isSprinting ? sprintStepInterval : walkStepInterval;
                stepTimer = interval;

                if (audioSource != null && footstepClip != null)
                {
                    SoundManager.Instance.PlaySFXOnSource(audioSource, footstepClip, 1f, stepMinPitch, stepMaxPitch);
                }
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void HandleMovement()
    {
        bool isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (hasBall)
        {
            velocity.y += (gravity * powerUpController.GravityScale) * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
            return;
        }

        float currentSpeed = moveSpeed * powerUpController.SpeedMultiplier;
        if (inputHandler.IsSprinting)
        {
            currentSpeed *= sprintMultiplier;
        }

        Vector2 rawMoveInput = inputHandler.MoveInput;
        if (powerUpController.IsPlatformPanicActive)
        {
            rawMoveInput *= -1f;
        }

        Vector3 move = transform.right * rawMoveInput.x + transform.forward * rawMoveInput.y;
        characterController.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += (gravity * powerUpController.GravityScale) * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        float mouseX = inputHandler.LookInput.x * mouseSensitivity;
        float mouseY = inputHandler.LookInput.y * mouseSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    private void OnJump()
    {
        if (characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt((jumpForce * powerUpController.JumpMultiplier) * -2f * gravity);
        }
    }

    private void OnThrowStarted()
    {
        if (!hasBall) return;
        isCharging = true;
        chargePower = 0f;
    }

    private void OnThrowCanceled()
    {
        if (!hasBall || !isCharging) return;
        isCharging = false;

        ThrowBall(chargePower);

        chargePower = 0f;
        hasBall = false;
    }

    private void ThrowBall(float power)
    {
        if (heldBall != null && photonView.IsMine)
        {
            Vector3 throwDir = cameraTransform.forward;
            float force = Mathf.Lerp(5f, 25f, power / maxChargePower) * powerUpController.ThrowPowerMultiplier;

            heldBall.photonView.RPC("RPC_ThrowBall", RpcTarget.All,
                throwDir * force,
                photonView.Owner.ActorNumber,
                powerUpController.BallMassMultiplier,
                powerUpController.IsFireBallActive);

            if (powerUpController.IsFireBallActive)
            {
                powerUpController.ConsumeFireBall();
            }

            heldBall = null;
        }

        Debug.Log($"Threw ball with power: {power}");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(cameraPitch);
        }
        else
        {
            float receivedPitch = (float)stream.ReceiveNext();
            if (cameraTransform != null && !photonView.IsMine)
            {
                targetCameraPitch = receivedPitch;
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!photonView.IsMine || hasBall) return;

        BallController ball = hit.collider.GetComponent<BallController>();
        if (ball != null)
        {
            ball.TryPickup(this);
        }
    }

    [HideInInspector]
    public BallController heldBall;

    private void OnEscape()
    {
        if (photonView.IsMine)
        {
            NetworkManager.Instance.ReturnToMainMenu();
        }
    }




}