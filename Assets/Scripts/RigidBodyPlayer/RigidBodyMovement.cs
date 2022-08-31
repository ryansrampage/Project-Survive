using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RigidBodyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float groundDrag;
    private Vector3 moveDirection;
    private float moveSpeed;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float airMultiplier;

    [Header("Crouching")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchYScale;
    private float startYScale;

    [Header("Grounding")]
    [SerializeField] private float playerHeight;
    public LayerMask groundMask;
    public bool isGrounded;
    private bool crouchFloorSnap = false;

    [Header("Slope Handler")]
    [SerializeField] private float maxSlopeAngle;
    private RaycastHit slopeHit;

    [Header("Oritentation")]
    [SerializeField] private Transform orientation;
    
    //Internal event based movement variables
    private Vector2 move;
    private float jump;
    private float sprint;
    private float crouch;

    //RigidBody
    private Rigidbody rb;

    [Header("Movement State")]
    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
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
    }

    private void Update()
    {
        //Ground Check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask);

        //Run checks
        SpeedControl();
        StateHandler();

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
    }

    private void StateHandler()
    {
        //Potentially modify to update based on state instead of doing both in same statement

        if (isGrounded && sprint > 0) //Sprinting
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (isGrounded) //Walking
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else //In the Air
        {
            state = MovementState.air;
        }

        if (crouch > 0 && isGrounded) //Crouching
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        else if (crouch > 0 && !isGrounded) //Air crouching
        {
            state = MovementState.aircrouch;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * move.y + orientation.right * move.x;

        //Player is on a slope
        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        if (isGrounded)
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
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    private void Jump()
    {
        if (isGrounded && jump > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
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

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    //Input Event Functions ----------------------------------------
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

    //Getters & Setters -- Add when necessary
    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public Rigidbody GetRigidBody()
    {
        return rb;
    }
}
