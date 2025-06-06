using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Boss : NetworkBehaviour
{
    float rotateElapsed = 0f;
    const float rotDur = 2f;
    float pauseElapsed = 0f;
    const float pauseDur = 1.5f;
    [SerializeField] BossBullet bulletPrefab;
    Vector3 bullPos;
    [SerializeField] TMP_Text hpTxt;
    [SerializeField] Item itemDrop;

    public NetworkVariable<int> HP = new(50);
    public NetworkVariable<BossState> state = new(BossState.RotateState);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        bullPos = transform.position;
        bullPos.y = 0.35f;
        HP.OnValueChanged += UpdateHealthRPC;
    }
    private void Update()
    {
        if(state.Value == BossState.RotateState)
        {
            //rotate
            rotateElapsed += Time.deltaTime;
            Vector3 v = transform.rotation.eulerAngles;
            v.y += Time.deltaTime * 10f;
            transform.rotation = Quaternion.Euler(v);
            if(IsServer && rotateElapsed >= rotDur)
            {
                rotateElapsed = 0f;
                state.Value = BossState.AttackState;
            }
        }
        else if(state.Value == BossState.AttackState)
        {
            SpawnBullet();
        }
        else
        {
            pauseElapsed += Time.deltaTime;
            if(IsServer && pauseElapsed >= pauseDur)
            {
                pauseElapsed = 0f;
                state.Value = BossState.RotateState;
            }
        }

    }

    private void SpawnBullet()
    {
        if (!IsServer)
            return;

        BossBullet b = Instantiate(bulletPrefab, bullPos, transform.rotation);
        b.dir = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        b.GetComponent<NetworkObject>().Spawn();
        state.Value = BossState.PauseState;
    }

    [Rpc(SendTo.Server)]
    public void HitByGhostRPC()
    {
        HP.Value -= 10;
        if (HP.Value <= 0)
        {
            //add 50 score and notify
            foreach(NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                client.PlayerObject.TryGetComponent(out PlayerMovement player);
                if (!player) return;
                player.hp.Value += 50;
                player.SetNotif("Boss defeated!");
            }
            //create item on the server
            Instantiate(itemDrop, new Vector3(transform.position.x, 0.5f, transform.position.z),
                transform.rotation).GetComponent<NetworkObject>().Spawn();
            Debug.Log("Destroy boss");
            Destroy(gameObject);
        }
    }

    [Serializable]
    public enum BossState
    {
        RotateState = 0,
        AttackState = 1,
        PauseState = 2,
    };

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateHealthRPC(int prev, int curr)
    {
        hpTxt.text = "hp: " + curr;
    }
}