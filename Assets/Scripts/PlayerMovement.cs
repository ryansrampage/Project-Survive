using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Gives access to input action map and character controller respectively
    //private PlayerControls controls;
    private CharacterController controller;

    //Values in relation to character movement
    private float moveSpeed = 12f;
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
        controller = GetComponent<CharacterController>();
        cam.fieldOfView = walkFov;
    }

    private void Update()
    {
        PlayerMove();
    }

    public void PlayerMove()
    {
        Debug.Log(move.x);
        Vector3 movement = (move.y * transform.forward) + (move.x * transform.right);
        controller.Move(movement * moveSpeed * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log(context.ReadValue<Vector2>());
        move = context.ReadValue<Vector2>();
    }

    private void OnEnable()
    {
        //controls.Enable();
    }

    private void OnDisable()
    {
        //controls.Disable();
    }

}
