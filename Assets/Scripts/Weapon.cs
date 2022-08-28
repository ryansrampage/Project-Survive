using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{


    private Vector3 originalPosition;
    public Vector3 aimPosition;
    bool isAiming;
    public float adsSpeed = 5;

    private void Awake()
    {

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

}
