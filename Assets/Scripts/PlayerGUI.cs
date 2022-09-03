using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerGUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerSpeed;
    [SerializeField] private TextMeshProUGUI playerState;
    [SerializeField] private RigidBodyMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerSpeed.text = "Speed: " + playerMovement.GetMoveSpeed().ToString();
        playerState.text = playerMovement.GetMovementState().ToString();
    }
}
