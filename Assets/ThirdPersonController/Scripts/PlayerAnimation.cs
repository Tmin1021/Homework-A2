using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private float animationBlendSpeed = 4f;

    private PlayerLocomotionInput inputHandler;
    private PlayerState playerStateManager;

    private static int horizontalInputHash = Animator.StringToHash("inputX");
    private static int verticalInputHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");

    private Vector3 currentAnimationBlend = Vector3.zero;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerLocomotionInput>();
        playerStateManager = GetComponent<PlayerState>();
    }

    private void Update()
    {
        ProcessAnimationParameters();
    }

    /*
     * Animation Parameter Processing:
     * - Checks current movement state (running vs sprinting)
     * - Calculates target input values based on state
     * - Smoothly blends between animation values using Lerp
     * - Updates animator parameters for blend tree control
     */
    private void ProcessAnimationParameters()
    {
        bool isCurrentlySprinting = playerStateManager.currentPlayerMovementState == PlayerMovementState.Sprinting;

        Vector2 targetInputValues = isCurrentlySprinting ? inputHandler.movementInput * 1.5f : inputHandler.movementInput;
        currentAnimationBlend = Vector3.Lerp(currentAnimationBlend, targetInputValues, animationBlendSpeed * Time.deltaTime);

        characterAnimator.SetFloat(horizontalInputHash, currentAnimationBlend.x);
        characterAnimator.SetFloat(verticalInputHash, currentAnimationBlend.y);
        characterAnimator.SetFloat(inputMagnitudeHash, currentAnimationBlend.magnitude);
    }
}