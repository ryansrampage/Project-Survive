using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSway : MonoBehaviour
{
    private PlayerControls controls;

    [Header("Rotation")]
    public float rotationAmount = 2f;
    public float rotationMaxAmount = 4f;
    public float rotationSmooth = 6f;

    [Space]
    public bool rotationX;
    public bool rotationY;
    public bool rotationZ;

    [Header("Position")]
    public float positionAmount = 0.002f;
    public float positionMaxAmount = 0.006f;
    public float positionSmooth = 1f;

    [Header("Weapon Bounce")]


    //Internal Variables
    private Quaternion originRotation;
    private Vector3 originPosition;
    Vector2 mouseLook;
    float mouseX;
    float mouseY;
    bool isAiming;
    
    private void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.Aim.performed += ctx => isAiming = true;
        controls.Gameplay.Aim.canceled += ctx => isAiming = false;
    }

    private void Start()
    {
        originRotation = transform.localRotation;
        originPosition = transform.localPosition;
    }

    private void Update()
    {
        CalculateSway();

        AimDamp();
        UpdateTiltSway();
        UpdateMoveSway();
    }

    private void CalculateSway()
    {
        //Get the mouse data
        mouseLook = controls.Gameplay.Look.ReadValue<Vector2>();
        mouseX = mouseLook.x;
        mouseY = mouseLook.y;
    }

    private void UpdateTiltSway()
    {
        float tiltX = Mathf.Clamp(-mouseY * rotationAmount, -rotationMaxAmount, rotationMaxAmount);
        float tiltY = Mathf.Clamp(mouseX * rotationAmount, -rotationMaxAmount, rotationMaxAmount);

        Quaternion targetRotation = Quaternion.Euler(new Vector3(
            rotationX ? -tiltX : 0f, 
            rotationY ? tiltY : 0f, 
            rotationZ ? tiltY : 0f
            ));

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation * originRotation, Time.deltaTime * rotationSmooth);
    }

    private void UpdateMoveSway()
    {
        float moveX = Mathf.Clamp(-mouseX * positionAmount, -positionMaxAmount, positionMaxAmount);
        float moveY = Mathf.Clamp(-mouseY * positionAmount, -positionMaxAmount, positionMaxAmount);

        Vector3 targetPosition = new Vector3(moveX, moveY, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition + originPosition, Time.deltaTime * positionSmooth);
    }

    private void AimDamp()
    {
        if (isAiming)
        {
            rotationMaxAmount = 0.4f;
            positionMaxAmount = 0.001f;
        }
        else
        {
            rotationMaxAmount = 4f;
            positionMaxAmount = 0.006f;
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
