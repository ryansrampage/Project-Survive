using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private PlayerControls controls;

    private Vector3 originalPosition;
    public Vector3 aimPosition;
    bool isAiming;
    public float adsSpeed = 5;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.Aim.performed += ctx => isAiming = true;
        controls.Gameplay.Aim.canceled += ctx => isAiming = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        ADS();
    }

    private void ADS()
    {
        if(isAiming) //expand for reloading
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPosition, Time.deltaTime * adsSpeed);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * adsSpeed);
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
