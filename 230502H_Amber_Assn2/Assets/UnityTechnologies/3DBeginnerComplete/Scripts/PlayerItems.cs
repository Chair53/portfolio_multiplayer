using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerItems : NetworkBehaviour
{
    Notification notifText;
    private NetworkList<int> inventory;
    GhostGenerator generator = null;
    GargoyleDoor door = null;
    PlayerMovement mvt;

    public override void OnNetworkSpawn()
    {
        notifText = GameObject.Find("Notif").GetComponent<Notification>();
        if(!IsOwner) return;
        base.OnNetworkSpawn();
        inventory.OnListChanged += NotifInv;
        mvt = GetComponent<PlayerMovement>();
    }
    void Awake()
    {
        inventory = new(default,
            NetworkVariableReadPermission.Owner,
            NetworkVariableWritePermission.Owner);
    }
    private void Update()
    {
        if (!IsOwner) return;

        if (mvt.hp.Value <= 0 || mvt.caught) return;

        if(Input.GetKeyUp(KeyCode.E))
        {
            if(generator)
            {
                if(generator.soulsNeeded.Value != 0)
                {
                    //submit soul
                    if (inventory.Contains((int)Item.ItemType.GhostSoul))
                    {
                        inventory.Remove((int)Item.ItemType.GhostSoul);
                        UseSoulRPC(generator.GetComponent<NetworkObject>());
                    }
                    else
                        notifText.ShowNotif("Need ghost soul");
                }
                else
                {
                    //summon ghost!! :O
                    if (generator.canUse)
                        generator.SpawnGhostRPC();
                    else
                        notifText.ShowNotif("Ghost is busy haunting");
                }
            }
            else if(door)
            {
                if (door.keysNeeded.Value != 0)
                {
                    //submit key
                    if (inventory.Contains((int)Item.ItemType.GargoyleKey))
                    {
                        inventory.Remove((int)Item.ItemType.GargoyleKey);
                        UseKeyRPC(door.GetComponent<NetworkObject>());
                    }
                    else
                        notifText.ShowNotif("Need gargoyle key");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        if (other.TryGetComponent(out Item item))
        {
            //pickup rpc
            OnPickupItemRPC(other.GetComponent<NetworkObject>());
        }
        else if(other.TryGetComponent(out generator))
        {
        }
        else if(other.TryGetComponent(out door))
        {
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;
        if (generator && other.gameObject == generator)
        {
            generator = null;
        }
        if (door && other.gameObject == door)
            door = null;
    }

    private void NotifInv(NetworkListEvent<int> changeEvent)
    {
        if(!IsOwner) return;
        Debug.Log("notif inventory called");
        string itemName = (Item.ItemType)changeEvent.Value == Item.ItemType.GhostSoul ? "Ghost soul" : "Gargoyle key";
        string s = "Inventory " + (changeEvent.Type == NetworkListEvent<int>.EventType.Add ? "added " : "removed ")
            + itemName;
        notifText.ShowNotif(s);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void AddItemRPC(Item.ItemType type, RpcParams rpcParams)
    {
        inventory.Add((int)type);
    }

    [Rpc(SendTo.Server)]
    private void OnPickupItemRPC(NetworkObjectReference item, RpcParams rpcParams = default)
    {
        if (!item.TryGet(out NetworkObject itemObj) || !itemObj.TryGetComponent<Item>(out Item itemComponent))
            return;
        //destroy the item and add to inventory
        AddItemRPC(itemComponent.type, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
        Destroy(itemObj.gameObject);
    }
    [Rpc(SendTo.Server)]
    private void UseSoulRPC(NetworkObjectReference generator, RpcParams rpcParams = default)
    {
        Debug.Log("UseSoulRPC");
        generator.TryGet(out NetworkObject nwobj);
        nwobj.TryGetComponent(out GhostGenerator ghostGenerator);
        bool complete = (--ghostGenerator.soulsNeeded.Value <= 0);
        if (complete)
            NotifyItemUsedRPC(rpcParams.Receive.SenderClientId, (int)Item.ItemType.GhostSoul,
               complete, RpcTarget.ClientsAndHost);
        else
            NotifyItemUsedRPC(rpcParams.Receive.SenderClientId, (int)Item.ItemType.GhostSoul,
               complete, RpcTarget.Not(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
    }
    [Rpc(SendTo.SpecifiedInParams)]
    private void NotifyItemUsedRPC(ulong userid, int itemid, bool unlockedSomething, RpcParams rpcParams)
    {
        Debug.Log("Notify item used rpc | id" + itemid);
        Item.ItemType type = (Item.ItemType)itemid;
        Debug.Log("Notify item uesd: ghostbool = " + (type == Item.ItemType.GhostSoul));
        string msg;
        string itemname = (type == Item.ItemType.GhostSoul ? "Soul" : "Key");
        string bigItemName = (type == Item.ItemType.GhostSoul ? "Ghost generator" : "Door");
        string actionname = (type == Item.ItemType.GhostSoul ? "fixed" : "unlocked");
        if (unlockedSomething)
            msg = bigItemName + ' ' + actionname;
        else
            msg = "Player " + userid + " gave a " + itemname + " to the " + bigItemName;
        notifText.ShowNotif(msg);
    }
    [Rpc(SendTo.Server)]
    private void UseKeyRPC(NetworkObjectReference door, RpcParams rpcParams = default)
    {
        Debug.Log("UseKeyRPC");
        door.TryGet(out NetworkObject nwobj);
        nwobj.TryGetComponent(out GargoyleDoor gargoyleDoor);
        NotifyItemUsedRPC(rpcParams.Receive.SenderClientId, (int)Item.ItemType.GargoyleKey,
           (--gargoyleDoor.keysNeeded.Value <= 0), RpcTarget.ClientsAndHost);
    }
}
