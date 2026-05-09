using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
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

    private CharacterController characterController;
    private PlayerInputHandler inputHandler;
    private Vector3 velocity;
    private float cameraPitch = 0f;

    public bool hasBall = false;
    public float chargePower = 0f;
    public bool isCharging = false;

    public static PlayerController Local;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();

        inputHandler.OnJumpEvent += OnJump;
        inputHandler.OnThrowStartedEvent += OnThrowStarted;
        inputHandler.OnThrowCanceledEvent += OnThrowCanceled;
    }

    private void Start()
    {
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

        HandleMovement();
        HandleLook();

        if (hasBall && isCharging)
        {
            chargePower += Time.deltaTime * chargeSpeed;
            chargePower = Mathf.Clamp(chargePower, 0f, maxChargePower);
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
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
            return;
        }

        float currentSpeed = moveSpeed;
        if (inputHandler.IsSprinting)
        {
            currentSpeed *= sprintMultiplier;
        }

        Vector3 move = transform.right * inputHandler.MoveInput.x + transform.forward * inputHandler.MoveInput.y;
        characterController.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
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
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
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
            float force = Mathf.Lerp(5f, 25f, power / maxChargePower);

            heldBall.photonView.RPC("RPC_ThrowBall", RpcTarget.All, throwDir * force);
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
            cameraPitch = (float)stream.ReceiveNext();
            if (cameraTransform != null && !photonView.IsMine)
            {
                cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
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
}