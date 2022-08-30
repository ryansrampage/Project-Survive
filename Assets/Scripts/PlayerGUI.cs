using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerGUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerData;
    [SerializeField] private RigidBodyMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerData.text = "Speed: " + playerMovement.GetMoveSpeed().ToString();
    }
}
