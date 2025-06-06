using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnding : NetworkBehaviour
{
    public float fadeDuration = 1f;
    public float displayImageDuration = 1f;
    public GameObject player;
    public CanvasGroup exitBackgroundImageCanvasGroup;
    public AudioSource exitAudio;
    public CanvasGroup caughtBackgroundImageCanvasGroup;
    public AudioSource caughtAudio;
    public Notification notifText;

    bool m_IsPlayerAtExit;
    bool m_IsPlayerCaught;
    float m_Timer;
    bool m_HasAudioPlayed;
    
    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.name.Contains("JohnLemon"))
        {
            Debug.Log("game ending trigger enter");
            SomeoneGameOverRPC(true, other.GetComponent<NetworkObject>());
        }
    }

    void Update ()
    {
        //if (m_IsPlayerAtExit)
        //{
        //    //EndLevel (exitBackgroundImageCanvasGroup, false, exitAudio);
        //    SomeoneGameOverRPC(true, other.GetComponent<NetworkObject>());
        //}
        //else if (m_IsPlayerCaught)
        //{
        //    //EndLevel (caughtBackgroundImageCanvasGroup, true, caughtAudio);
        //    SomeoneGameOverRPC(false, other.GetComponent<NetworkObject>());
        //}
    }

    void EndLevel (CanvasGroup imageCanvasGroup, bool doRestart, AudioSource audioSource)
    {
        if (!m_HasAudioPlayed)
        {
            audioSource.Play();
            m_HasAudioPlayed = true;
        }

        imageCanvasGroup.alpha = 1.0f;
    }

    [Rpc(SendTo.Server)]
    public void SomeoneGameOverRPC(bool won, NetworkObjectReference nwo)
    {
        Debug.Log("Server game over");
        nwo.TryGet(out NetworkObject nwobj);
        ulong who = nwobj.GetComponent<PlayerMovement>().OwnerClientId;
        if(!won)
            nwobj.GetComponent<PlayerMovement>().caught = true;
        SpecificGameOverRPC(won, RpcTarget.Single(who, RpcTargetUse.Temp));
        NotifySomeoneGameOverRPC(won, who, RpcTarget.Not(who, RpcTargetUse.Temp));
    }
    [Rpc(SendTo.SpecifiedInParams)]
    private void SpecificGameOverRPC(bool won, RpcParams rpcParams)
    {
        Debug.Log("player game over");
        if(won)
            EndLevel(exitBackgroundImageCanvasGroup, false, exitAudio);
        else
            EndLevel(caughtBackgroundImageCanvasGroup, true, caughtAudio);

    }
    [Rpc(SendTo.SpecifiedInParams)]
    private void NotifySomeoneGameOverRPC(bool won, ulong who, RpcParams rpcparams)
    {
        Debug.Log("someone else game over");
        notifText.ShowNotif("Player " + who + (won ? "escaped" : "died"));
    }
}
