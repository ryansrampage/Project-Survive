using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class RigidBodyCamera : MonoBehaviour
{
    [SerializeField] private float mouseSens = 100f;

    private Vector2 mouseLook;
    private float xRotation;
    private float yRotation;

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform camHolder;

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

        //Rotate the camera and the orientation of the player
        camHolder.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        mouseLook = context.action.ReadValue<Vector2>();
    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }
}