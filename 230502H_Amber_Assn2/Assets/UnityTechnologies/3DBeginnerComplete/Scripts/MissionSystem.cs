using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MissionSystem : NetworkBehaviour
{
    NetworkVariable<int> missionItemsCount = new(3);
    Notification notifText;
    GameObject[] missionItems = new GameObject[3];
    [SerializeField] NetworkObject missionItemPrefab;
    bool hasInit = false;

    public override void OnNetworkSpawn()
    {
        notifText = FindObjectOfType<Notification>();
        Debug.Log("Mission system notif: " + (notifText != null));
        if(IsServer)
        {

            Vector3[] positionArray3 = new[] {new Vector3(-4.2f, 0.1f, -0.7f),
                                                new Vector3(-1.9f,0.1f,5.5f),
                                                new Vector3(-0.5f,0.1f,11.3f)};

            foreach (Vector3 c in positionArray3)
            {
                var instance = Instantiate(missionItemPrefab);
                instance.transform.position = c;
                var instanceNWO = instance.GetComponent<NetworkObject>();
                instanceNWO.Spawn();
                instanceNWO.transform.parent = transform;
            }
        }
        InitList();
    }
    private void InitList()
    {
        Debug.Log("Call init list " + transform.childCount);
        if (transform.childCount <= 0)
            return;
        for (int i = 0; i < 3; i++)
            missionItems[i] = transform.GetChild(i).gameObject;
        hasInit = true;
        Debug.Log("Mission system: " + (missionItems[0] != null));
    }
    private void Update()
    {
        if (!hasInit)
            InitList();
        if(Input.GetKeyUp(KeyCode.M))
        {
            for(int i=0; i<3; i++)
            {
                GameObject go = missionItems[i];
                if (!go) continue;
                Collider[] colls = Physics.OverlapBox(go.transform.position, new Vector3(1, 1, 1));
                foreach (Collider coll in colls)
                {
                    if (coll.TryGetComponent(out PlayerMovement player))
                    {
                        ServerMissionStepRPC(i);
                        break;
                    }

                }
            }
        }
    }


    [Rpc(SendTo.Server)]
    private void ServerMissionStepRPC(int index)
    {
        missionItemsCount.Value--;
        Destroy(missionItems[index]);
        if (missionItemsCount.Value == 0)
        {
            NotifyMissionRPC(index, "Hidden mission complete!");
            foreach (NetworkClient client in NetworkManager.ConnectedClientsList)
                client.PlayerObject.GetComponent<PlayerMovement>().hp.Value += 10;
        }
        else
            NotifyMissionRPC(index, "Hidden mission: Found " + (3 - missionItemsCount.Value) + " /3 bombs");
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void NotifyMissionRPC(int index, string msg)
    {
        missionItems[index] = null;
        notifText.ShowNotif(msg);
    }
}