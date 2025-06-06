using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Observer : MonoBehaviour
{
    public Transform player;
    public GameEnding gameEnding;

    bool m_IsPlayerInRange;

    private void Awake()
    {
        gameEnding = FindObjectOfType<GameEnding>();
    }
    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.name.Contains("JohnLemon"))
        {
            Debug.Log("Observer trigger enter");
            if(IsDetected(other.transform))
            {
                Debug.Log("observer detected");
                //gameEnding.caughtBackgroundImageCanvasGroup.gameObject.SetActive(true);
                gameEnding.SomeoneGameOverRPC(false, other.GetComponent<NetworkObject>()); 
            }
        }
    }

    private bool IsDetected(Transform player)
    {
        Vector3 direction = player.position - transform.position + Vector3.up;
        Ray ray = new Ray(transform.position, direction);
        RaycastHit raycastHit;

        return (Physics.Raycast(ray, out raycastHit) && raycastHit.collider.transform == player);
    }
}
