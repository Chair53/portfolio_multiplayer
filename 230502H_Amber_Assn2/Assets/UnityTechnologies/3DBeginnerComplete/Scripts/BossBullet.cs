using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BossBullet : NetworkBehaviour
{
    public Vector3 dir;
    [SerializeField] float timer = 0f;

    private void Update()
    {
        transform.position += dir * 5.0f * Time.deltaTime;
        timer += Time.deltaTime;
        if(timer >= 3f && IsServer)
        { Destroy(gameObject);  Debug.Log("Boss bullet destroy"); }
    }
}