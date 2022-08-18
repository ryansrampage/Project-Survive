using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    //Handle camera rotation
    [SerializeField] private float mouseSens = 100f;
    [SerializeField] private Transform playerBody;
    private Vector2 mouseLook;
    private float xRotation;

    //Sprint FOV Changes
    private PlayerMovement playerMovement;
    private Camera cam;
    [SerializeField] private float walkFOV = 90f;
    [SerializeField] private float sprintFOV = 110f;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = GetComponent<Camera>();
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void Start()
    {
        cam.fieldOfView = walkFOV;
    }

    private void LateUpdate()
    {
        Look();
        SprintFOV();
    }

    private void Look()
    {
        float mouseX = mouseLook.x * mouseSens;
        float mouseY = mouseLook.y * mouseSens;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void SprintFOV()
    {
        if (playerMovement.isSprinting)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, sprintFOV, 5 * Time.deltaTime);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, walkFOV, 5 * Time.deltaTime);
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        mouseLook = context.action.ReadValue<Vector2>();
    }
}
