using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateCameraPosition : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    // Update is called once per frame
    void Update()
    {
        transform.position = cameraTransform.position;
    }
}