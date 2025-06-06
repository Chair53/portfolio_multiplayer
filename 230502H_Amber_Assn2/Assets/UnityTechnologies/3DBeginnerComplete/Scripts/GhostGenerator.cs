using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GhostGenerator : NetworkBehaviour
{
    public bool canUse = false;
    public NetworkVariable<int> soulsNeeded = new(4);
    [SerializeField] GameObject bossPrefab;
    [SerializeField] Vector3 bossPos = new(-9, -0.4f, 0);
    [SerializeField] GhostCompanion ghostPrefab;
    [SerializeField] TMP_Text uiText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //if(IsServer)
        {
            soulsNeeded.OnValueChanged += CheckSouls;
            Debug.Log("Generator += checksouls");
        }
    }

    private void CheckSouls(int prev, int curr)
    {
        Debug.Log("Check souls");
        if(curr == 0)
        {
            SetUsableRPC(true, 0);
            if(IsServer)
                Instantiate(bossPrefab, bossPos, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
        }
        else
            SetUsableRPC(false, (4-curr));
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetUsableRPC(bool usable, int numsouls)
    {
        if(usable)
            uiText.text = "[E] Call ghost";
        else
            uiText.text = "Ghost souls: " + numsouls + "/4";
        canUse = usable;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnGhostRPC()
    {
        Debug.Log("Spawn ghost");
        canUse = false;
        if(IsServer)
        {
            Debug.Log("Server spawn ghost");
            Instantiate(ghostPrefab, transform.position, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
        }
    }
}