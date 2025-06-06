using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class EnemyRPC : NetworkBehaviour
{
    NetworkVariable<int> health = new(30);
    NetworkList<ulong> damagerIDs;
    [SerializeField] Item dropPrefab;
    [SerializeField] bool testingGen = true;
    [SerializeField] TMP_Text hpTxt;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        health.OnValueChanged += UpdateHealth;
    }
    private void Awake()
    {
        damagerIDs = new();
    }
    public void TakeDamage(ulong whoDamaged)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            health.Value = Math.Max(0, health.Value-5);
            if(!damagerIDs.Contains(whoDamaged))
                damagerIDs.Add(whoDamaged);

            if(health.Value == 0)
            {
                //drops etc
                KillEnemyRPC(whoDamaged);
            }
        }
    }

    [Rpc(SendTo.Server)]
    void KillEnemyRPC(ulong killerID)
    {
        Debug.Log("Enemydie");
        foreach (ulong id in damagerIDs)
        {
            Debug.Log("enemie die: " + id);
            PlayerMovement p = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<PlayerMovement>();
            if (p == null)
                continue;
            if (id == killerID)
                p.hp.Value += 30;
            else
                p.hp.Value += 20;
        }

        //create item on the server
        Instantiate(dropPrefab, new Vector3(transform.position.x, 0.5f, transform.position.z),
            transform.rotation).GetComponent<NetworkObject>().Spawn();
        if(testingGen)
        {
            Instantiate(dropPrefab, transform.position, transform.rotation).GetComponent<NetworkObject>().Spawn();
            Instantiate(dropPrefab, transform.position, transform.rotation).GetComponent<NetworkObject>().Spawn();
            Instantiate(dropPrefab, transform.position, transform.rotation).GetComponent<NetworkObject>().Spawn();
        }
        Destroy(gameObject);
    }

    private void UpdateHealth(int prev, int curr)
    {
        hpTxt.text = "HP: " + curr;
    }
}