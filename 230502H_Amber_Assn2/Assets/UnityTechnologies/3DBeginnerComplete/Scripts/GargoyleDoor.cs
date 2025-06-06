using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GargoyleDoor : NetworkBehaviour
{
    public NetworkVariable<int> keysNeeded = new(4);
    [SerializeField] TMP_Text uiText;
    [SerializeField] BoxCollider blocking;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        keysNeeded.OnValueChanged += CheckKeys;
        Debug.Log("Door += checkKeys");
    }

    private void CheckKeys(int prev, int curr)
    {
        Debug.Log("Check keys");
        uiText.text = "Gargoyle keys: " + (4 - curr) + "/4";
        if (curr == 0)
        {
            OpenDoorRPC();
            uiText.enabled = false;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void OpenDoorRPC()
    {
        GetComponent<MeshRenderer>().enabled = false;
        blocking.enabled = false;
        Debug.Log("Open door");
    }
}