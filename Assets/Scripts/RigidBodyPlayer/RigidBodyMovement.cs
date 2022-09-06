using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RigidBodyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float slideSpeed;
    [SerializeField] private float wallRunSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private float speedIncreaseMultiplier;
    [SerializeField] private float slopeIncreaseMultiplier;

    //Internal speed variables
    private Vector3 moveDirection;
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float airMultiplier;

    [Header("Crouching")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchYScale;
    private float startYScale;

    [Header("Sliding")]
    [SerializeField] private float maxSlideTime;
    [SerializeField] private float slideForce;
    [SerializeField] private float slideYScale;
    [SerializeField] private float slideCooldown;
    [SerializeField] private float slideCooldownTime;
    private float slideTimer;
    private bool readyToSlide;
    private bool sliding;

    [Header("Wallrunning")]
    public LayerMask wallMask;
    [SerializeField] private float wallRunForce;
    [SerializeField] private float maxWallRunTime;
    private float wallRunTimer;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;
    public bool wallrunning; //make private later (for testing)

    [Header("Grounding")]
    [SerializeField] private float playerHeight;
    public LayerMask groundMask;
    public bool isGrounded;
    private bool crouchFloorSnap = false;

    [Header("Slope Handler")]
    [SerializeField] private float maxSlopeAngle;
    private RaycastHit slopeHit;
    [SerializeField] private bool exitingSlope;

    [Header("Oritentation")]
    [SerializeField] private Transform orientation;

    //Internal event based movement variables
    private Vector2 move;
    private float jump;
    private float sprint;
    private float crouch;
    private float slide;

    //RigidBody
    private Rigidbody rb;

    [Header("Movement State")]
    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        sliding,
        aircrouch,
        air
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        //Set initial scale (For crouching)
        startYScale = transform.localScale.y;

        //Set initial slide cooldown time
        slideCooldown = slideCooldownTime;
        readyToSlide = true;
    }

    private void Update()
    {
        //Run checks
        GroundCheck();
        CheckForWall();
        SpeedControl();
        StateHandler();
        SlideCheck();
        WallRunCheck();

        if (!readyToSlide)
        {
            slideCooldown -= Time.deltaTime;

            if (slideCooldown <= 0)
            {
                readyToSlide = true;
                slideCooldown = slideCooldownTime;
            }
        }

        //Handles drag
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MovePlayer();
        Jump();
        Crouch();
        SlidingMovement();
        WallRunMovement();
    }

    private void StateHandler()
    {
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallRunSpeed;
        }
        else if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }
        else if (crouch > 0 && isGrounded) //Crouching
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        else if (crouch > 0 && !isGrounded) //Air crouching
        {
            state = MovementState.aircrouch;
        }

        else if (isGrounded && sprint > 0) //Sprinting
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (isGrounded) //Walking
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else //In the Air
        {
            state = MovementState.air;
        }
        
        //Check if move speed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
            {
                time += Time.deltaTime * speedIncreaseMultiplier;
            }
            
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, groundMask);
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * move.y + orientation.right * move.x;

        //Player is on a slope
        if (OnSlope() && !exitingSlope) //Checks player is on slope and not exiting the slope
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        else if (isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        //Slope speed limiting
        if (OnSlope() && !exitingSlope) //Checks player is on slope and not exiting the slope
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
            }
        }
    }

    private void Jump()
    {
        if (isGrounded && jump > 0)
        {
            exitingSlope = true;

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            Invoke(nameof(SlopeExit), 0.2f); //Reset the slope exit flag so it can apply appropriate OnSlope() measures
        }
    }

    private void Crouch()
    {
        if (crouch > 0)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

            //Snap player to floor once only if they are grounded when initiating the crouch
            if (!crouchFloorSnap && isGrounded)
            {
                rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
                crouchFloorSnap = true;
            }
            else if (!crouchFloorSnap && !isGrounded) //Prevents a snap upon contact with the floor if player initates crouch in the air
            {
                crouchFloorSnap = true;
            }
            
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

            //Reset the crouch snap so they don't get floating crouches
            crouchFloorSnap = false;
        }
    }

    private void SlideCheck()
    {
        //If they press the slide button, are sprinting, not sliding, grounded and ready to slide
        if (slide > 0 && 
            moveSpeed > walkSpeed && 
            !sliding &&
            isGrounded &&
            readyToSlide)
        {
            sliding = true;
            slideTimer = maxSlideTime;
        }
    }

    private void SlidingMovement()
    {
        if (sliding)
        {
            //Scale player down
            transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);

            if (!OnSlope() || rb.velocity.y > -0.1f)
            {
                //Add the force to slide
                rb.AddForce(moveDirection.normalized * slideForce, ForceMode.Force);

                slideTimer -= Time.fixedDeltaTime;
            }
            else
            {
                rb.AddForce(GetSlopeMoveDirection(moveDirection) * slideForce, ForceMode.Force);
            }

            if (slideTimer <= 0)
            {
                StopSlide();
            }
        }
    }

    private void StopSlide()
    {
        sliding = false;
        readyToSlide = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

    private void WallRunCheck()
    {
        if ((wallLeft || wallRight) && move.y > 0 && AboveGround())
        {
            wallrunning = true;
        }
        else
        {
            wallrunning = false;
        }
    }

    private void WallRunMovement()
    {
        if (wallrunning)
        {
            rb.useGravity = false;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

            Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

            if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            {
                wallForward = -wallForward;
            }

            rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
        }
    }

    private void StopWallRun()
    {
        wallrunning = false;
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, wallMask);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, wallMask);
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void SlopeExit()
    {
        exitingSlope = false;
    }

    //---------------------------------- Input Event Functions ----------------------------------
    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jump = context.ReadValue<float>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        sprint = context.ReadValue<float>();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        crouch = context.ReadValue<float>();
    }

    public void OnSlide(InputAction.CallbackContext context)
    {
        slide = context.ReadValue<float>();
    }

    //---------------------------------- Getters & Setters -- Add when necessary ----------------------------------
    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public Rigidbody GetRigidBody()
    {
        return rb;
    }

    public MovementState GetMovementState()
    {
        return state;
    }
}
