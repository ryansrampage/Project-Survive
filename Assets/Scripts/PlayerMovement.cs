using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Gives access to input action map and character controller respectively
    private PlayerControls controls;
    private CharacterController controller;

    //Values in relation to character movement
    private float moveSpeed = 6f;
    private float sprintSpeed = 12f;
    private Vector3 velocity;
    [SerializeField]  private float gravity = -9.81f;
    private Vector2 move;
    private float jumpHeight = 2.4f;
    public bool isSprinting;

    //Variables in relation to if the character is grounded or not
    public Transform ground;
    public float distToGround = 1.3f;
    public LayerMask groundMask;
    public bool isGrounded;

    //Camera values
    public Camera cam;
    public float walkFov = 90f;
    public float sprintFov = 105f;

    private void Awake()
    {
        controls = new PlayerControls();
        controller = GetComponent<CharacterController>();
        cam.fieldOfView = walkFov;

        controls.Gameplay.Enable();

        //controls.Gameplay.Movement.started += PlayerMove;
        

        controls.Gameplay.Sprint.performed += ctx => SprintPressed();
        controls.Gameplay.Sprint.canceled += ctx => SprintReleased();
        controls.Gameplay.Movement.performed += ctx => PlayerMove();
    }

    private void Update()
    {
        
        //Gravity();
        PlayerMove();
        //Jump();
        //Sprint();
    }

    public void Gravity()
    {
        isGrounded = Physics.CheckSphere(ground.position, distToGround, groundMask);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -1f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void PlayerMove()
    {
        //Debug.Log(ctx);
        move = controls.Gameplay.Movement.ReadValue<Vector2>();

        Vector3 movement = (move.y * transform.forward) + (move.x * transform.right);
        controller.Move(movement * moveSpeed * Time.deltaTime);
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        Debug.Log("Here JUMP");
        if (controls.Gameplay.Jump.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void SprintPressed()
    {
        isSprinting = true;
    }

    private void SprintReleased()
    {
        isSprinting = false;
    }

    private void Sprint()
    {
        if (isSprinting)
        {
            moveSpeed = sprintSpeed;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, sprintFov, 5 * Time.deltaTime);
        }
        else
        {
            moveSpeed = 6f;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, walkFov, 5 * Time.deltaTime);
        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

}
