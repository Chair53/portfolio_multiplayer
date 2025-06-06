using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.UI;

public class PlayerEmote : NetworkBehaviour
{
    [SerializeField] Image emoteBox;
    private Image[] emoteImgs = new Image[3];

    [Serializable]
    public enum EmoteState
    {
        Emote_None = 0,
        Emote_Yes = 1,
        Emote_No = 2,
        Emote_Help = 3
    };

    public NetworkVariable<EmoteState> currEmote = new(EmoteState.Emote_None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private void Start()
    {
        int index = 0;
        foreach (Transform t in emoteBox.rectTransform)
            emoteImgs[index++] = t.GetComponent<Image>();
        Debug.Log("Player emote start: " + index);
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currEmote.OnValueChanged += OnEmoteChange;
    }

    private void OnEmoteChange(EmoteState prev, EmoteState curr)
    {
        //set the image
        int target = (int)curr;
        for (int i = 0; i < 3; i++)
            emoteImgs[i].enabled = (i == target - 1);
        emoteBox.enabled = (curr != EmoteState.Emote_None);
        Debug.Log("On emote change " + target);
    }
    public void ChangeEmote(EmoteState changeTo)
    {
        if (changeTo == EmoteState.Emote_None)
            currEmote.Value = changeTo;
        else
            StartCoroutine(ShowEmote(changeTo));
    }
    private IEnumerator ShowEmote(EmoteState changeTo)
    {
        currEmote.Value = changeTo;
        yield return new WaitForSecondsRealtime(3);
        currEmote.Value = EmoteState.Emote_None;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if(Input.GetKey(KeyCode.E))
        {
            if (Input.GetKeyUp(KeyCode.Alpha1))
                ChangeEmote(EmoteState.Emote_Yes);
            else if (Input.GetKeyUp(KeyCode.Alpha2))
                ChangeEmote(EmoteState.Emote_No);
            else if (Input.GetKeyUp(KeyCode.Alpha3))
                ChangeEmote(EmoteState.Emote_Help);
        }
    }
}