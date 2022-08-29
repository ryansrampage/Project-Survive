using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RigidBodyCamera : MonoBehaviour
{
    [SerializeField] private float mouseSens = 100f;

    private Vector2 mouseLook;
    private float xRotation;
    private float yRotation;
    [SerializeField] private Transform orientation;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Look();
    }

    public void Look()
    {
        float mouseX = mouseLook.x * mouseSens;
        float mouseY = mouseLook.y * mouseSens;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        mouseLook = context.action.ReadValue<Vector2>();
    }
}