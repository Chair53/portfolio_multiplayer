using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GhostCompanion : NetworkBehaviour
{
    float finaldriftTime = 3f;
    public Transform target = null;
    Vector3 finalDir = Vector3.zero;
    Boss boss;
    bool canChangeTarget = true;
    GhostGenerator generator;

    private void Awake()
    {
        boss = FindObjectOfType<Boss>();
        if (!boss)
            Debug.Log("unemployed");
        generator = FindObjectOfType<GhostGenerator>();
        if (!generator)
            Debug.Log("Ghost no gen");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (boss && other.gameObject == boss.gameObject)
        {
            Debug.Log("Ghost -> boss");
            CannotChangeTargetRPC();
            boss.HitByGhostRPC();
        }
        else if(target && other.transform == target)
        {
            Debug.Log("Ghost -> target");
            RemoveTargetRPC();
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.G))
        {
            Debug.Log("Try atract ghost");
            AttractRPC();
        }
        if (target)
        {
            Vector3 dir = target.position - transform.position;
            dir.y = 0;
            dir.Normalize();
            transform.position += dir * 4f * Time.deltaTime;
        }
        else if(finalDir != Vector3.zero)
        {
            transform.position += finalDir * 6f * Time.deltaTime;
            finaldriftTime -= Time.deltaTime;
            if (finaldriftTime <= 0)
            {
                FindObjectOfType<GhostGenerator>().SetUsableRPC(true, 0);
                if(IsServer)
                {
                    Debug.Log("Byebye ghost");
                    Destroy(gameObject);
                }
            }
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void RemoveTargetRPC()
    {
        Debug.Log("Ghost remove target");
        target = null;
        canChangeTarget = true;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CannotChangeTargetRPC()
    {
        canChangeTarget = false;
        finalDir = target.position - transform.position;
        finalDir.y = 0;
        finalDir.Normalize();
        target = null;
    }

    [Rpc(SendTo.Server)]
    private void AttractRPC(RpcParams rpcParams = default)
    {
        Debug.Log("Ghost attract");
        if (!canChangeTarget)
            return;
        Debug.Log("Ghost can change target");
        ulong senderID = rpcParams.Receive.SenderClientId;
        NetworkManager.Singleton.ConnectedClients[senderID].PlayerObject.TryGetComponent(out NetworkObject senderPlayer);
        if (senderPlayer)
        {
            Debug.Log("ghost Sending target");
            ChangeTargetRPC(senderPlayer);
        }
        else
            Debug.Log("WHAT (ghost)");
    }
    [Rpc(SendTo.ClientsAndHost)]
    void ChangeTargetRPC(NetworkObjectReference newtargetRef)
    {
        Debug.Log("ghost Change target");
        if (newtargetRef.TryGet(out NetworkObject nwo))
            target = nwo.gameObject.transform;
        else
            Debug.Log("change null target");
    }
}