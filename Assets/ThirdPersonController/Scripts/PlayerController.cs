using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera playerCamera;

    [Header("Movement Settings")]
    public float movementAcceleration = 35f;
    public float maxMovementSpeed = 4f;
    public float movementDrag = 20f;
    public float movementThreshold = 0.01f;
    public float gravity = -9.81f; 
    public LayerMask groundLayers; 

    private float verticalVelocity = 0f;
    private bool isOnSlope = false;
    private Vector3 slopeNormal;

    [Header("Camera Settings")]
    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    public float lookLimitV = 89f;

    private PlayerLocomotionInput inputHandler;
    private PlayerState playerStateManager;

    private Vector2 currentCameraRotation = Vector2.zero;
    private Vector2 targetPlayerRotation = Vector2.zero;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerLocomotionInput>();
        playerStateManager = GetComponent<PlayerState>();

        if (groundLayers.value == 0)
            groundLayers = ~0; 
    }

    #region Update Logic
    private void Update()
    {
        DetermineMovementState();
        ProcessPlayerMovement();
    }

    private void DetermineMovementState()
    {
        bool hasMovementInput = inputHandler.movementInput != Vector2.zero;
        bool isCurrentlyMoving = IsPlayerMoving();
        PlayerMovementState currentState = isCurrentlyMoving || hasMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;

        playerStateManager.SetPlayerMovementState(currentState);
    }

    /* 
     * Player Movement Calculation Process:
     * 
     * 1. Camera-Relative Direction:
     *    - Flattens camera forward/right vectors to horizontal plane
     *    - Combines with input to get camera-relative movement direction
     * 
     * 2. Slope Handling:
     *    - Detects ground beneath player
     *    - Projects movement direction onto slope surface when on non-level terrain
     *    - Prevents sliding into or through sloped surfaces
     *
     * 3. Velocity Calculation:
     *    - Applies acceleration force based on input direction
     *    - Adds to current velocity for smooth acceleration
     *    - Applies drag force to slow movement when no input is given
     *    - Clamps maximum velocity to prevent excessive speed
     * 
     * 4. Gravity Application:
     *    - Applies downward force when not grounded
     *    - Uses small downward force when grounded to maintain contact
     *
     * 5. Final Movement:
     *    - Combines horizontal velocity with vertical (gravity) component
     *    - Applies to CharacterController for actual movement
     */

    private void ProcessPlayerMovement()
    {
        Vector3 cameraForwardFlat = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;
        Vector3 cameraRightFlat = new Vector3(playerCamera.transform.right.x, 0f, playerCamera.transform.right.z).normalized;
        Vector3 inputDirection = cameraRightFlat * inputHandler.movementInput.x + cameraForwardFlat * inputHandler.movementInput.y;

        DetectGround(out RaycastHit groundHit);

        Vector3 movementDirection = inputDirection;
        if (isOnSlope)
        {
            movementDirection = Vector3.ProjectOnPlane(inputDirection, slopeNormal).normalized;
        }

        Vector3 movementDelta = movementDirection * movementAcceleration * Time.deltaTime;
        Vector3 newVelocity = characterController.velocity + movementDelta;

        Vector3 dragForce = newVelocity.normalized * movementDrag * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > movementDrag * Time.deltaTime) ? newVelocity - dragForce : Vector3.zero;
        newVelocity = Vector3.ClampMagnitude(newVelocity, maxMovementSpeed);

        
        if (characterController.isGrounded)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 finalMovement = newVelocity * Time.deltaTime;
        finalMovement.y = verticalVelocity * Time.deltaTime;

        characterController.Move(finalMovement);
    }
    
    private bool DetectGround(out RaycastHit hit)
    {
        Vector3 raycastOrigin = transform.position + Vector3.up * 0.1f;
        float raycastDistance = 2f;
        
        if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, raycastDistance, groundLayers))
        {
            slopeNormal = hit.normal;
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            isOnSlope = slopeAngle != 0 && slopeAngle <= characterController.slopeLimit;
            return true;
        }
        
        isOnSlope = false;
        return false;
    }
    #endregion

    #region Late Update Logic
    private void LateUpdate()
    {
        currentCameraRotation.x += lookSenseH * inputHandler.lookInput.x;
        currentCameraRotation.y = Mathf.Clamp(currentCameraRotation.y - lookSenseV * inputHandler.lookInput.y * 0.1f, -lookLimitV, lookLimitV);

        targetPlayerRotation.x += lookSenseH * inputHandler.lookInput.x * 0.1f;
        transform.rotation = Quaternion.Euler(0f, targetPlayerRotation.x, 0f);

        playerCamera.transform.rotation = Quaternion.Euler(currentCameraRotation.y, currentCameraRotation.x, 0f);
    }
    #endregion

    #region State Check
    private bool IsPlayerMoving()
    {
        Vector3 lateralVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
        return lateralVelocity.magnitude > movementThreshold;
    }
    #endregion
}