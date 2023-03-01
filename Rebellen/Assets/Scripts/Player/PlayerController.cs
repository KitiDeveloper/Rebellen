using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Advanced settings")]
    [SerializeField] private float runBuildUp;
    [SerializeField] private float slopeForce;
    [SerializeField] private float slopeForceRayLength;
    [SerializeField] private AnimationCurve jumpFallOff;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [Header("Player stats")]
    [SerializeField] private float standingHeightY = 1.7f;
    [SerializeField] private float crouchHeightY = 0.9f;
    [SerializeField] private float crouchSpeed, walkSpeed, runSpeed;
    [SerializeField] private float jumpMultiplier;

    [Header("Player inputs")]
    [SerializeField] private string horizontalInputName;
    [SerializeField] private string verticalInputName;
    [SerializeField] private KeyCode runKey;
    [SerializeField] private KeyCode jumpKey;
    [SerializeField] private KeyCode crouchKey;

    [Header("Camera settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 cameraStandingPosition;
    [SerializeField] private Vector3 cameraCrouchPosition;
    [SerializeField] private float cameraHeightLerpSpeed = 5f;


    private CharacterController charController;

    private float movementSpeed;

    private bool isJumping;

    private bool isCrouched;

    private bool StartCon = true;

    private void Awake()
    {
        charController = GetComponent<CharacterController>();
        isCrouched = false;
    }

    private void Update()
    {
        PlayerMovement();
    }

    private void PlayerMovement()
    {
        float vertInput = Input.GetAxis(verticalInputName);
        float horizInput = Input.GetAxis(horizontalInputName);

        Vector3 forwardMovement = transform.forward * vertInput;
        Vector3 rightMovement = transform.right * horizInput;

        charController.SimpleMove(Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * movementSpeed);

        if((vertInput != 0 || horizInput != 0) && OnSlope())
        {
            charController.Move(Vector3.down * charController.height / 2 * slopeForce * Time.deltaTime);
        }

        if (isCrouched == true)
        {
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, cameraCrouchPosition, Time.deltaTime * cameraHeightLerpSpeed);
            StartCon = false;
        }
        else
        {
            if(StartCon == false)
            {
                cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, cameraStandingPosition, Time.deltaTime * cameraHeightLerpSpeed);
            }
        }

        SetMovementSpeed();
        JumpInput();
    }

    private void SetMovementSpeed()
    {
        if (Input.GetKey(crouchKey))
        {
            movementSpeed = Mathf.Lerp(movementSpeed, crouchSpeed, Time.deltaTime * runBuildUp);
            CrouchEvent();
            isCrouched = true;
        }
        else if (Input.GetKey(runKey))
        {
            StandEvent();
            movementSpeed = Mathf.Lerp(movementSpeed, runSpeed, Time.deltaTime * runBuildUp);
            isCrouched = false;
        }
        else
        {
            StandEvent();
            movementSpeed = Mathf.Lerp(movementSpeed, walkSpeed, Time.deltaTime * runBuildUp);
            isCrouched = false;
        }
    }

    private bool OnSlope()
    {
        if(isJumping)
        {
            return false;
        }

        RaycastHit hit;

        if(Physics.Raycast(transform.position, Vector3.down, out hit, charController.height / 2 * slopeForceRayLength))
        {
            if(hit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private void CrouchEvent()
    {
        if (charController.height > crouchHeightY)
        {
            UpdatePlayerHeight(crouchHeightY);

            if (charController.height - 0.05f <= crouchHeightY)
            {
                charController.height = crouchHeightY;
            }
        }
    }

    private void StandEvent()
    {
        if (charController.height < standingHeightY)
        {
            float lastHeight = charController.height;
            UpdatePlayerHeight(standingHeightY);

            if (charController.height + 0.05f >= standingHeightY)
            {
                charController.height = standingHeightY;
            }

            transform.position += new Vector3(0, (charController.height - lastHeight), 0);
        }
    }

    private void UpdatePlayerHeight(float newHeight)
    {
        charController.height = Mathf.Lerp(charController.height, newHeight, crouchTransitionSpeed * Time.deltaTime);
    }

    private void JumpInput()
    {
        if(Input.GetKeyDown(jumpKey) && !isJumping)
        {
            isJumping = true;
            StartCoroutine(JumpEvent());
        }
    }

    private IEnumerator JumpEvent()
    {
        charController.slopeLimit = 90.0f;
        float timeInAir = 0.0f;

        do
        {
            float JumpForce = jumpFallOff.Evaluate(timeInAir);
            charController.Move(Vector3.up * JumpForce * jumpMultiplier * Time.deltaTime);
            timeInAir += Time.deltaTime;

            yield return null;

        } while (!charController.isGrounded && charController.collisionFlags != CollisionFlags.Above);

        yield return new WaitForSeconds(0.5f);
        charController.slopeLimit = 45.0f;
        isJumping = false;
    }
}