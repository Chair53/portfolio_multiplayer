using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEditor;

public class PlayerMovement : NetworkBehaviour
{
    public InputAction MoveAction;
    
    public float turnSpeed = 20f;

    Animator m_Animator;
    Rigidbody m_Rigidbody;
    AudioSource m_AudioSource;
    Vector3 m_Movement;
    Quaternion m_Rotation = Quaternion.identity;
    public NetworkVariable<Vector3> Position = new();
    public NetworkVariable<int> PlayerNum = new();
    [SerializeField] TMP_Text hpTxt;
    public Notification notifText;
    [SerializeField] Vector3 assistDetectHalf = new(1, 0.35f, 1);
    public bool caught = false;
    [SerializeField] Image attackRangeUI;
    [SerializeField] bool testAssist = false;

    public NetworkVariable<int> hp = new(100);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            transform.position = new Vector3(-9.8f, 0f, -3.2f);
            CinemachineVirtualCamera virtualCam = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera as CinemachineVirtualCamera;
            if(virtualCam != null)
            {
                virtualCam.Follow = transform;
                virtualCam.LookAt = transform;
            }

        }
        if (NetworkManager.Singleton.IsServer)
            PlayerNum.Value = (int)(OwnerClientId);
        hp.OnValueChanged += NotifHP;
        notifText = GameObject.Find("Notif").GetComponent<Notification>();
    }

    void Start ()
    {
        m_Animator = GetComponent<Animator> ();
        m_Rigidbody = GetComponent<Rigidbody> ();
        m_AudioSource = GetComponent<AudioSource> ();
        
        MoveAction.Enable();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if(caught && Input.GetKeyUp(KeyCode.R))
        {
            caught = false;
            //disable the screen thign
            FindObjectOfType<GameEnding>().caughtBackgroundImageCanvasGroup.alpha = 0;
            Debug.Log("Attempt uncauhgt");
        }

        if (hp.Value <= 0 || caught)
            return;

        if (Input.GetMouseButtonUp(0))
        {
            StartCoroutine(ShowAttackUI());
            AttackRPC(transform.position);
        }

        if(Input.GetKeyUp(KeyCode.H))
        {
            Collider[] colls = Physics.OverlapBox(transform.position, assistDetectHalf);
            foreach(Collider coll in colls)
            {
                if(coll.TryGetComponent(out PlayerMovement otherPlayer) && (otherPlayer.hp.Value <= 0 || testAssist))
                {
                    //assist player
                    AssistRPC(otherPlayer.GetComponent<NetworkObject>());
                    notifText.ShowNotif("Assisted Player");
                }
            }
        }
    }
    void FixedUpdate ()
    {
        if (!IsOwner) return;

        Vector2 pos;

        if (hp.Value <= 0)
            pos = Vector2.zero;
        else
            pos = MoveAction.ReadValue<Vector2>();
        
        float horizontal = pos.x;
        float vertical = pos.y;
        
        m_Movement.Set(horizontal, 0f, vertical);
        m_Movement.Normalize ();
        m_Movement *= 3f;

        bool hasHorizontalInput = !Mathf.Approximately (horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately (vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;
        m_Animator.SetBool ("IsWalking", isWalking);
        
        if (isWalking)
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.Play();
            }
        }
        else
        {
            m_AudioSource.Stop ();
        }

        Vector3 desiredForward = Vector3.RotateTowards (transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
        m_Rotation = Quaternion.LookRotation (desiredForward);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if(other.TryGetComponent(out BossBullet bullet))
        {
            OnBulletHitRPC(bullet.GetComponent<NetworkObject>());
        }
    }

    void OnAnimatorMove ()
    {
        if(!IsOwner) return;
        m_Rigidbody.MovePosition (m_Rigidbody.position + m_Movement * m_Animator.deltaPosition.magnitude);
        m_Rigidbody.MoveRotation (m_Rotation);
    }

    [Rpc(SendTo.Server)]
    void AttackRPC(Vector3 hitCenter, RpcParams rpcParams = default)
    {
        ulong sender = rpcParams.Receive.SenderClientId;

        Collider[] colls = Physics.OverlapBox(hitCenter, new Vector3(2, 2, 2), Quaternion.identity, LayerMask.GetMask("Enemy"));
        foreach (Collider coll in colls)
        {
            if (coll.TryGetComponent<EnemyRPC>(out EnemyRPC enemy))
            {
                enemy.TakeDamage(sender);
            }
        }
    }

    private void NotifHP(int prev, int curr)
    {
        //if(!IsOwner) return;
        Debug.Log("Call hp update");
        if (curr < 0)
            hp.Value = 0;
        hpTxt.text = "HP: " + curr;
    }

    [Rpc(SendTo.Server)]
    private void OnBulletHitRPC(NetworkObjectReference bulletRef, RpcParams rpcParams = default)
    {
        //destroy the bullet, damage the player
        if(bulletRef.TryGet(out NetworkObject nwo))
        {
            Debug.Log("Destroy bullet");
            Destroy(nwo.gameObject);
            ulong sender = rpcParams.Receive.SenderClientId;
            Debug.Log("hitbybullet " + sender);
            NetworkManager.Singleton.ConnectedClients[sender].PlayerObject.GetComponent<PlayerMovement>().hp.Value -= 20;
        }
    }

    [Rpc(SendTo.Server)]
    private void AssistRPC(NetworkObjectReference assistedRef, RpcParams rpcParams = default)
    {
        Debug.Log("AssistRPC");
        if (!assistedRef.TryGet(out NetworkObject nwo))
            return;
        PlayerMovement assistedPlayer = nwo.GetComponent<PlayerMovement>();
        ulong senderID = rpcParams.Receive.SenderClientId;
        assistedPlayer.hp.Value += 40;
        assistedPlayer.SetNotif("Assisted by player " + senderID);
    }

    public void SetNotif(string msg)
    {
        notifText.ShowNotif(msg);
    }

    private IEnumerator ShowAttackUI()
    {
        attackRangeUI.enabled = true;
        yield return new WaitForSeconds(0.5f);
        attackRangeUI.enabled = false;
    }
}