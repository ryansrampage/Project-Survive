using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerObject;
    private Rigidbody rb;
    private RigidBodyMovement movement;

    
    private float startYScale;



    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<RigidBodyMovement>();

        startYScale = playerObject.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
