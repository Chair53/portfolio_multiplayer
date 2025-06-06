using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour
{
	TMP_Text txt;
    private void Start()
    {
        txt = GetComponent<TMP_Text>();
    }
    public void ShowNotif(string msg)
	{
		StartCoroutine(Notif3s(msg));
	}
	private IEnumerator Notif3s(string msg)
	{
		txt.text = msg;
		yield return new WaitForSecondsRealtime(3);
		txt.text = "";
	}
}