using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Item : NetworkBehaviour
{
    public enum ItemType
    {
        GhostSoul = 0,
        GargoyleKey = 1
    };
    public ItemType type;
}