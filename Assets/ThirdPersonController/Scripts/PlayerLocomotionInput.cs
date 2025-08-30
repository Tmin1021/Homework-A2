using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-2)]
public class PlayerLocomotionInput : MonoBehaviour, PlayerControl.ILocomotionMapActions
{
    public PlayerControl playerControl { get; private set; }
    public Vector2 movementInput { get; private set; }
    public Vector2 lookInput { get; private set; }

    private void OnEnable()
    {
        playerControl = new PlayerControl();
        playerControl.Enable();

        playerControl.LocomotionMap.Enable();
        playerControl.LocomotionMap.SetCallbacks(this);
    }

    private void OnDisable()
    {
        playerControl.LocomotionMap.Disable();
        playerControl.LocomotionMap.RemoveCallbacks(this);
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
        print(movementInput);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
}