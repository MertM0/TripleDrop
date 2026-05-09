using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool IsSprinting { get; private set; }

    public delegate void JumpEvent();
    public event JumpEvent OnJumpEvent;

    public delegate void ThrowStartedEvent();
    public event ThrowStartedEvent OnThrowStartedEvent;

    public delegate void ThrowCanceledEvent();
    public event ThrowCanceledEvent OnThrowCanceledEvent;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        inputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => LookInput = Vector2.zero;

        inputActions.Player.Sprint.performed += ctx => IsSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => IsSprinting = false;

        inputActions.Player.Jump.performed += ctx => OnJumpEvent?.Invoke();

        inputActions.Player.Attack.started += ctx => OnThrowStartedEvent?.Invoke();
        inputActions.Player.Attack.canceled += ctx => OnThrowCanceledEvent?.Invoke();
    }

    public void EnableInputs()
    {
        inputActions.Player.Enable();
    }

    public void DisableInputs()
    {
        inputActions.Player.Disable();
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Player.Disable();
            inputActions.Dispose();
        }
    }
}
