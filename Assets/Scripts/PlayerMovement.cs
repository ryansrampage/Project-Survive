using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Gives access to input action map and character controller respectively
    //private PlayerControls controls;
    private CharacterController controller;

    //Values in relation to character movement
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float walkspeed = 10f;
    [SerializeField] private float sprintSpeed = 20f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2.4f;

    //Handles staying grounded and sprinting
    private Vector3 velocity;
    public bool isSprinting;
    public bool isGrounded;

    //Event based input values
    private Vector2 move;
    private float jump;
    private float aim;
    private float sprint;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        PlayerMove();
        Gravity();
    }

    private void PlayerMove()
    {
        Vector3 movement = (move.y * transform.forward) + (move.x * transform.right);
        controller.Move(movement * moveSpeed * Time.deltaTime);
    }

    private void Gravity()
    {
        isGrounded = controller.isGrounded;
        
        //Snap player to floor
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -1f;
        }

        //Calculate and apply, to characater, gravity every frame
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Sprint()
    {
        if (sprint > 0)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

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

    public void OnAim(InputAction.CallbackContext context)
    {
        aim = context.ReadValue<float>();
    }
}
